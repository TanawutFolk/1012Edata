using EdataManagement.Controllers;
using EdataManagement.Property;
using EdataManagement.SQLFactory;
using EdataManagement.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EdataManagement.Views.Setting
{
    /// <summary>
    /// "Spec Setting" page: pick product + customer and see how every spec of that customer is sent -
    /// Send Method / File Format / Recipient Group / Subject / Body, one row each in
    /// key_send_methods_ / email_attach_ / key_group_email_ / email_subject_ / email_body_ + product.
    /// Tick specs, pick the values in the middle, Apply; the footer Save writes them together.
    /// The right side shows who receives the mail when it goes by Email (ucSpecSetting.Recipients.cs,
    /// moved in from the old Email Sending page).
    ///
    /// Read from the config DB of App.config (ConnectionQAOC) + the product DB of info_db_name.
    /// Save = one transaction under SET @app_user: recipient changes with their log rows, then the key table
    /// values (logged by the audit triggers of DatabaseScripts\audit_key_tables.sql). Then everything is read again.
    /// </summary>
    public partial class ucSpecSetting : UserControl, ISettingPage
    {
        private const int ALERT_TEXT_GAP = 12;

        // first item of every list on the right: leave that value as each spec has it
        private const int KEEP = -1;

        private const int SEND_DONT_SEND = 0;
        private const int SEND_EMAIL = 3;
        private const int FORMAT_NOTHING = 0;
        private const int SUBJECT_TEST = 0;
        private const int GROUP_INTERNAL = 0;

        // columns of _specs (and of the grid)
        private const string COL_CHECK = "CHECKED";
        private const string COL_KEY = "KEY_ID";
        private const string COL_SPEC = "SPECIFICATION";
        private const string COL_PART = "PART_NO";
        private const string COL_CUS_PN = "CUSTOMER_PN";
        private const string COL_SEND = "SEND_METHODS_ID";
        private const string COL_FORMAT = "FILE_FORMATS_ID";
        private const string COL_GROUP = "GROUP_ID";
        private const string COL_SUBJECT = "SUBJECT_ID";
        private const string COL_BODY = "BODY_ID";
        private const string COL_STATUS = "STATUS";
        private const string COL_LEVEL = "STATUS_LEVEL";
        private const string COL_CHANGED = "CHANGED";

        /// <summary>Prefix of the column that keeps each field as it was loaded.</summary>
        private const string ORIG = "ORIG_";

        private static readonly string[] FIELDS = { COL_SEND, COL_FORMAT, COL_GROUP, COL_SUBJECT, COL_BODY };

        // "Show" filter
        private const string SHOW_ALL = "All specs";
        private const string SHOW_SENDING = "Sending";
        private const string SHOW_DONT_SEND = "Don't Send";
        private const string SHOW_ATTENTION = "Needs attention";
        private const string SHOW_CHANGED = "Changed, not saved";

        // STATUS_LEVEL, worst last
        private const int LEVEL_OFF = 0;
        private const int LEVEL_OK = 1;
        private const int LEVEL_WARN = 2;
        private const int LEVEL_BAD = 3;

        // created on first show, not in the constructor, so the designer can open this control
        private MainControllers _mainCon;
        private SettingSpecControllers _specCon;
        private SettingSpecSQLFactory _specSql;
        private SettingEmailSQLFactory _emailSql;

        private readonly ToolTip toolTipAlert = new ToolTip();
        private readonly Font _gridBold = new Font(SettingUi.FONT_NAME, 9F, FontStyle.Bold);

        // lookup lists: ID, LIST (text in the lists on the right), SHORT (text in the grid)
        private DataTable _sendMethods;
        private DataTable _fileFormats;
        private DataTable _groups;
        private DataTable _subjects;
        private DataTable _bodies;

        /// <summary>Specs of the chosen customer, edited in place until Save.</summary>
        private DataTable _specs;
        private DataView _view;

        private bool _initialized;
        private bool _loading;
        private int _productIndex = -1;
        private int _customerIndex = -1;

        /// <summary>Which specs the alert button ticks for the message on screen.</summary>
        private Func<DataRow, bool> _alertTickRule;

        /// <summary>Shown once on the alert bar (after Apply / Save) instead of the summary.</summary>
        private string _notice = "";

        public event EventHandler SaveButtonTextChanged;

        public ucSpecSetting()
        {
            InitializeComponent();

            cbProduct.SelectedIndexChanged += cbProduct_SelectedIndexChanged;
            cbCustomer.SelectedIndexChanged += cbCustomer_SelectedIndexChanged;
            cbFilter.SelectedIndexChanged += cbFilter_SelectedIndexChanged;
            txtSearch.OnValueChanged += txtSearch_OnValueChanged;
            btnAlertAction.Click += btnAlertAction_Click;
            cbSendMethod.SelectedIndexChanged += cbSendMethod_SelectedIndexChanged;
            cbGroup.SelectedIndexChanged += cbGroup_SelectedIndexChanged;
            btnApply.Click += btnApply_Click;

            WireRecipientEvents();
        }

        private ComboBox[] EditorLists
        {
            get { return new[] { cbSendMethod, cbFileFormat, cbGroup, cbSubject, cbBody }; }
        }

        // =====================================================================
        // ISettingPage
        // =====================================================================
        public string PageTitle
        {
            get { return "Spec Setting"; }
        }

        public string SaveButtonText
        {
            get
            {
                int n = PendingCount();
                return n == 0 ? "Save" : "Save (" + n + ")";
            }
        }

        public bool HasPendingChanges
        {
            get { return PendingCount() > 0; }
        }

        public void PageShown()
        {
            if (_initialized) return;
            _initialized = true;

            _mainCon = new MainControllers();
            _specCon = new SettingSpecControllers();
            _specSql = new SettingSpecSQLFactory();
            _emailSql = new SettingEmailSQLFactory();

            _loading = true;

            SetupSpecGrid();
            SetupRecipients();
            LoadLookups();

            cbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFilter.Items.Clear();
            cbFilter.Items.AddRange(new object[] { SHOW_ALL, SHOW_SENDING, SHOW_DONT_SEND, SHOW_ATTENTION, SHOW_CHANGED });
            cbFilter.SelectedIndex = 0;

            cbProduct.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCustomer.DropDownStyle = ComboBoxStyle.DropDownList;

            // 1480 / 980 / 980Dual / 1480VE with their product database (info_db_name)
            cbProduct.DataSource = _mainCon.getProduct();
            cbProduct.DisplayMember = "PRODUCT_NAME";
            cbProduct.ValueMember = "PRODUCT_ID";

            _loading = false;

            _productIndex = cbProduct.SelectedIndex;
            LoadCustomers();
        }

        /// <summary>Edits stay staged while another tab is open; they are checked again when Setting closes.</summary>
        public bool ConfirmLeavePage()
        {
            return true;
        }

        public void SavePage()
        {
            int specCount = ChangedSpecCount();
            int recipientCount = RecipientChangeCount();

            if (specCount + recipientCount == 0)
            {
                SetAlert(SettingUi.ALERT_NONE_BACK, SettingUi.ALERT_NONE_TEXT, "i", "Nothing has changed.", null, null);
                return;
            }

            if (!frmConfirm.Ask(this, SaveQuestion(specCount, recipientCount), "Save", "Cancel")) return;

            WriteChanges();
        }

        /// <summary>Where it goes and what it does to sending, before anything is written.</summary>
        private string SaveQuestion(int specCount, int recipientCount)
        {
            List<DataRow> changed = ChangedRows();
            int start = changed.Count(r => ToInt(r[ORIG + COL_SEND]) == SEND_DONT_SEND && ToInt(r[COL_SEND]) != SEND_DONT_SEND);
            int stop = changed.Count(r => ToInt(r[ORIG + COL_SEND]) != SEND_DONT_SEND && ToInt(r[COL_SEND]) == SEND_DONT_SEND);

            string text = "Save to " + MailSafety.ConfigDatabase + ": " + specCount + " spec(s) of " + SelectedCustomerName() +
                          ", " + recipientCount + " recipient change(s)?";
            if (start > 0) text += Environment.NewLine + "START sending: " + start + " spec(s)";
            if (stop > 0) text += Environment.NewLine + "STOP sending: " + stop + " spec(s)";
            return text;
        }

        /// <summary>
        /// Build every statement, run them in one transaction, then read everything again.
        /// A failure writes nothing (rollback) and keeps the staged edits on screen.
        /// </summary>
        private bool WriteChanges()
        {
            int specCount = ChangedSpecCount();
            int recipientCount = RecipientChangeCount();
            string user = SettingUi.CurrentUserCode();
            List<string> batch = new List<string>();

            try
            {
                // one connection for the whole Save: the audit triggers pick up who is saving from @app_user
                batch.Add(_emailSql.setAppUser(user));

                Dictionary<int, int> groupIds;
                if (!AddRecipientStatements(batch, user, out groupIds)) return false;
                AddSpecStatements(batch, groupIds);

                batch.Add(_emailSql.clearAppUser());
            }
            catch (Exception ex)
            {
                frmError.ShowError(this, "Save failed, nothing was written." + Environment.NewLine + ex.Message);
                return false;
            }

            if (!_emailCon.saveChanges(batch))
            {
                frmError.ShowError(this, "Save failed, nothing was written." + Environment.NewLine + _emailCon.lastError);
                return false;
            }

            ReloadAfterSave("Saved " + specCount + " spec change(s) and " + recipientCount + " recipient change(s) to " +
                MailSafety.ConfigDatabase + ".");
            return true;
        }

        /// <summary>One upsert per changed field; a group made on this screen gets the id taken at Save.</summary>
        private void AddSpecStatements(List<string> batch, Dictionary<int, int> groupIds)
        {
            string product = SelectedProductName();

            foreach (DataRow row in ChangedRows())
            {
                int keyId = ToInt(row[COL_KEY]);

                foreach (string field in FIELDS)
                {
                    if (!IsFieldChanged(row, field)) continue;

                    int value = ToInt(row[field]);
                    if (field == COL_GROUP) value = MapGroup(groupIds, value);

                    batch.Add(_specSql.saveSpecValue(product, field, keyId, value));
                }
            }
        }

        /// <summary>Everything is read again, so the screen shows what the database now holds.</summary>
        private void ReloadAfterSave(string message)
        {
            ResetRecipients();   // before the lists are rebuilt: it takes the staged groups out of them
            LoadLookups();       // member counts / the new groups
            LoadSpecs();

            SetAlert(SettingUi.ALERT_OK_BACK, SettingUi.ALERT_OK_TEXT, "v", message, null, null);
        }

        public bool CancelPage()
        {
            int n = PendingCount();
            if (n == 0) return false;

            if (!frmConfirm.Ask(this,
                "Discard " + n + " unsaved change(s)?",
                "Discard", "Keep editing")) return true;

            RevertAll();
            return true;
        }

        /// <summary>Changed specs + changed recipients (added / removed / TO-CC, new groups).</summary>
        private int PendingCount()
        {
            return ChangedSpecCount() + RecipientChangeCount();
        }

        private int ChangedSpecCount()
        {
            return ChangedRows().Count;
        }

        /// <summary>Every staged spec, also the ones the Show filter hides.</summary>
        private List<DataRow> ChangedRows()
        {
            if (_specs == null) return new List<DataRow>();
            return _specs.Rows.Cast<DataRow>().Where(r => (bool)r[COL_CHANGED]).ToList();
        }

        private void RaiseSaveButtonTextChanged()
        {
            EventHandler handler = SaveButtonTextChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>Ask before switching product / customer while edits are staged.</summary>
        private bool ConfirmDropPendingChanges()
        {
            int n = PendingCount();
            if (n == 0) return true;

            return frmConfirm.Ask(this,
                "You have " + n + " unsaved change(s). Discard them?",
                "Discard", "Keep editing");
        }

        private void RevertAll()
        {
            if (_specs == null) return;

            foreach (DataRow row in _specs.Rows)
            {
                foreach (string field in FIELDS)
                {
                    row[field] = row[ORIG + field];
                }
                EvaluateRow(row);
            }

            ResetRecipients();
            RefreshAll();
        }

        // =====================================================================
        // product / customer / filter
        // =====================================================================
        private void cbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (!ConfirmDropPendingChanges())
            {
                RestoreIndex(cbProduct, _productIndex);
                return;
            }

            _productIndex = cbProduct.SelectedIndex;
            LoadCustomers();
        }

        private void cbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (!ConfirmDropPendingChanges())
            {
                RestoreIndex(cbCustomer, _customerIndex);
                return;
            }

            _customerIndex = cbCustomer.SelectedIndex;
            LoadSpecs();
        }

        private void RestoreIndex(ComboBox combo, int index)
        {
            _loading = true;
            combo.SelectedIndex = index;
            _loading = false;
        }

        private string SelectedProductName()
        {
            DataRowView selected = cbProduct.SelectedItem as DataRowView;
            return selected == null ? "" : selected["PRODUCT_NAME"].ToString();
        }

        private string SelectedCustomerName()
        {
            DataRowView selected = cbCustomer.SelectedItem as DataRowView;
            return selected == null ? "" : selected["CUSTOMER_NAME"].ToString();
        }

        private void LoadCustomers()
        {
            string product = SelectedProductName();
            if (product == "") return;

            DataTable customers = _specCon.searchCustomers(BuildProperty(""));
            string error = _specCon.lastError;

            _loading = true;
            cbCustomer.DataSource = null;

            if (customers.Rows.Count == 0)
            {
                _loading = false;
                _customerIndex = -1;
                ShowSpecs(null);
                SetAlert(SettingUi.ALERT_NONE_BACK, SettingUi.ALERT_NONE_TEXT, "i",
                    error != "" ? "Could not read the customers of " + product + "." : "No customer of " + product + " has an in-use spec.",
                    null, null);
                if (error != "") frmError.ShowError(this, error);
                return;
            }

            cbCustomer.DataSource = customers;
            cbCustomer.DisplayMember = "CUSTOMER_NAME";
            cbCustomer.ValueMember = "CUSTOMER_NAME";
            _loading = false;

            _customerIndex = cbCustomer.SelectedIndex;
            LoadSpecs();
        }

        private void LoadSpecs()
        {
            string customer = SelectedCustomerName();
            if (customer == "")
            {
                ShowSpecs(null);
                return;
            }

            DataTable specs = _specCon.searchSpecs(BuildProperty(customer));
            if (_specCon.lastError != "")
            {
                ShowSpecs(null);
                frmError.ShowError(this, "Could not read the specs of " + customer + ":" + Environment.NewLine + _specCon.lastError);
                return;
            }

            ShowSpecs(specs);
        }

        private SettingSpecProperty BuildProperty(string customer)
        {
            DataRowView product = cbProduct.SelectedItem as DataRowView;

            return new SettingSpecProperty
            {
                productName = product == null ? "" : product["PRODUCT_NAME"].ToString(),
                dbProduct = product == null ? "" : product["DATABASE_NAME"].ToString(),
                customerName = customer
            };
        }

        // =====================================================================
        // lists of the editor
        // =====================================================================
        /// <summary>Read the five lists and give every row the text of the list (LIST) and of the grid (SHORT).</summary>
        private void LoadLookups()
        {
            bool wasLoading = _loading;
            _loading = true;

            _sendMethods = NewLookup();
            foreach (DataRow row in ReadList(_specCon.searchSendMethods()).Rows)
            {
                // "Email,Attach File(s)" -> list "Email, Attach File(s)", grid "Email"
                string name = Convert.ToString(row["NAME"]).Trim();
                _sendMethods.Rows.Add(ToInt(row["ID"]), name.Replace(",", ", ").Replace(",  ", ", "), name.Split(',')[0].Trim());
            }

            _fileFormats = NewLookup();
            foreach (DataRow row in ReadList(_specCon.searchFileFormats()).Rows)
            {
                int id = ToInt(row["ID"]);
                string name = Convert.ToString(row["NAME"]).Trim();
                string details = Convert.ToString(row["DETAILS"]).Trim();
                string list = id + " - " + name + (details == "" || details == name ? "" : "  (" + details + ")");
                _fileFormats.Rows.Add(id, list, id + " - " + name);
            }

            _subjects = TextLookup(ReadList(_specCon.searchSubjects()));
            _bodies = TextLookup(ReadList(_specCon.searchBodies()));

            _groups = NewLookup();
            foreach (DataRow row in ReadList(_specCon.searchSpecGroups()).Rows)
            {
                int id = ToInt(row["ID"]);
                _groups.Rows.Add(id, GroupListText(id, ToInt(row["MEMBERS"]), false), "#" + id);
            }

            BindEditorList(cbSendMethod, _sendMethods);
            BindEditorList(cbFileFormat, _fileFormats);
            BindEditorList(cbGroup, _groups);
            BindEditorList(cbSubject, _subjects);
            BindEditorList(cbBody, _bodies);

            _loading = wasLoading;
        }

        /// <summary>A failed read shows its error once and gives an empty list.</summary>
        private DataTable ReadList(DataTable result)
        {
            if (_specCon.lastError != "")
            {
                frmError.ShowError(this, "Could not read a Setting list:" + Environment.NewLine + _specCon.lastError);
            }
            return result;
        }

        private static DataTable NewLookup()
        {
            DataTable table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("LIST", typeof(string));
            table.Columns.Add("SHORT", typeof(string));
            return table;
        }

        /// <summary>Subjects / bodies: "7 - LD data ..." in the list, "#7" in the grid.</summary>
        private static DataTable TextLookup(DataTable source)
        {
            DataTable table = NewLookup();
            foreach (DataRow row in source.Rows)
            {
                int id = ToInt(row["ID"]);
                string text = System.Text.RegularExpressions.Regex.Replace(
                    Convert.ToString(row["NAME"]).Replace("<BR>", " ").Replace("<br>", " "), @"\s+", " ").Trim();
                bool inUse = !source.Columns.Contains("INUSE") || ToInt(row["INUSE"]) != 0;

                table.Rows.Add(id, id + " - " + text + (inUse ? "" : "  (not in use)"), "#" + id);
            }
            return table;
        }

        /// <summary>Add the working columns (tick, original values, status) and put the specs on the grid.</summary>
        private void ShowSpecs(DataTable loaded)
        {
            _specs = null;

            // staged recipient edits belong to the customer being left (the switch was confirmed)
            ResetRecipients();

            if (loaded != null)
            {
                loaded.Columns.Add(COL_CHECK, typeof(bool));
                foreach (string field in FIELDS)
                {
                    loaded.Columns.Add(ORIG + field, typeof(int));
                }
                loaded.Columns.Add(COL_STATUS, typeof(string));
                loaded.Columns.Add(COL_LEVEL, typeof(int));
                loaded.Columns.Add(COL_CHANGED, typeof(bool));

                foreach (DataRow row in loaded.Rows)
                {
                    row[COL_CHECK] = false;
                    foreach (string field in FIELDS)
                    {
                        row[ORIG + field] = row[field];
                    }
                    EvaluateRow(row);
                }

                loaded.AcceptChanges();
                _specs = loaded;
            }

            _view = _specs == null ? null : new DataView(_specs);
            dtgSpecs.DataSource = _view;
            ApplyFilter();

            RefreshAll();
        }

        private void cbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            ApplyFilter();
            UpdateEditor();
        }

        private void txtSearch_OnValueChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            ApplyFilter();
            UpdateEditor();
        }

        private void ApplyFilter()
        {
            if (_view == null) return;

            _view.RowFilter = BuildRowFilter();
            UntickHidden();
            dtgSpecs.ClearSelection();
        }

        private string BuildRowFilter()
        {
            List<string> parts = new List<string>();

            string show = Convert.ToString(cbFilter.SelectedItem);
            if (show == SHOW_SENDING) parts.Add(COL_SEND + " <> " + SEND_DONT_SEND);
            else if (show == SHOW_DONT_SEND) parts.Add(COL_SEND + " = " + SEND_DONT_SEND);
            else if (show == SHOW_ATTENTION) parts.Add(COL_LEVEL + " >= " + LEVEL_WARN);
            else if (show == SHOW_CHANGED) parts.Add(COL_CHANGED + " = true");

            string search = (txtSearch.Text ?? "").Trim();
            if (search != "")
            {
                string like = " LIKE '%" + EscapeLike(search) + "%'";
                parts.Add("(" + COL_SPEC + like + " OR " + COL_PART + like + " OR " + COL_CUS_PN + like + ")");
            }

            return string.Join(" AND ", parts);
        }

        /// <summary>DataView LIKE: quote ' and wrap the wildcard characters in brackets.</summary>
        private static string EscapeLike(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c == '[' || c == ']' || c == '*' || c == '%') sb.Append('[').Append(c).Append(']');
                else if (c == '\'') sb.Append("''");
                else sb.Append(c);
            }
            return sb.ToString();
        }

        // =====================================================================
        // spec grid
        // =====================================================================
        private void SetupSpecGrid()
        {
            SettingUi.StyleGrid(dtgSpecs, 30, 9F, 34);
            dtgSpecs.AutoGenerateColumns = false;

            // 9 columns in 668 px: a little less padding than the other grids
            dtgSpecs.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            dtgSpecs.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            dtgSpecs.Columns.Clear();

            DataGridViewCheckBoxColumn check = new DataGridViewCheckBoxColumn();
            check.Name = COL_CHECK;
            check.DataPropertyName = COL_CHECK;
            check.HeaderText = "ALL";
            check.ReadOnly = true;
            check.SortMode = DataGridViewColumnSortMode.NotSortable;
            check.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            check.Width = 36;
            check.Resizable = DataGridViewTriState.False;
            check.DefaultCellStyle.Padding = new Padding(0);
            check.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            check.HeaderCell.Style.Padding = new Padding(0);
            check.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dtgSpecs.Columns.Add(check);

            // weights ~ pixels needed for the usual longest value ("Y-3CJ4021H", "FOL1439R50-867-1450", "No file format")
            AddTextColumn(COL_SPEC, "SPEC", 86, false);
            AddTextColumn(COL_PART, "PART NO", 140, false);
            AddTextColumn(COL_SEND, "SEND", 78, false);
            AddTextColumn(COL_FORMAT, "FORMAT", 88, false);
            AddTextColumn(COL_GROUP, "GROUP", 54, true);
            AddTextColumn(COL_SUBJECT, "SUBJ.", 44, true);
            AddTextColumn(COL_BODY, "BODY", 44, true);
            AddTextColumn(COL_STATUS, "STATUS", 96, false);

            dtgSpecs.CellFormatting += dtgSpecs_CellFormatting;
            dtgSpecs.CellClick += dtgSpecs_CellClick;
            dtgSpecs.ColumnHeaderMouseClick += dtgSpecs_ColumnHeaderMouseClick;
            dtgSpecs.CellToolTipTextNeeded += dtgSpecs_CellToolTipTextNeeded;
            dtgSpecs.KeyDown += dtgSpecs_KeyDown;
            dtgSpecs.DataBindingComplete += delegate { dtgSpecs.ClearSelection(); };
        }

        private void AddTextColumn(string name, string header, int weight, bool narrow)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.DataPropertyName = name;
            column.HeaderText = header;
            column.FillWeight = weight;
            column.ReadOnly = true;
            column.SortMode = DataGridViewColumnSortMode.Automatic;

            if (narrow)
            {
                column.DefaultCellStyle.Padding = new Padding(4, 0, 0, 0);
                column.HeaderCell.Style.Padding = new Padding(4, 0, 0, 0);
            }

            dtgSpecs.Columns.Add(column);
        }

        private DataRow RowAt(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dtgSpecs.Rows.Count) return null;

            DataRowView item = dtgSpecs.Rows[rowIndex].DataBoundItem as DataRowView;
            return item == null ? null : item.Row;
        }

        /// <summary>
        /// Ids shown as short text, changed values in bold blue, Don't Send rows in grey (their
        /// file / group / subject / body are not used, so "-"), ticked rows tinted.
        /// The grid's own selection color is hidden - the tick is the selection.
        /// </summary>
        private void dtgSpecs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataRow row = RowAt(e.RowIndex);
            if (row == null) return;

            string column = dtgSpecs.Columns[e.ColumnIndex].Name;
            bool dontSend = ToInt(row[COL_SEND]) == SEND_DONT_SEND;

            Color back = IsTicked(row) ? SettingUi.GRID_SELECT_BACK
                : (e.RowIndex % 2 == 1 ? SettingUi.GRID_ALT_ROW : Color.White);
            Color fore = dontSend ? SettingUi.ALERT_NONE_TEXT : SettingUi.GRID_TEXT;
            bool bold = false;

            DataTable lookup = LookupOf(column);
            if (lookup != null)
            {
                bool changed = IsFieldChanged(row, column);

                e.Value = (dontSend && column != COL_SEND && !changed) ? "-" : ShortText(lookup, row[column]);
                e.FormattingApplied = true;

                if (changed)
                {
                    fore = SettingUi.TAB_ACTIVE_BACK;
                    bold = true;
                }
            }
            else if (column == COL_STATUS)
            {
                fore = LevelColor(ToInt(row[COL_LEVEL]));
                bold = true;
            }

            e.CellStyle.BackColor = back;
            e.CellStyle.SelectionBackColor = back;
            e.CellStyle.ForeColor = fore;
            e.CellStyle.SelectionForeColor = fore;
            if (bold) e.CellStyle.Font = _gridBold;
        }

        private void dtgSpecs_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            DataRow row = RowAt(e.RowIndex);
            if (row == null || e.ColumnIndex < 0) return;

            string column = dtgSpecs.Columns[e.ColumnIndex].Name;
            DataTable lookup = LookupOf(column);

            if (lookup != null)
            {
                e.ToolTipText = ListText(lookup, row[column]);
                if (column != COL_SEND && ToInt(row[COL_SEND]) == SEND_DONT_SEND)
                {
                    e.ToolTipText = "Not used while Don't Send. Stored: " + e.ToolTipText;
                }
                if (IsFieldChanged(row, column))
                {
                    e.ToolTipText += Environment.NewLine + "was: " + ListText(lookup, row[ORIG + column]);
                }
            }
            else if (column == COL_SPEC || column == COL_PART)
            {
                e.ToolTipText = "KEY_ID " + row[COL_KEY] + "   Customer PN: " + Convert.ToString(row[COL_CUS_PN]);
            }
            else if (column == COL_STATUS)
            {
                e.ToolTipText = StatusHelp(row);
            }
        }

        /// <summary>A click anywhere on a row ticks / unticks it.</summary>
        private void dtgSpecs_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataRow row = RowAt(e.RowIndex);
            if (row == null) return;

            row[COL_CHECK] = !IsTicked(row);
            AfterTickChanged();
        }

        private void dtgSpecs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Space || dtgSpecs.CurrentRow == null) return;

            DataRow row = RowAt(dtgSpecs.CurrentRow.Index);
            if (row == null) return;

            row[COL_CHECK] = !IsTicked(row);
            e.Handled = true;
            AfterTickChanged();
        }

        /// <summary>The ALL header ticks every spec on screen, or unticks them when all are ticked already.</summary>
        private void dtgSpecs_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dtgSpecs.Columns[e.ColumnIndex].Name != COL_CHECK) return;

            List<DataRow> shown = ShownRows();
            bool tick = shown.Any(r => !IsTicked(r));

            foreach (DataRow row in shown)
            {
                row[COL_CHECK] = tick;
            }
            AfterTickChanged();
        }

        private void AfterTickChanged()
        {
            dtgSpecs.Invalidate();
            UpdateEditor();
        }

        private List<DataRow> ShownRows()
        {
            if (_view == null) return new List<DataRow>();
            return _view.Cast<DataRowView>().Select(v => v.Row).ToList();
        }

        private List<DataRow> TickedRows()
        {
            return ShownRows().Where(IsTicked).ToList();
        }

        /// <summary>A ticked spec hidden by the filter would be changed by Apply without being seen.</summary>
        private void UntickHidden()
        {
            if (_specs == null) return;

            HashSet<DataRow> shown = new HashSet<DataRow>(ShownRows());
            foreach (DataRow row in _specs.Rows)
            {
                if (IsTicked(row) && !shown.Contains(row)) row[COL_CHECK] = false;
            }
        }

        // =====================================================================
        // editor (right side)
        // =====================================================================
        private void BindEditorList(ComboBox combo, DataTable lookup)
        {
            DataTable list = new DataTable();
            list.Columns.Add("ID", typeof(int));
            list.Columns.Add("LIST", typeof(string));
            list.Rows.Add(KEEP, "(keep as is)");
            foreach (DataRow row in lookup.Rows)
            {
                list.Rows.Add(row["ID"], row["LIST"]);
            }

            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.DataSource = list;
            combo.DisplayMember = "LIST";
            combo.ValueMember = "ID";

            // long subjects / bodies are readable in the open list
            int widest = combo.Width;
            foreach (DataRow row in list.Rows)
            {
                widest = Math.Max(widest, TextRenderer.MeasureText(row["LIST"].ToString(), combo.Font).Width + 24);
            }
            combo.DropDownWidth = Math.Min(widest, 560);
        }

        private void UpdateEditor()
        {
            List<DataRow> ticked = TickedRows();
            int n = ticked.Count;

            _loading = true;

            if (n == 0)
            {
                lbEditorTitle.Text = _specs == null ? "No spec" : "No spec ticked";
                lbEditorHint.Text = "Tick specs on the left (ALL = every spec shown), pick values, then Apply.";
                foreach (ComboBox combo in EditorLists)
                {
                    combo.SelectedValue = KEEP;
                }
                btnApply.ButtonText = "Apply";
            }
            else
            {
                if (n == 1)
                {
                    lbEditorTitle.Text = ticked[0][COL_SPEC] + "  " + ticked[0][COL_PART];
                    lbEditorHint.Text = "Change a value, then Apply.";
                }
                else
                {
                    lbEditorTitle.Text = n + " specs ticked";
                    lbEditorHint.Text = "\"(keep as is)\" = the ticked specs differ there. Leave it to keep each one.";
                }

                ShowCommonValue(cbSendMethod, ticked, COL_SEND);
                ShowCommonValue(cbFileFormat, ticked, COL_FORMAT);
                ShowCommonValue(cbGroup, ticked, COL_GROUP);
                ShowCommonValue(cbSubject, ticked, COL_SUBJECT);
                ShowCommonValue(cbBody, ticked, COL_BODY);

                btnApply.ButtonText = "Apply To " + n + (n == 1 ? " Spec" : " Specs");
            }

            _loading = false;

            RefreshEditorEnabled();
            RefreshRecipients();
        }

        /// <summary>The value all ticked specs share, or "(keep as is)" when they differ.</summary>
        private void ShowCommonValue(ComboBox combo, List<DataRow> ticked, string field)
        {
            List<int> values = ticked.Select(r => ToInt(r[field])).Distinct().ToList();

            combo.SelectedValue = KEEP;
            if (values.Count == 1) combo.SelectedValue = values[0];

            // an id missing from the list (e.g. BODY_ID 0) leaves nothing selected
            if (combo.SelectedIndex < 0) combo.SelectedValue = KEEP;
        }

        /// <summary>Don't Send needs no file / group / subject / body, so those lists rest.</summary>
        private void RefreshEditorEnabled()
        {
            bool any = TickedRows().Count > 0;
            bool dontSend = SelectedId(cbSendMethod) == SEND_DONT_SEND;

            cbSendMethod.Enabled = any;
            cbFileFormat.Enabled = any && !dontSend;
            cbGroup.Enabled = any && !dontSend;
            cbSubject.Enabled = any && !dontSend;
            cbBody.Enabled = any && !dontSend;
            SetButtonEnabled(btnApply, any, SettingUi.BUTTON_BACK);
        }

        private void cbSendMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            RefreshEditorEnabled();
            RefreshRecipients();
        }

        /// <summary>Picking a group shows its members on the right before it is applied.</summary>
        private void cbGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            RefreshRecipients();
        }

        private static int SelectedId(ComboBox combo)
        {
            return combo.SelectedValue == null ? KEEP : ToInt(combo.SelectedValue);
        }

        /// <summary>Stage the picked values on the ticked specs; Save writes them.</summary>
        private void btnApply_Click(object sender, EventArgs e)
        {
            List<DataRow> ticked = TickedRows();
            if (ticked.Count == 0) return;

            Dictionary<string, int> picked = new Dictionary<string, int>();
            AddPicked(picked, COL_SEND, cbSendMethod);
            AddPicked(picked, COL_FORMAT, cbFileFormat);
            AddPicked(picked, COL_GROUP, cbGroup);
            AddPicked(picked, COL_SUBJECT, cbSubject);
            AddPicked(picked, COL_BODY, cbBody);

            if (picked.Count == 0)
            {
                SetAlert(SettingUi.ALERT_NONE_BACK, SettingUi.ALERT_NONE_TEXT, "i",
                    "Every list says (keep as is) - pick at least one value to change.", null, null);
                return;
            }

            int changed = 0;
            foreach (DataRow row in ticked)
            {
                bool rowChanged = false;
                foreach (KeyValuePair<string, int> value in picked)
                {
                    if (ToInt(row[value.Key]) == value.Value) continue;

                    row[value.Key] = value.Value;
                    rowChanged = true;
                }

                if (rowChanged)
                {
                    changed++;
                    EvaluateRow(row);
                }
            }

            _notice = changed == 0
                ? "Nothing to apply: the ticked specs already have these values."
                : "Applied to " + changed + " spec(s) - Save writes them, Cancel undoes.";

            UntickHidden();
            RefreshAll();
        }

        private void AddPicked(Dictionary<string, int> picked, string field, ComboBox combo)
        {
            if (!combo.Enabled) return;

            int id = SelectedId(combo);
            if (id != KEEP) picked[field] = id;
        }

        // =====================================================================
        // status + alert
        // =====================================================================
        /// <summary>What happens to this spec when it ships, worst problem first.</summary>
        private void EvaluateRow(DataRow row)
        {
            int send = ToInt(row[COL_SEND]);
            string status;
            int level;

            if (send == SEND_DONT_SEND) { status = "Don't Send"; level = LEVEL_OFF; }
            else if (send != SEND_EMAIL) { status = "Ready"; level = LEVEL_OK; }
            else if (ToInt(row[COL_FORMAT]) == FORMAT_NOTHING || FindId(_fileFormats, row[COL_FORMAT]) == null) { status = "No file format"; level = LEVEL_BAD; }
            else if (ToInt(row[COL_SUBJECT]) == SUBJECT_TEST || FindId(_subjects, row[COL_SUBJECT]) == null) { status = "Subject is Test"; level = LEVEL_BAD; }
            else if (FindId(_bodies, row[COL_BODY]) == null) { status = "No body"; level = LEVEL_BAD; }
            else if (FindId(_groups, row[COL_GROUP]) == null) { status = "No group"; level = LEVEL_BAD; }
            else if (ToInt(row[COL_GROUP]) == GROUP_INTERNAL) { status = "Internal only"; level = LEVEL_WARN; }
            else { status = "Ready"; level = LEVEL_OK; }

            row[COL_STATUS] = status;
            row[COL_LEVEL] = level;
            row[COL_CHANGED] = FIELDS.Any(f => IsFieldChanged(row, f));
        }

        private string StatusHelp(DataRow row)
        {
            switch (Convert.ToString(row[COL_STATUS]))
            {
                case "Don't Send": return "This spec is not sent.";
                case "No file format": return "Send is Email but File Format is 0 (Nothing): the mail would go out without the e-data.";
                case "Subject is Test": return "Send is Email but the subject is #0 \"Test\".";
                case "No body": return "Send is Email but BODY_ID " + row[COL_BODY] + " is not in info_email_body.";
                case "No group": return "Send is Email but GROUP_ID " + row[COL_GROUP] + " is not in info_group_email.";
                case "Internal only": return "Group #0 = internal addresses only, the customer gets nothing.";
                default: return "Ready to send.";
            }
        }

        private static Color LevelColor(int level)
        {
            if (level == LEVEL_BAD) return SettingUi.NG_TEXT;
            if (level == LEVEL_WARN) return SettingUi.ALERT_WARN_TEXT;
            if (level == LEVEL_OK) return SettingUi.ALERT_OK_TEXT;
            return SettingUi.ALERT_NONE_TEXT;
        }

        private void RefreshAll()
        {
            dtgSpecs.Invalidate();
            UpdateEditor();
            RefreshAlert();
            RaiseSaveButtonTextChanged();
        }

        /// <summary>One line about the customer, the most important thing first.</summary>
        private void RefreshAlert()
        {
            if (_specs == null) return;

            List<DataRow> all = _specs.Rows.Cast<DataRow>().ToList();
            int total = all.Count;
            int bad = all.Count(r => ToInt(r[COL_LEVEL]) == LEVEL_BAD);
            int warn = all.Count(r => ToInt(r[COL_LEVEL]) == LEVEL_WARN);
            int dontSend = all.Count(r => ToInt(r[COL_SEND]) == SEND_DONT_SEND);
            int sending = total - dontSend;
            string customer = SelectedCustomerName();

            // a spec that would go out wrong outranks the "Applied" notice (Save (n) shows the staging anyway)
            string notice = _notice;
            _notice = "";

            if (bad > 0)
            {
                SetAlert(SettingUi.ALERT_WARN_BACK, SettingUi.ALERT_WARN_TEXT, "!",
                    bad + " of " + total + " specs are set to send but miss a value (see STATUS).",
                    "Tick Them (" + bad + ")", r => ToInt(r[COL_LEVEL]) == LEVEL_BAD);
            }
            else if (notice != "")
            {
                SetAlert(SettingUi.ALERT_INFO_BACK, SettingUi.ALERT_INFO_TEXT, "i", notice, null, null);
            }
            else if (sending == 0)
            {
                SetAlert(SettingUi.ALERT_NONE_BACK, SettingUi.ALERT_NONE_TEXT, "i",
                    customer + " is Don't Send on all " + total + " specs. Tick them and pick a Send Method to start sending.",
                    "Tick All (" + total + ")", r => true);
            }
            else if (dontSend > 0)
            {
                SetAlert(SettingUi.ALERT_INFO_BACK, SettingUi.ALERT_INFO_TEXT, "i",
                    sending + " of " + total + " specs send, the other " + dontSend + " are Don't Send. A new spec that should send too?",
                    "Tick Don't Send (" + dontSend + ")", r => ToInt(r[COL_SEND]) == SEND_DONT_SEND);
            }
            else if (warn > 0)
            {
                SetAlert(SettingUi.ALERT_WARN_BACK, SettingUi.ALERT_WARN_TEXT, "!",
                    warn + " spec(s) send to internal group #0 only - the customer gets nothing.",
                    "Tick Them (" + warn + ")", r => ToInt(r[COL_LEVEL]) == LEVEL_WARN);
            }
            else
            {
                SetAlert(SettingUi.ALERT_OK_BACK, SettingUi.ALERT_OK_TEXT, "v",
                    "All " + total + " specs of " + customer + " are ready" + CommonSetup(all) + ".", null, null);
            }
        }

        /// <summary>" (Email, group #6)" when every spec is sent the same way, else nothing.</summary>
        private string CommonSetup(List<DataRow> rows)
        {
            List<int> sends = rows.Select(r => ToInt(r[COL_SEND])).Distinct().ToList();
            if (sends.Count != 1) return "";

            string text = " (" + ShortText(_sendMethods, sends[0]);

            List<int> groups = rows.Select(r => ToInt(r[COL_GROUP])).Distinct().ToList();
            if (sends[0] == SEND_EMAIL && groups.Count == 1) text += ", group #" + groups[0];

            return text + ")";
        }

        private void SetAlert(Color back, Color fore, string icon, string message, string actionText, Func<DataRow, bool> tickRule)
        {
            bool showAction = actionText != null && tickRule != null;

            pnAlert.BackColor = back;
            lbAlertIcon.ForeColor = fore;
            lbAlertIcon.Text = icon;
            lbAlertText.ForeColor = fore;
            btnAlertAction.Visible = showAction;
            _alertTickRule = showAction ? tickRule : null;

            if (showAction)
            {
                btnAlertAction.ButtonText = actionText;
            }

            // the label must stop before the action button, otherwise the text runs underneath it
            int rightEdge = showAction
                ? btnAlertAction.Left - ALERT_TEXT_GAP
                : pnAlert.ClientSize.Width - ALERT_TEXT_GAP;

            lbAlertText.Width = Math.Max(120, rightEdge - lbAlertText.Left);
            lbAlertText.Text = message;

            toolTipAlert.SetToolTip(lbAlertText, message);
            toolTipAlert.SetToolTip(lbAlertIcon, message);
        }

        /// <summary>Tick exactly the specs the alert talks about (showing all specs first).</summary>
        private void btnAlertAction_Click(object sender, EventArgs e)
        {
            if (_alertTickRule == null || _specs == null) return;

            _loading = true;
            cbFilter.SelectedItem = SHOW_ALL;
            txtSearch.Text = "";
            _loading = false;
            ApplyFilter();

            foreach (DataRow row in _specs.Rows)
            {
                row[COL_CHECK] = _alertTickRule(row);
            }
            AfterTickChanged();
        }

        // =====================================================================
        // helpers
        // =====================================================================
        private DataTable LookupOf(string column)
        {
            if (column == COL_SEND) return _sendMethods;
            if (column == COL_FORMAT) return _fileFormats;
            if (column == COL_GROUP) return _groups;
            if (column == COL_SUBJECT) return _subjects;
            if (column == COL_BODY) return _bodies;
            return null;
        }

        /// <summary>
        /// Grey a BunifuFlatButton. Its Enabled hides Control.Enabled and only repaints - the click still
        /// fires (disabling the base control draws the caption in the wrong place), so every click handler
        /// here checks for itself that it may act.
        /// </summary>
        private static void SetButtonEnabled(Bunifu.Framework.UI.BunifuFlatButton button, bool enabled, Color back)
        {
            button.Enabled = enabled;
            button.BackColor = enabled ? back : Color.Gray;
            button.Normalcolor = button.BackColor;
        }

        private static bool IsTicked(DataRow row)
        {
            return row[COL_CHECK] is bool && (bool)row[COL_CHECK];
        }

        private static bool IsFieldChanged(DataRow row, string field)
        {
            return ToInt(row[field]) != ToInt(row[ORIG + field]);
        }

        private static int ToInt(object value)
        {
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private static DataRow FindId(DataTable lookup, object id)
        {
            if (lookup == null) return null;

            int value = ToInt(id);
            foreach (DataRow row in lookup.Rows)
            {
                if (ToInt(row["ID"]) == value) return row;
            }
            return null;
        }

        private static string ShortText(DataTable lookup, object id)
        {
            DataRow row = FindId(lookup, id);
            return row == null ? "#" + ToInt(id) + " ?" : row["SHORT"].ToString();
        }

        private static string ListText(DataTable lookup, object id)
        {
            DataRow row = FindId(lookup, id);
            return row == null ? "#" + ToInt(id) + " (not in the list)" : row["LIST"].ToString();
        }
    }
}
