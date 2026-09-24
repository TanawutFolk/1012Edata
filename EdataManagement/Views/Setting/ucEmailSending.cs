using EdataManagement.Controllers;
using EdataManagement.Property;
using EdataManagement.SQLFactory;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EdataManagement.Views.Setting
{
    /// <summary>
    /// "Email Recipients / Group" page: pick product + customer, see and edit who receives its e-data.
    /// Add / Remove / TO-CC / Split are staged and written together by the footer Save button.
    /// </summary>
    public partial class ucEmailSending : UserControl, ISettingPage
    {
        // alert panel: gap kept between the message and the action button,
        // and how many customer names are spelled out before counting the rest
        private const int ALERT_TEXT_GAP = 12;
        private const int ALERT_MAX_NAMES = 3;

        private const string COL_REMOVE = "colRemove";
        private const string COL_TYPE = "RECIPIENT_TYPE";

        private readonly ToolTip toolTipAlert = new ToolTip();

        // created on first show, not in the constructor, so the designer can open this control
        private MainControllers _mainCon;
        private SettingEmailControllers _setCon;
        private SettingEmailSQLFactory _sqlFactory;

        /// <summary>Statements staged by the user, committed together on Save.</summary>
        private readonly List<string> _pendingSql = new List<string>();

        /// <summary>Grid rows the user is editing (not written to the database yet).</summary>
        private DataTable _members;

        private bool _initialized;
        private bool _loading;

        /// <summary>Shown on the alert bar after a successful save, instead of a popup.</summary>
        private string _savedNotice = "";
        private int _currentGroupId = -1;

        // info_document ID of the E-data type shown; 1 = Edata (LD data) uses the group of the specs
        private const int DOC_LD_DATA = 1;
        private int _edataTypeIndex = 0;
        private int _nextEmailId = -1;
        private int _nextGroupId = -1;

        public event EventHandler SaveButtonTextChanged;

        public ucEmailSending()
        {
            InitializeComponent();

            // the designer lost these when the controls were moved over from frmSetting
            cbProduct.SelectedIndexChanged += cbProduct_SelectedIndexChanged;
            cbCustomer.SelectedIndexChanged += cbCustomer_SelectedIndexChanged;
            btnAllGroupDetails.Click += btnAllGroupDetails_Click;
            btnAddToGroup.Click += btnAddToGroup_Click;
            btnAlertAction.Click += btnAlertAction_Click;
            dtgShowEmailAll.CellContentClick += dtgShowEmailAll_CellContentClick;
            cbEdataType.SelectedIndexChanged += cbEdataType_SelectedIndexChanged;
        }

        // =====================================================================
        // ISettingPage
        // =====================================================================
        public string PageTitle
        {
            get { return "Email Sending Setting"; }
        }

        public string SaveButtonText
        {
            get { return _pendingSql.Count == 0 ? "Save" : "Save (" + _pendingSql.Count + ")"; }
        }

        public bool HasPendingChanges
        {
            get { return _pendingSql.Count > 0; }
        }

        public void PageShown()
        {
            if (_initialized) return;
            _initialized = true;

            _mainCon = new MainControllers();
            _setCon = new SettingEmailControllers();
            _sqlFactory = new SettingEmailSQLFactory();

            _loading = true;

            SetupMemberGrid();

            cbToOrCC.DropDownStyle = ComboBoxStyle.DropDownList;
            cbToOrCC.Items.Clear();
            cbToOrCC.Items.Add("TO");
            cbToOrCC.Items.Add("CC");
            cbToOrCC.SelectedIndex = 0;

            cbProduct.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCustomer.DropDownStyle = ComboBoxStyle.DropDownList;

            cbProduct.DataSource = _mainCon.getProduct();
            cbProduct.DisplayMember = "PRODUCT_NAME";
            cbProduct.ValueMember = "PRODUCT_ID";

            // E-data type = info_document: Edata (LD data) / Serial List / Wafer / ...
            cbEdataType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbEdataType.DataSource = _setCon.searchEdataTypes();
            cbEdataType.DisplayMember = "NAME";
            cbEdataType.ValueMember = "ID";

            _loading = false;

            if (cbProduct.Items.Count > 0)
            {
                LoadCustomers();
            }
        }

        /// <summary>Edits stay staged while another tab is open; they are checked again when Setting closes.</summary>
        public bool ConfirmLeavePage()
        {
            return true;
        }

        public void SavePage()
        {
            if (_pendingSql.Count == 0)
            {
                SetAlert(SettingUi.ALERT_NONE_BACK, SettingUi.ALERT_NONE_TEXT, "i", "Nothing has changed.", false);
                return;
            }

            if (!frmConfirm.Ask(this,
                "Save " + _pendingSql.Count + " change(s) to the database?",
                "Save", "Cancel")) return;

            int n = _pendingSql.Count;
            bool saved = _setCon.saveChanges(new List<string>(_pendingSql));

            if (!saved)
            {
                frmError.ShowError(this, "Save failed, nothing was written." + Environment.NewLine + _setCon.lastError);
                return;
            }

            ClearPending();
            _savedNotice = "Saved " + n + " change(s).";

            LoadGroupOfCustomer();
        }

        public bool CancelPage()
        {
            if (_pendingSql.Count == 0) return false;

            if (!frmConfirm.Ask(this,
                "Discard " + _pendingSql.Count + " unsaved change(s)?",
                "Discard", "Keep editing")) return true;

            ClearPending();
            LoadGroupOfCustomer();
            return true;
        }

        private void ClearPending()
        {
            _pendingSql.Clear();
            _nextEmailId = -1;
            _nextGroupId = -1;
            UpdatePendingCount();
        }

        private void UpdatePendingCount()
        {
            EventHandler handler = SaveButtonTextChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>Ask before switching product / customer while edits are staged.</summary>
        private bool ConfirmDropPendingChanges()
        {
            if (_pendingSql.Count == 0) return true;

            if (!frmConfirm.Ask(this,
                "You have " + _pendingSql.Count + " unsaved change(s). Discard them?",
                "Discard", "Keep editing")) return false;

            ClearPending();
            return true;
        }

        // =====================================================================
        // product / customer
        // =====================================================================
        private void cbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (!ConfirmDropPendingChanges()) return;

            LoadCustomers();
        }

        private SettingEmailProperty BuildProperty()
        {
            SettingEmailProperty prop = new SettingEmailProperty();

            DataRowView selectedProduct = cbProduct.SelectedItem as DataRowView;
            if (selectedProduct == null) return prop;

            prop.productId = selectedProduct["PRODUCT_ID"].ToString();
            prop.productName = selectedProduct["PRODUCT_NAME"].ToString();
            prop.dbProduct = selectedProduct["DATABASE_NAME"].ToString();
            prop.createUser = SettingUi.CurrentUserCode();

            DataRowView selectedCustomer = cbCustomer.SelectedItem as DataRowView;
            if (selectedCustomer != null)
            {
                prop.customerName = selectedCustomer["CUSTOMER_NAME"].ToString();
                prop.totalSpecs = Convert.ToInt32(selectedCustomer["TOTAL_SPECS"]);
                prop.emailSpecs = Convert.ToInt32(selectedCustomer["EMAIL_SPECS"]);
            }

            prop.groupId = _currentGroupId;
            prop.documentId = cbEdataType.SelectedValue == null ? DOC_LD_DATA : Convert.ToInt32(cbEdataType.SelectedValue);

            return prop;
        }

        /// <summary>E-data type changed: show the recipients of that mail (LD data / Serial List / Wafer).</summary>
        private void cbEdataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (!ConfirmDropPendingChanges())
            {
                _loading = true;
                cbEdataType.SelectedIndex = _edataTypeIndex;
                _loading = false;
                return;
            }

            _edataTypeIndex = cbEdataType.SelectedIndex;
            LoadGroupOfCustomer();
        }

        private void LoadCustomers()
        {
            SettingEmailProperty prop = BuildProperty();
            if (string.IsNullOrEmpty(prop.productName)) return;

            DataTable customers = _setCon.searchCustomers(prop);

            if (!string.IsNullOrEmpty(_setCon.lastError))
            {
                frmError.ShowError(this, _setCon.lastError);
                return;
            }

            _loading = true;
            cbCustomer.DataSource = null;

            if (customers.Rows.Count == 0)
            {
                _loading = false;
                lbChangeForReceiver.Text = "-";
                lbForGroupID.Text = "Group -";
                SetEditingEnabled(false);
                ClearMemberArea("No customer of this product is set to send by email.");
                return;
            }

            // plain customer name - the spec counts are shown on the alert line instead,
            // the combo is too narrow for both
            cbCustomer.DataSource = customers;
            cbCustomer.DisplayMember = "CUSTOMER_NAME";
            cbCustomer.ValueMember = "CUSTOMER_NAME";
            _loading = false;

            LoadGroupOfCustomer();
        }

        private void cbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (!ConfirmDropPendingChanges()) return;

            LoadGroupOfCustomer();
        }

        // =====================================================================
        // group + members
        // =====================================================================
        private void LoadGroupOfCustomer()
        {
            SettingEmailProperty prop = BuildProperty();
            if (string.IsNullOrEmpty(prop.customerName)) return;

            lbChangeForReceiver.Text = prop.customerName;

            // no spec of this customer is set to Email -> nothing to manage
            if (prop.emailSpecs == 0)
            {
                _currentGroupId = -1;
                ClearMemberArea("");
                lbForGroupID.Text = "Group -";
                SetAlert(SettingUi.ALERT_NONE_BACK, SettingUi.ALERT_NONE_TEXT, "i",
                    prop.customerName + " is set to Don't Send on all " + prop.totalSpecs +
                    " specs, so it has no recipient group. Change Send Method first.", false);
                SetEditingEnabled(false);
                return;
            }

            // Serial List / Wafer: their own mail, own group (key_group_email_document)
            btnAlertAction.ButtonText = prop.documentId == DOC_LD_DATA ? "Split To New Group" : "Create Group";
            if (prop.documentId != DOC_LD_DATA)
            {
                LoadDocumentGroup(prop);
                return;
            }

            DataTable groups = _setCon.searchGroupOfCustomer(prop);

            if (!string.IsNullOrEmpty(_setCon.lastError))
            {
                frmError.ShowError(this, _setCon.lastError);
                return;
            }

            if (groups.Rows.Count == 0)
            {
                _currentGroupId = -1;
                ClearMemberArea("");
                lbForGroupID.Text = "Group -";
                SetAlert(SettingUi.ALERT_NONE_BACK, SettingUi.ALERT_NONE_TEXT, "i",
                    "No group is bound to " + prop.customerName + " yet.", false);
                SetEditingEnabled(false);
                return;
            }

            // the group used by most Email specs of this customer
            _currentGroupId = Convert.ToInt32(groups.Rows[0]["GROUP_ID"]);
            prop.groupId = _currentGroupId;

            lbForGroupID.Text = "Group #" + _currentGroupId;
            SetEditingEnabled(true);

            LoadMembers(prop);
            LoadAlert(prop, groups);

            if (!string.IsNullOrEmpty(_savedNotice))
            {
                SetAlert(SettingUi.ALERT_OK_BACK, SettingUi.ALERT_OK_TEXT, "v", _savedNotice, false);
                _savedNotice = "";
            }
        }

        /// <summary>Recipients of a Serial List / Wafer mail of the selected product + customer.</summary>
        private void LoadDocumentGroup(SettingEmailProperty prop)
        {
            string typeName = cbEdataType.Text;

            DataTable found = _setCon.searchDocumentGroup(prop);

            if (!string.IsNullOrEmpty(_setCon.lastError))
            {
                frmError.ShowError(this, _setCon.lastError);
                return;
            }

            if (found.Rows.Count == 0)
            {
                _currentGroupId = -1;
                ClearMemberArea("");
                lbForGroupID.Text = "Group -";
                SetAlert(SettingUi.ALERT_NONE_BACK, SettingUi.ALERT_NONE_TEXT, "i",
                    "No " + typeName + " recipients for " + prop.customerName + " (" + prop.productName + ") in the database" +
                    " - sending still reads Setting\\EmailSetting.ini. Create a group to manage them here.", true);
                SetEditingEnabled(false);
                return;
            }

            _currentGroupId = Convert.ToInt32(found.Rows[0]["GROUP_ID"]);
            prop.groupId = _currentGroupId;

            lbForGroupID.Text = "Group #" + _currentGroupId;
            SetEditingEnabled(true);
            LoadMembers(prop);

            SetAlert(SettingUi.ALERT_INFO_BACK, SettingUi.ALERT_INFO_TEXT, "i",
                typeName + " of " + prop.customerName + " (" + prop.productName + ") goes out as its own mail" +
                " to Group #" + _currentGroupId + ", not to the LD data group.", false);

            if (!string.IsNullOrEmpty(_savedNotice))
            {
                SetAlert(SettingUi.ALERT_OK_BACK, SettingUi.ALERT_OK_TEXT, "v", _savedNotice, false);
                _savedNotice = "";
            }
        }

        /// <summary>Stage an empty group for this product + customer + E-data type; add recipients, then Save.</summary>
        private void CreateDocumentGroup()
        {
            SettingEmailProperty prop = BuildProperty();
            if (string.IsNullOrEmpty(prop.customerName) || prop.documentId == DOC_LD_DATA) return;

            if (!frmConfirm.Ask(this,
                "Create a " + cbEdataType.Text + " recipient group for " + prop.customerName + " (" + prop.productName + ")?" +
                Environment.NewLine + "Add the recipients, then press Save.",
                "Create", "Cancel")) return;

            if (_nextGroupId < 0)
            {
                _nextGroupId = _setCon.maxGroupId();
            }

            _nextGroupId = _nextGroupId + 1;
            prop.newGroupId = _nextGroupId;

            _pendingSql.Add(_sqlFactory.insertDocumentGroup(prop));

            _currentGroupId = prop.newGroupId;
            prop.groupId = _currentGroupId;
            lbForGroupID.Text = "Group #" + _currentGroupId + " (new)";
            LoadMembers(prop);   // empty grid with the right columns
            SetEditingEnabled(true);

            SetAlert(SettingUi.ALERT_INFO_BACK, SettingUi.ALERT_INFO_TEXT, "i",
                "Group #" + _currentGroupId + " is staged for " + cbEdataType.Text + " of " + prop.customerName +
                " - add the recipients, then press Save.", false);

            UpdatePendingCount();
        }

        private void LoadMembers(SettingEmailProperty prop)
        {
            DataTable members = _setCon.searchGroupMembers(prop);

            if (!string.IsNullOrEmpty(_setCon.lastError))
            {
                frmError.ShowError(this, _setCon.lastError);
                return;
            }

            _members = members;
            dtgShowEmailAll.DataSource = _members;
            FormatMemberGrid();
        }

        /// <summary>Alert panel: shared group, split-off specs, or all clear.</summary>
        private void LoadAlert(SettingEmailProperty prop, DataTable groupsOfCustomer)
        {
            DataTable shared = _setCon.searchCustomersSharingGroup(prop);

            List<string> others = new List<string>();
            foreach (DataRow row in shared.Rows)
            {
                others.Add(row["CUSTOMER_NAME"].ToString());
            }

            if (others.Count > 0)
            {
                // a group can be shared by many customers (17 of them on the test data),
                // so only the first few are named and the rest are counted
                string names = string.Join(", ", others.GetRange(0, Math.Min(ALERT_MAX_NAMES, others.Count)).ToArray());

                if (others.Count > ALERT_MAX_NAMES)
                {
                    names += " และอีก " + (others.Count - ALERT_MAX_NAMES) + " ราย";
                }

                SetAlert(SettingUi.ALERT_WARN_BACK, SettingUi.ALERT_WARN_TEXT, "!",
                    "ใช้ร่วมกับลูกค้าอื่นอีก " + others.Count + " ราย : " + names +
                    "  -  เพิ่ม/ลบที่นี่จะมีผลกับลูกค้าเหล่านั้นด้วย", true);
                return;
            }

            if (groupsOfCustomer.Rows.Count > 1)
            {
                SetAlert(SettingUi.ALERT_WARN_BACK, SettingUi.ALERT_WARN_TEXT, "!",
                    "Specs of " + prop.customerName + " point at " + groupsOfCustomer.Rows.Count +
                    " different groups. Showing Group #" + _currentGroupId + " (used by most specs).", false);
                return;
            }

            if (prop.emailSpecs < prop.totalSpecs)
            {
                SetAlert(SettingUi.ALERT_INFO_BACK, SettingUi.ALERT_INFO_TEXT, "i",
                    prop.emailSpecs + " of " + prop.totalSpecs + " specs use this group (Email). The other " +
                    (prop.totalSpecs - prop.emailSpecs) + " specs are set to Don't Send and need no group.", false);
                return;
            }

            SetAlert(SettingUi.ALERT_OK_BACK, SettingUi.ALERT_OK_TEXT, "v",
                "Group #" + _currentGroupId + " is used by " + prop.customerName + " only.", false);
        }

        private void SetAlert(Color back, Color fore, string icon, string message, bool showAction)
        {
            pnAlert.BackColor = back;
            lbAlertIcon.ForeColor = fore;
            lbAlertIcon.Text = icon;
            lbAlertText.ForeColor = fore;
            btnAlertAction.Visible = showAction;

            // the label must stop before the action button, otherwise the text runs
            // underneath it; without the button it may use the whole panel
            int rightEdge = showAction
                ? btnAlertAction.Left - ALERT_TEXT_GAP
                : pnAlert.ClientSize.Width - ALERT_TEXT_GAP;

            lbAlertText.Width = Math.Max(120, rightEdge - lbAlertText.Left);
            lbAlertText.Text = message;

            // anything cut off by the ellipsis is still readable on hover
            toolTipAlert.SetToolTip(lbAlertText, message);
            toolTipAlert.SetToolTip(lbAlertIcon, message);
        }

        private void ClearMemberArea(string message)
        {
            _members = null;
            dtgShowEmailAll.DataSource = null;

            if (!string.IsNullOrEmpty(message))
            {
                SetAlert(SettingUi.ALERT_NONE_BACK, SettingUi.ALERT_NONE_TEXT, "i", message, false);
            }
        }

        private void SetEditingEnabled(bool enabled)
        {
            txtAddEmail.Enabled = enabled;
            cbToOrCC.Enabled = enabled;
            btnAddToGroup.Enabled = enabled;
            dtgShowEmailAll.Enabled = enabled;
        }

        // =====================================================================
        // member grid
        // =====================================================================
        private void SetupMemberGrid()
        {
            SettingUi.StyleGrid(dtgShowEmailAll);

            dtgShowEmailAll.CellFormatting += dtgShowEmailAll_CellFormatting;
            dtgShowEmailAll.CellMouseEnter += dtgShowEmailAll_CellMouseEnter;
            dtgShowEmailAll.CellMouseLeave += dtgShowEmailAll_CellMouseLeave;
        }

        /// <summary>TO in blue, CC in grey, so the type reads at a glance.</summary>
        private void dtgShowEmailAll_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dtgShowEmailAll.Columns[e.ColumnIndex].Name != COL_TYPE) return;

            string type = Convert.ToString(e.Value);

            e.CellStyle.Font = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Bold);
            e.CellStyle.ForeColor = (type == "TO") ? SettingUi.TYPE_TO_TEXT : SettingUi.TYPE_CC_TEXT;
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        /// <summary>Hand cursor on the two clickable columns.</summary>
        private void dtgShowEmailAll_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string column = dtgShowEmailAll.Columns[e.ColumnIndex].Name;
            if (column == COL_REMOVE || column == COL_TYPE)
            {
                dtgShowEmailAll.Cursor = Cursors.Hand;
            }
        }

        private void dtgShowEmailAll_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dtgShowEmailAll.Cursor = Cursors.Default;
        }

        private void FormatMemberGrid()
        {
            if (dtgShowEmailAll.Columns.Count == 0) return;

            if (dtgShowEmailAll.Columns.Contains("EMAIL_ID")) dtgShowEmailAll.Columns["EMAIL_ID"].Visible = false;
            if (dtgShowEmailAll.Columns.Contains("RECIPIENT_ID")) dtgShowEmailAll.Columns["RECIPIENT_ID"].Visible = false;
            if (dtgShowEmailAll.Columns.Contains("INUSE")) dtgShowEmailAll.Columns["INUSE"].Visible = false;

            SettingUi.SetColumn(dtgShowEmailAll, "EMAIL_ADDRESS", "EMAIL ADDRESS", 300);
            SettingUi.SetColumn(dtgShowEmailAll, COL_TYPE, "TYPE  (click to switch)", 90);

            if (dtgShowEmailAll.Columns.Contains(COL_TYPE))
            {
                dtgShowEmailAll.Columns[COL_TYPE].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (!dtgShowEmailAll.Columns.Contains(COL_REMOVE))
            {
                dtgShowEmailAll.Columns.Add(SettingUi.MakeButtonColumn(COL_REMOVE, "Remove", 70, SettingUi.REMOVE_TEXT));
            }

            // TO rows first, then CC - reads like the email header itself
            if (_members != null && _members.DefaultView != null)
            {
                _members.DefaultView.Sort = "RECIPIENT_ID ASC, EMAIL_ADDRESS ASC";
            }
        }

        private void dtgShowEmailAll_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _members == null) return;
            if (_currentGroupId < 0) return;

            string columnName = dtgShowEmailAll.Columns[e.ColumnIndex].Name;
            DataRow row = ((DataRowView)dtgShowEmailAll.Rows[e.RowIndex].DataBoundItem).Row;

            SettingEmailProperty prop = BuildProperty();
            prop.groupId = _currentGroupId;
            prop.emailId = Convert.ToInt32(row["EMAIL_ID"]);
            prop.emailAddress = row["EMAIL_ADDRESS"].ToString();

            if (columnName == COL_REMOVE)
            {
                if (!frmConfirm.Ask(this,
                    "Remove " + prop.emailAddress + " from Group #" + _currentGroupId + " ?",
                    "Remove", "Cancel")) return;

                _pendingSql.Add(_sqlFactory.deleteGroupMember(prop));
                row.Delete();
                _members.AcceptChanges();
                UpdatePendingCount();
                return;
            }

            if (columnName == COL_TYPE)
            {
                int currentType = Convert.ToInt32(row["RECIPIENT_ID"]);
                prop.recipientId = (currentType == 1) ? 2 : 1;

                _pendingSql.Add(_sqlFactory.updateRecipientType(prop));

                row["RECIPIENT_ID"] = prop.recipientId;
                row[COL_TYPE] = (prop.recipientId == 1) ? "TO" : "CC";
                _members.AcceptChanges();
                UpdatePendingCount();
            }
        }

        // =====================================================================
        // add member
        // =====================================================================
        private void btnAddToGroup_Click(object sender, EventArgs e)
        {
            if (_currentGroupId < 0)
            {
                frmError.ShowError(this, "Select a customer that has a recipient group first.");
                return;
            }

            string address = (txtAddEmail.Text ?? "").Trim();

            if (!IsValidEmail(address))
            {
                frmError.ShowError(this, "Please type a valid email address.");
                txtAddEmail.Focus();
                return;
            }

            if (_members != null)
            {
                foreach (DataRow existing in _members.Rows)
                {
                    if (string.Equals(existing["EMAIL_ADDRESS"].ToString(), address, StringComparison.OrdinalIgnoreCase))
                    {
                        frmError.ShowError(this, "This address is already in Group #" + _currentGroupId + ".");
                        return;
                    }
                }
            }

            SettingEmailProperty prop = BuildProperty();
            prop.groupId = _currentGroupId;
            prop.emailAddress = address;
            prop.recipientId = (cbToOrCC.SelectedItem != null && cbToOrCC.SelectedItem.ToString() == "CC") ? 2 : 1;

            // reuse the master row when the address already exists, otherwise stage a new one
            DataTable found = _setCon.searchEmailByAddress(prop);

            if (found.Rows.Count > 0)
            {
                prop.emailId = Convert.ToInt32(found.Rows[0]["EMAIL_ID"]);
            }
            else
            {
                if (_nextEmailId < 0)
                {
                    _nextEmailId = _setCon.maxEmailId();
                }

                _nextEmailId = _nextEmailId + 1;
                prop.emailId = _nextEmailId;

                _pendingSql.Add(_sqlFactory.insertEmail(prop));
            }

            _pendingSql.Add(_sqlFactory.insertGroupMember(prop));

            if (_members != null)
            {
                DataRow row = _members.NewRow();
                row["EMAIL_ID"] = prop.emailId;
                row["EMAIL_ADDRESS"] = prop.emailAddress;
                row["RECIPIENT_ID"] = prop.recipientId;
                row[COL_TYPE] = (prop.recipientId == 1) ? "TO" : "CC";
                if (_members.Columns.Contains("INUSE")) row["INUSE"] = 1;
                _members.Rows.Add(row);
                _members.AcceptChanges();
            }

            txtAddEmail.Text = "";
            UpdatePendingCount();
        }

        private bool IsValidEmail(string address)
        {
            if (string.IsNullOrEmpty(address)) return false;
            return Regex.IsMatch(address, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        // =====================================================================
        // split to new group
        // =====================================================================
        private void btnAlertAction_Click(object sender, EventArgs e)
        {
            // on a Serial List / Wafer page the button creates that type's group instead of splitting
            if (BuildProperty().documentId != DOC_LD_DATA)
            {
                CreateDocumentGroup();
                return;
            }

            if (_currentGroupId < 0) return;

            SettingEmailProperty prop = BuildProperty();
            prop.groupId = _currentGroupId;

            if (!frmConfirm.Ask(this,
                "Copy Group #" + _currentGroupId + " into a new group for " + prop.customerName + " only?" +
                Environment.NewLine + "The other customers keep Group #" + _currentGroupId + " unchanged.",
                "Split", "Cancel")) return;

            if (_nextGroupId < 0)
            {
                _nextGroupId = _setCon.maxGroupId();
            }

            _nextGroupId = _nextGroupId + 1;
            prop.newGroupId = _nextGroupId;

            _pendingSql.Add(_sqlFactory.copyGroupMembers(prop));
            _pendingSql.Add(_sqlFactory.logCustomerGroupChange(prop));   // before the update: keeps the old group
            _pendingSql.Add(_sqlFactory.updateCustomerGroup(prop));

            SetAlert(SettingUi.ALERT_INFO_BACK, SettingUi.ALERT_INFO_TEXT, "i",
                "Group #" + prop.newGroupId + " is staged for " + prop.customerName +
                " - press Save to write it to the database.", false);

            UpdatePendingCount();
        }

        // =====================================================================
        // all groups
        // =====================================================================
        private void btnAllGroupDetails_Click(object sender, EventArgs e)
        {
            if (_setCon == null) return;

            // every product, not only the one selected above
            List<SettingEmailProperty> products = new List<SettingEmailProperty>();

            foreach (DataRowView row in cbProduct.Items)
            {
                products.Add(new SettingEmailProperty
                {
                    productName = row["PRODUCT_NAME"].ToString(),
                    dbProduct = row["DATABASE_NAME"].ToString()
                });
            }

            DataTable matrix = _setCon.searchGroupMatrix(products);

            if (!string.IsNullOrEmpty(_setCon.lastError))
            {
                frmError.ShowError(this, _setCon.lastError);
                return;
            }

            ShowAllGroupsDialog(matrix);
        }

        /// <summary>
        /// One table of every product + customer: the group it is bound to and who receives the mail.
        /// </summary>
        private void ShowAllGroupsDialog(DataTable matrix)
        {
            DataView view = new DataView(matrix);

            using (Form dialog = new Form())
            {
                dialog.Text = "Group Overview - every product / customer";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.ClientSize = new Size(1180, 620);
                dialog.MinimumSize = new Size(820, 420);
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.White;
                dialog.Font = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Regular);

                // ---- top bar: search ----
                Panel top = new Panel();
                top.Dock = DockStyle.Top;
                top.Height = 58;
                top.BackColor = Color.White;
                top.Padding = new Padding(16, 12, 16, 8);

                Label lbFind = new Label();
                lbFind.Text = "Search";
                lbFind.AutoSize = true;
                lbFind.Location = new Point(16, 20);
                lbFind.ForeColor = SettingUi.GRID_TEXT;

                TextBox txtFind = new TextBox();
                txtFind.Location = new Point(76, 16);
                txtFind.Size = new Size(320, 26);
                txtFind.Font = new Font(SettingUi.FONT_NAME, 10F, FontStyle.Regular);

                Label lbCount = new Label();
                lbCount.AutoSize = true;
                lbCount.Location = new Point(412, 20);
                lbCount.ForeColor = SettingUi.TYPE_CC_TEXT;
                lbCount.Text = matrix.Rows.Count + " row(s)";

                top.Controls.Add(lbCount);
                top.Controls.Add(txtFind);
                top.Controls.Add(lbFind);

                // ---- grid ----
                DataGridView grid = new DataGridView();
                grid.Dock = DockStyle.Fill;
                SettingUi.StyleGrid(grid);
                grid.DataSource = view;

                grid.DataBindingComplete += delegate
                {
                    FormatMatrixGrid(grid);
                };

                // TO in blue, CC in grey, same language as the member grid
                grid.CellFormatting += delegate (object s2, DataGridViewCellFormattingEventArgs e2)
                {
                    if (e2.RowIndex < 0) return;
                    string column = grid.Columns[e2.ColumnIndex].Name;

                    if (column == "TO_LIST" || column == "TO_COUNT")
                    {
                        e2.CellStyle.ForeColor = SettingUi.TYPE_TO_TEXT;
                        e2.CellStyle.SelectionForeColor = SettingUi.TYPE_TO_TEXT;
                    }
                    else if (column == "CC_LIST" || column == "CC_COUNT")
                    {
                        e2.CellStyle.ForeColor = SettingUi.TYPE_CC_TEXT;
                        e2.CellStyle.SelectionForeColor = SettingUi.TYPE_CC_TEXT;
                    }
                    else if (column == "GROUP_ID")
                    {
                        e2.Value = "Group #" + Convert.ToString(e2.Value);
                        e2.CellStyle.Font = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Bold);
                        e2.FormattingApplied = true;
                    }
                };

                // full address list on hover - the column is too narrow for 10 addresses
                grid.CellMouseEnter += delegate (object s3, DataGridViewCellEventArgs e3)
                {
                    if (e3.RowIndex < 0 || e3.ColumnIndex < 0) return;
                    DataGridViewCell cell = grid.Rows[e3.RowIndex].Cells[e3.ColumnIndex];
                    cell.ToolTipText = Convert.ToString(cell.Value);
                };

                // ---- search filter ----
                txtFind.TextChanged += delegate
                {
                    string keyword = txtFind.Text.Trim().Replace("'", "''");

                    view.RowFilter = keyword.Length == 0
                        ? ""
                        : "PRODUCT LIKE '%" + keyword + "%' OR CUSTOMER_NAME LIKE '%" + keyword + "%'"
                          + " OR TO_LIST LIKE '%" + keyword + "%' OR CC_LIST LIKE '%" + keyword + "%'"
                          + " OR CONVERT(GROUP_ID, 'System.String') LIKE '%" + keyword + "%'";

                    lbCount.Text = view.Count + " row(s)"
                        + (keyword.Length == 0 ? "" : " of " + matrix.Rows.Count);
                };

                // ---- bottom bar ----
                Panel bottom = new Panel();
                bottom.Dock = DockStyle.Bottom;
                bottom.Height = 56;
                bottom.BackColor = Color.White;

                Label lbHint = new Label();
                lbHint.AutoSize = true;
                lbHint.Location = new Point(16, 20);
                lbHint.ForeColor = SettingUi.TYPE_CC_TEXT;
                lbHint.Text = "Only customers whose Send Method is Email are listed. Hover a cell to read the full address list.";

                Button close = new Button();
                close.Text = "Close";
                close.Size = new Size(110, 32);
                close.Location = new Point(dialog.ClientSize.Width - 130, 12);
                close.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                close.FlatStyle = FlatStyle.Flat;
                close.BackColor = Color.White;
                close.Click += delegate { dialog.Close(); };

                bottom.Controls.Add(close);
                bottom.Controls.Add(lbHint);

                dialog.Controls.Add(grid);
                dialog.Controls.Add(top);
                dialog.Controls.Add(bottom);
                dialog.AcceptButton = close;

                dialog.ShowDialog(this);
            }
        }

        private void FormatMatrixGrid(DataGridView grid)
        {
            if (grid.Columns.Count == 0) return;

            SettingUi.SetColumn(grid, "PRODUCT", "PRODUCT", 70);
            SettingUi.SetColumn(grid, "CUSTOMER_NAME", "CUSTOMER", 150);
            SettingUi.SetColumn(grid, "GROUP_ID", "GROUP", 80);
            SettingUi.SetColumn(grid, "SPEC_COUNT", "SPECS", 60);
            SettingUi.SetColumn(grid, "TO_COUNT", "TO", 45);
            SettingUi.SetColumn(grid, "CC_COUNT", "CC", 45);
            SettingUi.SetColumn(grid, "TO_LIST", "TO (recipients)", 250);
            SettingUi.SetColumn(grid, "CC_LIST", "CC (recipients)", 250);

            if (grid.Columns.Contains("SPEC_COUNT"))
            {
                grid.Columns["SPEC_COUNT"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
    }
}
