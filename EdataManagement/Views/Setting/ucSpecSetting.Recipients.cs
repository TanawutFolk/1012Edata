using EdataManagement.Controllers;
using EdataManagement.Property;

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EdataManagement.Views.Setting
{
    /// <summary>
    /// Recipients panel of ucSpecSetting (moved in from the old Email Sending page): who receives the mail,
    /// shown only when it goes by Email.
    ///   Edata (LD data)      the group of the ticked specs - the Recipient Group list decides which -
    ///                        or, with nothing ticked, the group most Email specs of the customer use
    ///   Serial List / Wafer  the customer's own group in key_group_email_document
    /// Add / Remove / TO-CC / Split / Create Group are staged with the spec edits and written by the same Save.
    /// A group is shared, so a member change reaches every spec / customer on it: sharing is always spelled out,
    /// and Group #0 (internal, also used by other programs) is read only - Split it instead.
    ///
    /// Save writes them before the spec values (AddRecipientStatements); info_group_email has no trigger,
    /// so every change gets its own row in log_setting_edata.
    /// </summary>
    public partial class ucSpecSetting
    {
        private const int DOC_LD_DATA = 1;
        private const int RCP_TO = 1;
        private const int RCP_CC = 2;
        private const int NOTE_MAX_NAMES = 3;

        private const string COL_RCP_REMOVE = "colRemove";
        private const string COL_RCP_TYPE = "RECIPIENT_TYPE";
        private const string COL_RCP_ID = "RECIPIENT_ID";
        private const string COL_RCP_ADDRESS = "EMAIL_ADDRESS";

        private const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        private enum RecipientAction { None, Split, CreateGroup }

        private SettingEmailControllers _emailCon;

        /// <summary>Members of every group looked at: as edited on screen / as read from the database.</summary>
        private readonly Dictionary<int, DataTable> _members = new Dictionary<int, DataTable>();
        private readonly Dictionary<int, DataTable> _membersLoaded = new Dictionary<int, DataTable>();

        /// <summary>Other customers of the same product on a group, read once.</summary>
        private readonly Dictionary<int, List<string>> _sharing = new Dictionary<int, List<string>>();

        /// <summary>Groups made on this screen (Split / Create Group), not in the database yet: id -> what it is.</summary>
        private readonly Dictionary<int, string> _newGroups = new Dictionary<int, string>();

        /// <summary>Serial List / Wafer group made here for the current customer: document id -> group id.</summary>
        private readonly Dictionary<int, int> _newDocumentGroups = new Dictionary<int, int>();

        private int _nextGroupId = -1;

        /// <summary>Group made by Split -> the group it was copied from (for the log).</summary>
        private readonly Dictionary<int, int> _splitSource = new Dictionary<int, int>();

        /// <summary>Group on the panel; -1 = a message instead of a group.</summary>
        private int _shownGroupId = -1;

        private RecipientAction _recipientAction = RecipientAction.None;

        /// <summary>Specs that Split moves to the new group.</summary>
        private List<DataRow> _splitTargets = new List<DataRow>();

        private void WireRecipientEvents()
        {
            cbMailType.SelectedIndexChanged += cbMailType_SelectedIndexChanged;
            btnRcpAction.Click += btnRcpAction_Click;
            btnAddEmail.Click += btnAddEmail_Click;
            btnAllGroups.Click += btnAllGroups_Click;
            dtgRecipients.CellClick += dtgRecipients_CellClick;
            dtgRecipients.CellFormatting += dtgRecipients_CellFormatting;
            dtgRecipients.CellMouseEnter += dtgRecipients_CellMouseEnter;
            dtgRecipients.CellMouseLeave += delegate { dtgRecipients.Cursor = Cursors.Default; };
            dtgRecipients.DataBindingComplete += delegate { FormatRecipientGrid(); };
        }

        private void SetupRecipients()
        {
            _emailCon = new SettingEmailControllers();

            SettingUi.StyleGrid(dtgRecipients, 30, 9F, 32);

            cbToOrCC.DropDownStyle = ComboBoxStyle.DropDownList;
            cbToOrCC.Items.Clear();
            cbToOrCC.Items.Add("TO");
            cbToOrCC.Items.Add("CC");
            cbToOrCC.SelectedIndex = 0;

            // mail types = info_document: Edata (LD data, group per spec) / Serial List / Wafer ...
            DataTable types = _emailCon.searchEdataTypes();
            if (types.Rows.Count == 0)
            {
                types = new DataTable();
                types.Columns.Add("ID", typeof(int));
                types.Columns.Add("NAME", typeof(string));
                types.Rows.Add(DOC_LD_DATA, "Edata");
            }

            cbMailType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbMailType.DataSource = types;
            cbMailType.DisplayMember = "NAME";
            cbMailType.ValueMember = "ID";
            cbMailType.DropDownWidth = 160;

            ShowRecipientsMessage("Recipients", "");
        }

        private int SelectedMailType()
        {
            return cbMailType.SelectedValue == null ? DOC_LD_DATA : ToInt(cbMailType.SelectedValue);
        }

        private void cbMailType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            RefreshRecipients();
        }

        // =====================================================================
        // which group to show
        // =====================================================================
        /// <summary>Put the group that matters for what is ticked / picked on the panel, or say why there is none.</summary>
        private void RefreshRecipients()
        {
            if (_emailCon == null) return;

            _splitTargets = new List<DataRow>();

            string customer = SelectedCustomerName();
            if (_specs == null || customer == "")
            {
                ShowRecipientsMessage("Recipients", "No customer selected.");
                return;
            }

            int documentId = SelectedMailType();
            if (documentId != DOC_LD_DATA)
            {
                ShowDocumentRecipients(documentId);
                return;
            }

            List<DataRow> ticked = TickedRows();

            if (ticked.Count == 0)
            {
                ShowCustomerGroup(customer);
                return;
            }

            int send = SelectedId(cbSendMethod);
            if (send == KEEP)
            {
                ShowRecipientsMessage("Recipients", "The ticked specs use different Send Methods." +
                    Environment.NewLine + Environment.NewLine + "Pick Email (or tick only Email specs) to see who receives the mail.");
                return;
            }

            if (send != SEND_EMAIL)
            {
                ShowRecipientsMessage("Recipients", "Send Method: " + ListText(_sendMethods, send) +
                    Environment.NewLine + Environment.NewLine + "Recipients are for Email only.");
                return;
            }

            int groupId = SelectedId(cbGroup);
            if (groupId == KEEP)
            {
                List<int> groups = ticked.Select(r => ToInt(r[COL_GROUP])).Distinct().OrderBy(g => g).ToList();

                ShowRecipientsMessage("Recipients", groups.Count > 1
                    ? "The ticked specs use " + groups.Count + " groups: " + string.Join(", ", groups.Select(g => "#" + g)) + "." +
                      Environment.NewLine + Environment.NewLine + "Pick one Recipient Group to see and edit its members."
                    : "Group #" + groups[0] + " is not in info_group_email.");
                return;
            }

            _splitTargets = ticked;

            int notOnIt = ticked.Count(r => ToInt(r[COL_GROUP]) != groupId);
            ShowGroup(groupId, notOnIt > 0
                ? "Picked for " + notOnIt + " ticked spec(s) - Apply moves them to this group."
                : "Used by the " + ticked.Count + " ticked spec(s).", true);
        }

        /// <summary>Nothing ticked: the group most Email specs of the customer use, like the old Email Sending page.</summary>
        private void ShowCustomerGroup(string customer)
        {
            List<DataRow> email = _specs.Rows.Cast<DataRow>().Where(r => ToInt(r[COL_SEND]) == SEND_EMAIL).ToList();

            if (email.Count == 0)
            {
                ShowRecipientsMessage("Recipients", customer + " sends nothing by Email." +
                    Environment.NewLine + Environment.NewLine + "Tick specs and set Send Method = Email to choose who receives the mail.");
                return;
            }

            List<IGrouping<int, DataRow>> byGroup = email
                .GroupBy(r => ToInt(r[COL_GROUP]))
                .OrderByDescending(g => g.Count())
                .ToList();

            int groupId = byGroup[0].Key;
            _splitTargets = byGroup[0].ToList();

            ShowGroup(groupId, byGroup.Count > 1
                ? "Email specs of " + customer + " use " + byGroup.Count + " groups - this is the one of " + _splitTargets.Count + " specs. Tick specs to see another."
                : "Used by the " + email.Count + " Email spec(s) of " + customer + ". Tick specs to change their group.", true);
        }

        /// <summary>Serial List / Wafer: its own mail and group per product + customer, not per spec.</summary>
        private void ShowDocumentRecipients(int documentId)
        {
            string typeName = cbMailType.Text;
            string customer = SelectedCustomerName();
            string product = SelectedProductName();

            int groupId;
            if (!_newDocumentGroups.TryGetValue(documentId, out groupId))
            {
                DataTable found = _emailCon.searchDocumentGroup(new SettingEmailProperty
                {
                    productName = product,
                    customerName = customer,
                    documentId = documentId
                });

                if (!string.IsNullOrEmpty(_emailCon.lastError))
                {
                    ShowRecipientsMessage("Recipients", "Could not read the " + typeName + " group:" + Environment.NewLine + _emailCon.lastError);
                    return;
                }

                if (found.Rows.Count == 0)
                {
                    ShowRecipientsMessage("Recipients", "No " + typeName + " recipients for " + customer + " (" + product + ") in the database." +
                        Environment.NewLine + Environment.NewLine + "Sending still reads Setting\\EmailSetting.ini. Create a group to manage them here.");
                    SetRecipientNote("", SettingUi.ALERT_NONE_TEXT, RecipientAction.CreateGroup);
                    return;
                }

                groupId = ToInt(found.Rows[0]["GROUP_ID"]);
            }

            ShowGroup(groupId, typeName + " of " + customer + " goes out as its own mail, not to the spec group.", false);
        }

        // =====================================================================
        // panel content
        // =====================================================================
        private void ShowGroup(int groupId, string contextNote, bool splittable)
        {
            DataTable members = MembersOf(groupId);
            if (members == null)
            {
                ShowRecipientsMessage("Recipients", "Could not read Group #" + groupId + ":" + Environment.NewLine + _emailCon.lastError);
                return;
            }

            bool isNew = _newGroups.ContainsKey(groupId);
            bool locked = groupId == GROUP_INTERNAL;

            _shownGroupId = groupId;
            lbRcpTitle.Text = "Group #" + groupId + (isNew ? "  (new)" : "");

            lbRcpEmpty.Visible = false;
            dtgRecipients.Visible = true;

            // TO first, then CC - reads like the mail header
            members.DefaultView.Sort = COL_RCP_ID + " ASC, " + COL_RCP_ADDRESS + " ASC";
            if (dtgRecipients.DataSource != members) dtgRecipients.DataSource = members;
            FormatRecipientGrid();
            dtgRecipients.Invalidate();

            SetAddRow(true, !locked);

            // what an edit here reaches, most important first
            List<string> others = isNew ? new List<string>() : OtherCustomersOn(groupId);
            RecipientAction split = splittable && _splitTargets.Count > 0 ? RecipientAction.Split : RecipientAction.None;

            if (locked)
            {
                SetRecipientNote("#0 is internal and shared (other customers, uITLA) - read only. Split to give these specs their own group.",
                    SettingUi.ALERT_WARN_TEXT, split);
            }
            else if (isNew)
            {
                SetRecipientNote(_newGroups[groupId] + " - staged, Save creates it.", SettingUi.ALERT_INFO_TEXT, RecipientAction.None);
            }
            else if (others.Count > 0)
            {
                SetRecipientNote("Also used by " + NameList(others) + " - adding / removing here changes their mail too.",
                    SettingUi.ALERT_WARN_TEXT, split);
            }
            else
            {
                SetRecipientNote(contextNote, SettingUi.ALERT_NONE_TEXT, RecipientAction.None);
            }
        }

        private void ShowRecipientsMessage(string title, string message)
        {
            _shownGroupId = -1;
            lbRcpTitle.Text = title;

            dtgRecipients.DataSource = null;
            dtgRecipients.Visible = false;
            lbRcpEmpty.Text = message;
            lbRcpEmpty.Visible = true;

            SetAddRow(false, false);
            SetRecipientNote("", SettingUi.ALERT_NONE_TEXT, RecipientAction.None);
        }

        private void SetAddRow(bool visible, bool enabled)
        {
            txtAddEmail.Visible = visible;
            cbToOrCC.Visible = visible;
            btnAddEmail.Visible = visible;

            txtAddEmail.Enabled = enabled;
            cbToOrCC.Enabled = enabled;
            SetButtonEnabled(btnAddEmail, enabled, Color.FromArgb(102, 153, 0));
        }

        /// <summary>The line under the title, with Split / Create Group next to it when that is the way out.</summary>
        private void SetRecipientNote(string text, Color fore, RecipientAction action)
        {
            _recipientAction = action;

            btnRcpAction.Visible = action != RecipientAction.None;
            if (action == RecipientAction.Split) btnRcpAction.ButtonText = "Split";
            if (action == RecipientAction.CreateGroup) btnRcpAction.ButtonText = "Create Group";

            int rightEdge = btnRcpAction.Visible ? btnRcpAction.Left - 8 : pnRecipients.ClientSize.Width - 12;
            lbRcpNote.Width = Math.Max(120, rightEdge - lbRcpNote.Left);
            lbRcpNote.ForeColor = fore;
            lbRcpNote.Text = text;
            toolTipAlert.SetToolTip(lbRcpNote, text);
        }

        private static string NameList(List<string> names)
        {
            string text = string.Join(", ", names.Take(NOTE_MAX_NAMES).ToArray());
            if (names.Count > NOTE_MAX_NAMES) text += " +" + (names.Count - NOTE_MAX_NAMES) + " more";
            return text;
        }

        // =====================================================================
        // member grid
        // =====================================================================
        private void FormatRecipientGrid()
        {
            if (dtgRecipients.Columns.Count == 0) return;

            foreach (string hidden in new[] { "EMAIL_ID", COL_RCP_ID, "INUSE" })
            {
                if (dtgRecipients.Columns.Contains(hidden)) dtgRecipients.Columns[hidden].Visible = false;
            }

            SettingUi.SetColumn(dtgRecipients, COL_RCP_ADDRESS, "EMAIL ADDRESS", 250);
            SettingUi.SetColumn(dtgRecipients, COL_RCP_TYPE, "TYPE", 55);

            if (dtgRecipients.Columns.Contains(COL_RCP_TYPE))
            {
                dtgRecipients.Columns[COL_RCP_TYPE].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (!dtgRecipients.Columns.Contains(COL_RCP_REMOVE))
            {
                dtgRecipients.Columns.Add(SettingUi.MakeButtonColumn(COL_RCP_REMOVE, "Remove", 70, SettingUi.REMOVE_TEXT));
            }

            dtgRecipients.Columns[COL_RCP_REMOVE].Visible = _shownGroupId != GROUP_INTERNAL;
            dtgRecipients.Columns[COL_RCP_REMOVE].DisplayIndex = dtgRecipients.Columns.Count - 1;
            dtgRecipients.ClearSelection();
        }

        /// <summary>TO blue / CC grey like before; an added address or a switched TO/CC in bold blue.</summary>
        private void dtgRecipients_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || _shownGroupId < 0) return;

            DataRowView item = dtgRecipients.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item == null) return;

            string column = dtgRecipients.Columns[e.ColumnIndex].Name;
            int loadedType = LoadedType(_shownGroupId, Convert.ToString(item[COL_RCP_ADDRESS]));

            if (column == COL_RCP_TYPE)
            {
                bool switched = loadedType != 0 && loadedType != ToInt(item[COL_RCP_ID]);
                Color fore = switched ? SettingUi.TAB_ACTIVE_BACK
                    : (Convert.ToString(e.Value) == "TO" ? SettingUi.TYPE_TO_TEXT : SettingUi.TYPE_CC_TEXT);

                e.CellStyle.Font = _gridBold;
                e.CellStyle.ForeColor = fore;
                e.CellStyle.SelectionForeColor = fore;
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            else if (column == COL_RCP_ADDRESS && loadedType == 0)
            {
                e.CellStyle.Font = _gridBold;
                e.CellStyle.ForeColor = SettingUi.TAB_ACTIVE_BACK;
                e.CellStyle.SelectionForeColor = SettingUi.TAB_ACTIVE_BACK;
            }
        }

        private void dtgRecipients_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _shownGroupId == GROUP_INTERNAL) return;

            string column = dtgRecipients.Columns[e.ColumnIndex].Name;
            if (column == COL_RCP_REMOVE || column == COL_RCP_TYPE) dtgRecipients.Cursor = Cursors.Hand;
        }

        /// <summary>TYPE switches TO / CC, Remove takes the address out of the group.</summary>
        private void dtgRecipients_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (_shownGroupId < 0 || _shownGroupId == GROUP_INTERNAL) return;

            DataRowView item = dtgRecipients.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item == null) return;

            string column = dtgRecipients.Columns[e.ColumnIndex].Name;
            string address = Convert.ToString(item[COL_RCP_ADDRESS]);

            if (column == COL_RCP_REMOVE)
            {
                List<string> others = _newGroups.ContainsKey(_shownGroupId) ? new List<string>() : OtherCustomersOn(_shownGroupId);

                if (!frmConfirm.Ask(this,
                    "Remove " + address + " from Group #" + _shownGroupId + "?" +
                    (others.Count > 0 ? Environment.NewLine + "It also stops getting the mail of " + NameList(others) + "." : ""),
                    "Remove", "Cancel")) return;

                StageRemoveMember(_shownGroupId, address);
            }
            else if (column == COL_RCP_TYPE)
            {
                StageSwitchType(_shownGroupId, address);
            }
        }

        private void btnAddEmail_Click(object sender, EventArgs e)
        {
            if (_shownGroupId < 0 || _shownGroupId == GROUP_INTERNAL) return;

            string address = (txtAddEmail.Text ?? "").Trim();

            if (!Regex.IsMatch(address, EMAIL_PATTERN))
            {
                frmError.ShowError(this, "Please type a valid email address.");
                return;
            }

            if (FindMember(_shownGroupId, address) != null)
            {
                frmError.ShowError(this, address + " is already in Group #" + _shownGroupId + ".");
                return;
            }

            int type = Convert.ToString(cbToOrCC.SelectedItem) == "CC" ? RCP_CC : RCP_TO;
            StageAddMember(_shownGroupId, address, type);
            txtAddEmail.Text = "";
        }

        private void btnRcpAction_Click(object sender, EventArgs e)
        {
            if (_recipientAction == RecipientAction.Split) SplitShownGroup();
            else if (_recipientAction == RecipientAction.CreateGroup) CreateDocumentGroup();
        }

        private void btnAllGroups_Click(object sender, EventArgs e)
        {
            if (_emailCon == null) return;

            // every product, not only the one selected
            List<SettingEmailProperty> products = new List<SettingEmailProperty>();
            DataTable source = cbProduct.DataSource as DataTable;
            if (source != null)
            {
                foreach (DataRow row in source.Rows)
                {
                    products.Add(new SettingEmailProperty
                    {
                        productName = row["PRODUCT_NAME"].ToString(),
                        dbProduct = row["DATABASE_NAME"].ToString()
                    });
                }
            }

            DataTable matrix = _emailCon.searchGroupMatrix(products);

            if (!string.IsNullOrEmpty(_emailCon.lastError))
            {
                frmError.ShowError(this, _emailCon.lastError);
                return;
            }

            GroupOverviewDialog.Show(this, matrix);
        }

        // =====================================================================
        // staged edits
        // =====================================================================
        private void StageAddMember(int groupId, string address, int type)
        {
            DataTable members = MembersOf(groupId);
            if (members == null) return;

            DataRow row = members.NewRow();
            if (members.Columns.Contains("EMAIL_ID")) SetCell(row, "EMAIL_ID", -1);   // found or made in info_email on Save
            SetCell(row, COL_RCP_ADDRESS, address);
            SetCell(row, COL_RCP_ID, type);
            SetCell(row, COL_RCP_TYPE, type == RCP_TO ? "TO" : "CC");
            if (members.Columns.Contains("INUSE")) SetCell(row, "INUSE", 1);

            members.Rows.Add(row);
            members.AcceptChanges();
            AfterRecipientsChanged(groupId);
        }

        private void StageRemoveMember(int groupId, string address)
        {
            DataRow row = FindMember(groupId, address);
            if (row == null) return;

            row.Delete();
            row.Table.AcceptChanges();
            AfterRecipientsChanged(groupId);
        }

        private void StageSwitchType(int groupId, string address)
        {
            DataRow row = FindMember(groupId, address);
            if (row == null) return;

            int type = ToInt(row[COL_RCP_ID]) == RCP_TO ? RCP_CC : RCP_TO;
            SetCell(row, COL_RCP_ID, type);
            SetCell(row, COL_RCP_TYPE, type == RCP_TO ? "TO" : "CC");
            row.Table.AcceptChanges();
            AfterRecipientsChanged(groupId);
        }

        private void SplitShownGroup()
        {
            int source = _shownGroupId;
            List<DataRow> targets = _splitTargets;
            if (source < 0 || targets.Count == 0) return;

            if (!frmConfirm.Ask(this,
                "Copy Group #" + source + " into a new group for " + targets.Count + " spec(s) of " + SelectedCustomerName() + "?" +
                Environment.NewLine + "Everyone else keeps Group #" + source + " unchanged.",
                "Split", "Cancel")) return;

            StageSplit(source, targets);
        }

        /// <summary>New group = copy of the members; the specs move to it like any other staged spec change.</summary>
        private void StageSplit(int sourceGroupId, List<DataRow> targets)
        {
            DataTable source = MembersOf(sourceGroupId);
            if (source == null) return;

            int newId = NextGroupId();
            DataTable copy = source.Copy();

            _members[newId] = copy;
            _membersLoaded[newId] = copy.Copy();
            _newGroups[newId] = "Copy of #" + sourceGroupId + " for " + targets.Count + " spec(s)";
            _splitSource[newId] = sourceGroupId;
            AddGroupToLists(newId, copy.Rows.Count);

            foreach (DataRow row in targets)
            {
                row[COL_GROUP] = newId;
                EvaluateRow(row);
            }

            _notice = "Group #" + newId + " (copy of #" + sourceGroupId + ") staged for " + targets.Count +
                      " spec(s) - edit its addresses on the right, then Save.";
            RefreshAll();
        }

        private void CreateDocumentGroup()
        {
            int documentId = SelectedMailType();
            string customer = SelectedCustomerName();
            if (documentId == DOC_LD_DATA || customer == "") return;

            if (!frmConfirm.Ask(this,
                "Create a " + cbMailType.Text + " recipient group for " + customer + " (" + SelectedProductName() + ")?" +
                Environment.NewLine + "Add the recipients, then press Save.",
                "Create", "Cancel")) return;

            StageDocumentGroup(documentId);
        }

        private void StageDocumentGroup(int documentId)
        {
            int newId = NextGroupId();
            DataTable empty = EmptyMembers();

            _members[newId] = empty;
            _membersLoaded[newId] = empty.Copy();
            _newGroups[newId] = "New " + cbMailType.Text + " group of " + SelectedCustomerName();
            _newDocumentGroups[documentId] = newId;

            RefreshRecipients();
            RaiseSaveButtonTextChanged();
        }

        private void AfterRecipientsChanged(int groupId)
        {
            DataTable members;
            if (_members.TryGetValue(groupId, out members))
            {
                SetGroupListText(groupId, members.Rows.Count);
            }

            dtgRecipients.Invalidate();
            RaiseSaveButtonTextChanged();
        }

        /// <summary>Added + removed + switched addresses, plus every new group.</summary>
        private int RecipientChangeCount()
        {
            int count = _newGroups.Count;

            foreach (KeyValuePair<int, DataTable> pair in _members)
            {
                Dictionary<string, int> before = TypeByAddress(_membersLoaded[pair.Key]);
                Dictionary<string, int> now = TypeByAddress(pair.Value);

                count += before.Keys.Count(address => !now.ContainsKey(address));
                count += now.Count(p =>
                {
                    int old;
                    return !before.TryGetValue(p.Key, out old) || old != p.Value;
                });
            }

            return count;
        }

        /// <summary>Drop every staged recipient edit and the groups made here (Cancel / another customer).</summary>
        private void ResetRecipients()
        {
            bool wasLoading = _loading;
            _loading = true;

            if (_groups != null)
            {
                foreach (int id in _newGroups.Keys.ToList())
                {
                    RemoveFromList(_groups, id);
                    RemoveFromList(cbGroup.DataSource as DataTable, id);
                }

                // member counts in the list back to what the database says
                foreach (KeyValuePair<int, DataTable> pair in _membersLoaded)
                {
                    if (!_newGroups.ContainsKey(pair.Key)) SetGroupListText(pair.Key, pair.Value.Rows.Count);
                }
            }

            _loading = wasLoading;

            _members.Clear();
            _membersLoaded.Clear();
            _sharing.Clear();
            _newGroups.Clear();
            _newDocumentGroups.Clear();
            _splitSource.Clear();
            _nextGroupId = -1;
            _shownGroupId = -1;
        }

        // =====================================================================
        // Save
        // =====================================================================
        /// <summary>
        /// Statements of every staged recipient change, in the order they must run: new addresses into
        /// info_email, the members of each group, the log rows. A group made on this screen gets its id now
        /// (the next free one), not the one shown while staging - someone may have made a group since.
        /// groupIds = staged id -> id written. False = a read failed (shown), nothing is saved.
        /// </summary>
        private bool AddRecipientStatements(List<string> batch, string user, out Dictionary<int, int> groupIds)
        {
            groupIds = new Dictionary<int, int>();
            if (_members.Count == 0 && _newGroups.Count == 0) return true;

            string product = SelectedProductName();
            string customer = SelectedCustomerName();

            if (_newGroups.Count > 0)
            {
                int max = _specCon.maxGroupId();
                if (_specCon.lastError != "")
                {
                    frmError.ShowError(this, "Save stopped, nothing was written." + Environment.NewLine + _specCon.lastError);
                    return false;
                }

                foreach (int staged in _newGroups.Keys.OrderBy(id => id))
                {
                    max++;
                    groupIds[staged] = max;
                }
            }

            Dictionary<string, int> newEmailIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int lastEmailId = int.MinValue;   // read once, when the first new address needs an id

            foreach (KeyValuePair<int, DataTable> pair in _members.OrderBy(p => p.Key))
            {
                int staged = pair.Key;
                int groupId = MapGroup(groupIds, staged);
                bool isNew = _newGroups.ContainsKey(staged);

                Dictionary<string, int> before = TypeByAddress(_membersLoaded[staged]);
                Dictionary<string, int> now = TypeByAddress(pair.Value);

                List<string> removed = before.Keys.Where(a => !now.ContainsKey(a)).OrderBy(a => a).ToList();
                List<string> added = now.Keys.Where(a => !before.ContainsKey(a)).OrderBy(a => a).ToList();
                List<string> switched = now.Keys.Where(a => before.ContainsKey(a) && before[a] != now[a]).OrderBy(a => a).ToList();

                if (isNew)
                {
                    // a new group is written whole
                    foreach (DataRow row in pair.Value.Rows)
                    {
                        int emailId = EmailIdFor(row, newEmailIds, ref lastEmailId, batch, user);
                        if (emailId == int.MinValue) return false;
                        batch.Add(_emailSql.insertGroupMember(Member(groupId, emailId, ToInt(row[COL_RCP_ID]))));
                    }

                    int documentId = DocumentOfNewGroup(staged);
                    if (documentId > 0)
                    {
                        // binds the group to product + customer + Serial List / Wafer, with its own log row
                        batch.Add(_emailSql.insertDocumentGroup(new SettingEmailProperty
                        {
                            productName = product,
                            customerName = customer,
                            documentId = documentId,
                            newGroupId = groupId,
                            createUser = user
                        }));
                    }
                    else
                    {
                        int source;
                        batch.Add(_specSql.logNewGroup(product, groupId,
                            _splitSource.TryGetValue(staged, out source) ? source : -1, customer, user));
                    }
                }
                else
                {
                    foreach (string address in removed)
                    {
                        batch.Add(_emailSql.deleteGroupMember(Member(groupId, LoadedEmailId(staged, address), 0)));
                    }

                    foreach (string address in switched)
                    {
                        batch.Add(_emailSql.updateRecipientType(Member(groupId, LoadedEmailId(staged, address), now[address])));
                    }

                    foreach (string address in added)
                    {
                        DataRow row = FindMember(staged, address);
                        int emailId = EmailIdFor(row, newEmailIds, ref lastEmailId, batch, user);
                        if (emailId == int.MinValue) return false;
                        batch.Add(_emailSql.insertGroupMember(Member(groupId, emailId, now[address])));
                    }
                }

                // one log row per address, against the group as it was read (a split group: against its copy)
                foreach (string address in removed)
                {
                    batch.Add(_specSql.logRecipientChange(product, groupId, customer, address, before[address], null, user));
                }
                foreach (string address in switched)
                {
                    batch.Add(_specSql.logRecipientChange(product, groupId, customer, address, before[address], now[address], user));
                }
                foreach (string address in added)
                {
                    batch.Add(_specSql.logRecipientChange(product, groupId, customer, address, null, now[address], user));
                }
            }

            return true;
        }

        /// <summary>
        /// EMAIL_ID of a member: read with the group, or - for an address added here - the one info_email
        /// already has, or a new one (with its INSERT). int.MinValue = a read failed (shown).
        /// </summary>
        private int EmailIdFor(DataRow row, Dictionary<string, int> newEmailIds, ref int lastEmailId, List<string> batch, string user)
        {
            int id = row.Table.Columns.Contains("EMAIL_ID") ? ToInt(row["EMAIL_ID"]) : -1;
            if (id >= 0) return id;

            string address = Convert.ToString(row[COL_RCP_ADDRESS]);
            if (newEmailIds.TryGetValue(address, out id)) return id;

            DataTable found = _emailCon.searchEmailByAddress(new SettingEmailProperty { emailAddress = address });
            if (_emailCon.lastError != "")
            {
                frmError.ShowError(this, "Save stopped, nothing was written." + Environment.NewLine + _emailCon.lastError);
                return int.MinValue;
            }

            if (found.Rows.Count > 0)
            {
                id = ToInt(found.Rows[0]["EMAIL_ID"]);
            }
            else
            {
                if (lastEmailId == int.MinValue)
                {
                    lastEmailId = _emailCon.maxEmailId();
                    if (_emailCon.lastError != "")
                    {
                        frmError.ShowError(this, "Save stopped, nothing was written." + Environment.NewLine + _emailCon.lastError);
                        return int.MinValue;
                    }
                }

                lastEmailId++;
                id = lastEmailId;
                batch.Add(_emailSql.insertEmail(new SettingEmailProperty { emailId = id, emailAddress = address, createUser = user }));
            }

            newEmailIds[address] = id;
            return id;
        }

        private int LoadedEmailId(int groupId, string address)
        {
            DataRow row = _membersLoaded[groupId].Rows.Cast<DataRow>().First(r =>
                string.Equals(Convert.ToString(r[COL_RCP_ADDRESS]), address, StringComparison.OrdinalIgnoreCase));
            return ToInt(row["EMAIL_ID"]);
        }

        /// <summary>Serial List / Wafer document of a group made here; 0 = not one (a Split group).</summary>
        private int DocumentOfNewGroup(int stagedGroupId)
        {
            foreach (KeyValuePair<int, int> pair in _newDocumentGroups)
            {
                if (pair.Value == stagedGroupId) return pair.Key;
            }
            return 0;
        }

        private static SettingEmailProperty Member(int groupId, int emailId, int recipientId)
        {
            return new SettingEmailProperty { groupId = groupId, emailId = emailId, recipientId = recipientId };
        }

        private static int MapGroup(Dictionary<int, int> groupIds, int groupId)
        {
            int written;
            return groupIds.TryGetValue(groupId, out written) ? written : groupId;
        }

        // =====================================================================
        // data helpers
        // =====================================================================
        /// <summary>Members of a group, read from the database the first time.</summary>
        private DataTable MembersOf(int groupId)
        {
            DataTable members;
            if (_members.TryGetValue(groupId, out members)) return members;

            members = _emailCon.searchGroupMembers(new SettingEmailProperty { groupId = groupId });
            if (!string.IsNullOrEmpty(_emailCon.lastError)) return null;

            _members[groupId] = members;
            _membersLoaded[groupId] = members.Copy();
            SetGroupListText(groupId, members.Rows.Count);
            return members;
        }

        /// <summary>Other customers of this product whose Email specs use the group (database, not staged).</summary>
        private List<string> OtherCustomersOn(int groupId)
        {
            List<string> names;
            if (_sharing.TryGetValue(groupId, out names)) return names;

            DataRowView product = cbProduct.SelectedItem as DataRowView;

            DataTable shared = _emailCon.searchCustomersSharingGroup(new SettingEmailProperty
            {
                productName = SelectedProductName(),
                dbProduct = product == null ? "" : product["DATABASE_NAME"].ToString(),
                customerName = SelectedCustomerName(),
                groupId = groupId
            });

            names = shared.Rows.Cast<DataRow>().Select(r => r["CUSTOMER_NAME"].ToString()).ToList();
            _sharing[groupId] = names;
            return names;
        }

        private DataRow FindMember(int groupId, string address)
        {
            DataTable members = MembersOf(groupId);
            if (members == null) return null;

            return members.Rows.Cast<DataRow>().FirstOrDefault(r =>
                string.Equals(Convert.ToString(r[COL_RCP_ADDRESS]), address, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>TO / CC of an address when the group was read; 0 = it was not in the group.</summary>
        private int LoadedType(int groupId, string address)
        {
            DataTable loaded;
            if (!_membersLoaded.TryGetValue(groupId, out loaded)) return 0;

            int type;
            return TypeByAddress(loaded).TryGetValue(address, out type) ? type : 0;
        }

        private static Dictionary<string, int> TypeByAddress(DataTable members)
        {
            Dictionary<string, int> types = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in members.Rows)
            {
                types[Convert.ToString(row[COL_RCP_ADDRESS])] = ToInt(row[COL_RCP_ID]);
            }
            return types;
        }

        private DataTable EmptyMembers()
        {
            // same columns as searchGroupMembers
            DataTable any = _membersLoaded.Values.FirstOrDefault();
            if (any != null) return any.Clone();

            DataTable table = new DataTable();
            table.Columns.Add("EMAIL_ID", typeof(int));
            table.Columns.Add(COL_RCP_ADDRESS, typeof(string));
            table.Columns.Add(COL_RCP_ID, typeof(int));
            table.Columns.Add(COL_RCP_TYPE, typeof(string));
            table.Columns.Add("INUSE", typeof(int));
            return table;
        }

        /// <summary>The database decides the column types (int / sbyte / bool ...), so values are converted to them.</summary>
        private static void SetCell(DataRow row, string column, object value)
        {
            row[column] = Convert.ChangeType(value, row.Table.Columns[column].DataType);
        }

        private int NextGroupId()
        {
            if (_nextGroupId < 0)
            {
                int max = _specCon.maxGroupId();
                foreach (DataRow row in _groups.Rows)
                {
                    max = Math.Max(max, ToInt(row["ID"]));
                }
                _nextGroupId = max;
            }

            _nextGroupId++;
            return _nextGroupId;
        }

        // ---- the Recipient Group list on the left follows the member counts ----

        private static string GroupListText(int groupId, int memberCount, bool isNew)
        {
            string count = memberCount + (memberCount == 1 ? " address" : " addresses");
            if (groupId == GROUP_INTERNAL) return "#0   internal only (" + count + ")";
            return "#" + groupId + "   (" + (isNew ? "new, " : "") + count + ")";
        }

        private void AddGroupToLists(int groupId, int memberCount)
        {
            bool wasLoading = _loading;
            _loading = true;

            string text = GroupListText(groupId, memberCount, true);
            _groups.Rows.Add(groupId, text, "#" + groupId);

            DataTable list = cbGroup.DataSource as DataTable;
            if (list != null) list.Rows.Add(groupId, text);

            _loading = wasLoading;
        }

        /// <summary>Serial List / Wafer groups are not in the spec list, so they are skipped.</summary>
        private void SetGroupListText(int groupId, int memberCount)
        {
            DataRow lookupRow = FindId(_groups, groupId);
            if (lookupRow == null) return;

            string text = GroupListText(groupId, memberCount, _newGroups.ContainsKey(groupId));
            if (lookupRow["LIST"].ToString() == text) return;

            bool wasLoading = _loading;
            _loading = true;

            lookupRow["LIST"] = text;
            DataRow listRow = FindId(cbGroup.DataSource as DataTable, groupId);
            if (listRow != null) listRow["LIST"] = text;

            _loading = wasLoading;
        }

        private static void RemoveFromList(DataTable table, int id)
        {
            DataRow row = FindId(table, id);
            if (row != null) table.Rows.Remove(row);
        }
    }
}
