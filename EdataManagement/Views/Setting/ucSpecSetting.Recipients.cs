using EdataManagement.Controllers;
using EdataManagement.Property;
using EdataManagement.Utilities;

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
    /// Add / Remove / TO-CC / New Group / Create Group are staged with the spec edits and written by the same Save.
    /// A group is shared, so a member change reaches every spec / customer on it: sharing is always spelled out,
    /// and Group #0 (internal, also used by other programs such as uITLA) is edited like any group but every Add /
    /// Remove on it is confirmed. New Group (an empty group for the ticked specs, offered on every saved Edata group)
    /// and Create Group (Serial List / Wafer) are for ADMIN only; Save refuses a new group without a TO.
    ///
    /// Save writes them before the spec values (AddRecipientStatements); info_group_email has no trigger,
    /// so every change gets its own row in log_setting_edata.
    /// A group can have a name (info_group_name, read by this page only - sending never needs it): typed in the box
    /// under the note, staged like the rest, optional, anyone may set it. A database without the table shows no names.
    /// </summary>
    public partial class ucSpecSetting
    {
        private const int DOC_LD_DATA = 1;
        private const int RCP_TO = 1;
        private const int RCP_CC = 2;
        private const int NOTE_MAX_NAMES = 3;
        private const int CONFIRM_MAX_NAMES = 15;

        private const string COL_RCP_REMOVE = "colRemove";
        private const string COL_RCP_TYPE = "RECIPIENT_TYPE";
        private const string COL_RCP_ID = "RECIPIENT_ID";
        private const string COL_RCP_ADDRESS = "EMAIL_ADDRESS";

        private const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        private enum RecipientAction { None, NewGroup, CreateGroup }

        private SettingEmailControllers _emailCon;

        /// <summary>Members of every group looked at: as edited on screen / as read from the database.</summary>
        private readonly Dictionary<int, DataTable> _members = new Dictionary<int, DataTable>();
        private readonly Dictionary<int, DataTable> _membersLoaded = new Dictionary<int, DataTable>();

        /// <summary>Other customers of the same product on a group, read once.</summary>
        private readonly Dictionary<string, List<string>> _sharing = new Dictionary<string, List<string>>();

        /// <summary>Groups made on this screen (New Group / Create Group), not in the database yet: id -> what it is.</summary>
        private readonly Dictionary<int, string> _newGroups = new Dictionary<int, string>();

        /// <summary>Serial List / Wafer group made here for the current customer: document id -> group id.</summary>
        private readonly Dictionary<int, int> _newDocumentGroups = new Dictionary<int, int>();

        private int _nextGroupId = -1;

        /// <summary>Group on the panel; -1 = a message instead of a group.</summary>
        private int _shownGroupId = -1;

        private RecipientAction _recipientAction = RecipientAction.None;

        /// <summary>Specs that New Group moves to the new group.</summary>
        private List<DataRow> _newGroupTargets = new List<DataRow>();

        private const int GROUP_NAME_MAX = 100;

        /// <summary>Group names as read (info_group_name) / typed here and not saved yet (staged group id -> name, "" = no name).</summary>
        private readonly Dictionary<int, string> _groupNames = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _groupNamesStaged = new Dictionary<int, string>();

        /// <summary>False when the database has no info_group_name: no names are shown and none can be typed.</summary>
        private bool _groupNamesReadable;

        /// <summary>Set while the page itself fills txtGroupName, so that is not taken as a rename.</summary>
        private bool _fillingGroupName;

        private Label lbGroupName;
        private TextBox txtGroupName;

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
            SetupGroupNameRow();

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

        /// <summary>
        /// "ชื่อ group" + a box between the note and the member grid (made here, not in the designer);
        /// the grid gives up the room for it.
        /// </summary>
        private void SetupGroupNameRow()
        {
            const int rowTop = 72;
            const int rowHeight = 28;

            lbGroupName = new Label();
            lbGroupName.AutoSize = false;
            lbGroupName.Text = "ชื่อ group";
            lbGroupName.Font = new Font(SettingUi.FONT_NAME, 9F, FontStyle.Regular);
            lbGroupName.ForeColor = SettingUi.GRID_TEXT;
            lbGroupName.TextAlign = ContentAlignment.MiddleLeft;
            lbGroupName.SetBounds(dtgRecipients.Left, rowTop, 74, rowHeight);

            txtGroupName = new TextBox();
            txtGroupName.Font = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Regular);
            txtGroupName.MaxLength = GROUP_NAME_MAX;
            txtGroupName.BorderStyle = BorderStyle.FixedSingle;
            txtGroupName.SetBounds(lbGroupName.Right + 2, rowTop + 2, dtgRecipients.Right - lbGroupName.Right - 2, rowHeight - 4);
            txtGroupName.TextChanged += txtGroupName_TextChanged;

            pnRecipients.Controls.Add(lbGroupName);
            pnRecipients.Controls.Add(txtGroupName);

            int gridTop = rowTop + rowHeight + 6;
            dtgRecipients.Height -= gridTop - dtgRecipients.Top;
            dtgRecipients.Top = gridTop;

            SetGroupNameRow(false);
        }

        private void SetGroupNameRow(bool visible)
        {
            lbGroupName.Visible = visible;
            txtGroupName.Visible = visible;
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

            _newGroupTargets = new List<DataRow>();

            string customer = SelectedCustomerName();
            if (_specs == null || customer == "")
            {
                ShowRecipientsMessage("Recipients", "ยังไม่ได้เลือกลูกค้า");
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
                ShowRecipientsMessage("Recipients", "spec ที่ติ๊กใช้ Send Method ต่างกัน" +
                    Environment.NewLine + Environment.NewLine + (EMAIL_ONLY
                        ? "ติ๊กเฉพาะ spec ที่ส่ง Email เพื่อดูผู้รับ"
                        : "เลือก Email หรือติ๊กเฉพาะ spec ที่ส่ง Email เพื่อดูผู้รับ"));
                return;
            }

            if (send != SEND_EMAIL)
            {
                ShowRecipientsMessage("Recipients", "Send Method: " + ListText(_sendMethods, send) +
                    Environment.NewLine + Environment.NewLine + "ผู้รับใช้กับการส่ง Email เท่านั้น");
                return;
            }

            int groupId = SelectedId(cbGroup);
            if (groupId == KEEP)
            {
                List<int> groups = ticked.Select(r => ToInt(r[COL_GROUP])).Distinct().OrderBy(g => g).ToList();

                ShowRecipientsMessage("Recipients", groups.Count > 1
                    ? "spec ที่ติ๊กใช้ " + groups.Count + " group: " + string.Join(", ", groups.Select(g => "#" + g)) +
                      Environment.NewLine + Environment.NewLine + "เลือก Recipient Group เดียวเพื่อดูและแก้ผู้รับ"
                    : "ไม่พบ Group #" + groups[0] + " ในฐานข้อมูล");
                return;
            }

            _newGroupTargets = ticked;

            int notOnIt = ticked.Count(r => ToInt(r[COL_GROUP]) != groupId);
            ShowGroup(groupId, notOnIt > 0
                ? "เลือกให้ " + notOnIt + " spec ที่ติ๊ก กด Apply เพื่อย้ายมา group นี้"
                : "ใช้กับ " + ticked.Count + " spec ที่ติ๊ก", true);
        }

        /// <summary>Nothing ticked: the group most Email specs of the customer use, like the old Email Sending page.</summary>
        private void ShowCustomerGroup(string customer)
        {
            List<DataRow> email = _specs.Rows.Cast<DataRow>().Where(r => ToInt(r[COL_SEND]) == SEND_EMAIL).ToList();

            if (email.Count == 0)
            {
                ShowRecipientsMessage("Recipients", customer + " ไม่มี spec ที่ส่ง Email" +
                    Environment.NewLine + Environment.NewLine + (EMAIL_ONLY
                        ? "หน้านี้ยังเปลี่ยน Send Method ไม่ได้"
                        : "ติ๊ก spec แล้วตั้ง Send Method = Email เพื่อเลือกผู้รับ"));
                return;
            }

            List<IGrouping<int, DataRow>> byGroup = email
                .GroupBy(r => ToInt(r[COL_GROUP]))
                .OrderByDescending(g => g.Count())
                .ToList();

            int groupId = byGroup[0].Key;
            _newGroupTargets = byGroup[0].ToList();

            ShowGroup(groupId, byGroup.Count > 1
                ? "spec Email ของ " + customer + " ใช้ " + byGroup.Count + " group นี่คือ group ของ " + _newGroupTargets.Count + " spec ติ๊ก spec เพื่อดู group อื่น"
                : "ใช้กับ " + email.Count + " spec Email ของ " + customer + " ติ๊ก spec เพื่อเปลี่ยน group", true);
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
                    ShowRecipientsMessage("Recipients", "อ่าน group " + typeName + " ไม่ได้" + Environment.NewLine + _emailCon.lastError);
                    return;
                }

                if (found.Rows.Count == 0)
                {
                    bool admin = SettingUi.IsAdmin();
                    ShowRecipientsMessage("Recipients", "ยังไม่มีผู้รับ " + typeName + " ของ " + customer + " (" + product + ") ในฐานข้อมูล" +
                        Environment.NewLine + Environment.NewLine + "เมล " + typeName + " ของลูกค้านี้จะส่งไม่ได้จนกว่าจะมีผู้รับ " +
                        (admin ? "กด Create Group เพื่อสร้าง" : "ติดต่อ ADMIN ให้สร้าง group"));
                    SetRecipientNote("", SettingUi.ALERT_NONE_TEXT, admin ? RecipientAction.CreateGroup : RecipientAction.None);
                    return;
                }

                groupId = ToInt(found.Rows[0]["GROUP_ID"]);
            }

            ShowGroup(groupId, typeName + " ของ " + customer + " ส่งเป็นเมลแยก ไม่ได้ใช้ group ของ spec", false);
        }

        // =====================================================================
        // panel content
        // =====================================================================
        private void ShowGroup(int groupId, string contextNote, bool forSpecs)
        {
            DataTable members = MembersOf(groupId);
            if (members == null)
            {
                ShowRecipientsMessage("Recipients", "อ่าน Group #" + groupId + " ไม่ได้" + Environment.NewLine + _emailCon.lastError);
                return;
            }

            bool isNew = _newGroups.ContainsKey(groupId);
            bool internalGroup = groupId == GROUP_INTERNAL;

            _shownGroupId = groupId;
            lbRcpTitle.Text = "Group #" + groupId + (isNew ? "  (ใหม่)" : "");

            lbRcpEmpty.Visible = false;
            dtgRecipients.Visible = true;

            _fillingGroupName = true;
            txtGroupName.Text = NameOf(groupId);
            _fillingGroupName = false;
            txtGroupName.Enabled = _groupNamesReadable;
            toolTipAlert.SetToolTip(txtGroupName, _groupNamesReadable
                ? "ชื่อที่แสดงในหน้า Setting เท่านั้น ไม่มีผลกับการส่งเมล (เว้นว่าง = ไม่มีชื่อ)"
                : "ฐานข้อมูล " + MailSafety.ConfigDatabase + " ยังไม่มีตาราง info_group_name");
            SetGroupNameRow(true);

            // TO first, then CC - reads like the mail header
            members.DefaultView.Sort = COL_RCP_ID + " ASC, " + COL_RCP_ADDRESS + " ASC";
            if (dtgRecipients.DataSource != members) dtgRecipients.DataSource = members;
            FormatRecipientGrid();
            dtgRecipients.Invalidate();

            SetAddRow(true, true);

            // what an edit here reaches, most important first; New Group on every saved group of specs (ADMIN)
            List<string> others = isNew ? new List<string>() : OtherCustomersOn(groupId);
            bool admin = SettingUi.IsAdmin();
            RecipientAction newGroup = forSpecs && admin && !isNew && _newGroupTargets.Count > 0 ? RecipientAction.NewGroup : RecipientAction.None;

            if (internalGroup)
            {
                string note = "#0 ใช้ร่วมกัน (ลูกค้าอื่น, uITLA) แก้ที่นี่มีผลกับทุกที่ที่ใช้ #0";
                SetRecipientNote(note, SettingUi.ALERT_WARN_TEXT, newGroup,
                    note + Environment.NewLine +
                    (admin ? "กด New Group เพื่อแยก group ให้ spec เหล่านี้" : "ติดต่อ ADMIN เพื่อขอ group ใหม่"));
            }
            else if (isNew)
            {
                SetRecipientNote(_newGroups[groupId] + " - กด Save เพื่อสร้าง", SettingUi.ALERT_INFO_TEXT, RecipientAction.None);
            }
            else if (others.Count > 0)
            {
                SetRecipientNote("ใช้ร่วมกับ " + NameList(others) + " - เพิ่ม/ลบที่นี่มีผลกับเมลของลูกค้าเหล่านี้ด้วย",
                    SettingUi.ALERT_WARN_TEXT, newGroup,
                    "Group #" + groupId + " ส่งให้ลูกค้าเหล่านี้ด้วย:" + Environment.NewLine + FullList(others));
            }
            else
            {
                SetRecipientNote(contextNote, SettingUi.ALERT_NONE_TEXT, newGroup);
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

            SetGroupNameRow(false);
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
            SettingUi.SetButtonEnabled(btnAddEmail, enabled);
        }

        /// <summary>
        /// The line under the title, with New Group / Create Group next to it when that is the way out (ADMIN only).
        /// tooltip = the whole text when the line is cut short (default: the line itself).
        /// </summary>
        private void SetRecipientNote(string text, Color fore, RecipientAction action, string tooltip = null)
        {
            _recipientAction = action;

            btnRcpAction.Visible = action != RecipientAction.None;
            if (action == RecipientAction.NewGroup) btnRcpAction.ButtonText = "New Group";
            if (action == RecipientAction.CreateGroup) btnRcpAction.ButtonText = "Create Group";

            int rightEdge = btnRcpAction.Visible ? btnRcpAction.Left - 8 : pnRecipients.ClientSize.Width - 12;
            lbRcpNote.Width = Math.Max(120, rightEdge - lbRcpNote.Left);
            lbRcpNote.ForeColor = fore;
            lbRcpNote.Text = text;
            toolTipAlert.SetToolTip(lbRcpNote, tooltip ?? text);
        }

        private static string NameList(List<string> names)
        {
            string text = string.Join(", ", names.Take(NOTE_MAX_NAMES).ToArray());
            if (names.Count > NOTE_MAX_NAMES) text += " และอีก " + (names.Count - NOTE_MAX_NAMES);
            return text;
        }

        /// <summary>One name per line for the tooltip / confirm boxes, capped so the box stays on screen.</summary>
        private static string FullList(List<string> names)
        {
            string text = string.Join(Environment.NewLine, names.Take(CONFIRM_MAX_NAMES).Select(n => "  - " + n).ToArray());
            if (names.Count > CONFIRM_MAX_NAMES) text += Environment.NewLine + "  ... และอีก " + (names.Count - CONFIRM_MAX_NAMES);
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
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _shownGroupId < 0) return;

            DataRowView item = dtgRecipients.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item == null) return;

            string column = dtgRecipients.Columns[e.ColumnIndex].Name;
            if (column == COL_RCP_REMOVE || column == COL_RCP_TYPE) dtgRecipients.Cursor = Cursors.Hand;
        }

        /// <summary>TYPE switches TO / CC, Remove takes the address out of the group.</summary>
        private void dtgRecipients_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (_shownGroupId < 0) return;

            DataRowView item = dtgRecipients.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item == null) return;

            string column = dtgRecipients.Columns[e.ColumnIndex].Name;
            string address = Convert.ToString(item[COL_RCP_ADDRESS]);

            if (column == COL_RCP_REMOVE)
            {
                List<string> others = _newGroups.ContainsKey(_shownGroupId) ? new List<string>() : OtherCustomersOn(_shownGroupId);

                if (!frmConfirm.Ask(this,
                    "ลบ " + address + " ออกจาก Group #" + _shownGroupId + "?" +
                    (others.Count > 0
                        ? Environment.NewLine + "จะไม่ได้รับเมลของ:" + Environment.NewLine + FullList(others)
                        : "") +
                    (_shownGroupId == GROUP_INTERNAL ? Environment.NewLine + "รวมถึงเมลจากโปรแกรมอื่นที่ใช้ #0 (เช่น uITLA)" : "") +
                    Environment.NewLine + "ไม่กระทบ group อื่น",
                    "ลบ", "ยกเลิก")) return;

                StageRemoveMember(_shownGroupId, address);
            }
            else if (column == COL_RCP_TYPE)
            {
                StageSwitchType(_shownGroupId, address);
            }
        }

        private void btnAddEmail_Click(object sender, EventArgs e)
        {
            if (_shownGroupId < 0) return;

            string address = (txtAddEmail.Text ?? "").Trim();

            if (!Regex.IsMatch(address, EMAIL_PATTERN))
            {
                frmError.ShowError(this, "รูปแบบอีเมลไม่ถูกต้อง");
                return;
            }

            if (FindMember(_shownGroupId, address) != null)
            {
                frmError.ShowError(this, address + " มีอยู่ใน Group #" + _shownGroupId + " แล้ว");
                return;
            }

            // a shared group: the new address gets the mail of everyone on it, not only this customer's
            // (#0 also goes to other programs, so it is always asked)
            bool internalGroup = _shownGroupId == GROUP_INTERNAL;
            List<string> others = _newGroups.ContainsKey(_shownGroupId) ? new List<string>() : OtherCustomersOn(_shownGroupId);
            if ((others.Count > 0 || internalGroup) && !frmConfirm.Ask(this,
                    "เพิ่ม " + address + " ใน Group #" + _shownGroupId + "?" +
                    (others.Count > 0 ? Environment.NewLine + "จะได้รับเมลของ:" + Environment.NewLine + FullList(others) : "") +
                    (internalGroup ? Environment.NewLine + "รวมถึงเมลจากโปรแกรมอื่นที่ใช้ #0 (เช่น uITLA)" : ""),
                    "เพิ่ม", "ยกเลิก")) return;

            int type = Convert.ToString(cbToOrCC.SelectedItem) == "CC" ? RCP_CC : RCP_TO;
            StageAddMember(_shownGroupId, address, type);
            txtAddEmail.Text = "";
        }

        private void btnRcpAction_Click(object sender, EventArgs e)
        {
            // the button is only shown to ADMIN; checked again in case the role changed while the page was open
            if (!SettingUi.IsAdmin()) return;

            if (_recipientAction == RecipientAction.NewGroup) NewGroupForSpecs();
            else if (_recipientAction == RecipientAction.CreateGroup) CreateDocumentGroup();
        }

        private void btnAllGroups_Click(object sender, EventArgs e)
        {
            if (_emailCon == null) return;

            DataTable matrix = _emailCon.searchGroupMatrix(AllProducts());

            if (!string.IsNullOrEmpty(_emailCon.lastError))
            {
                frmError.ShowError(this, _emailCon.lastError);
                return;
            }

            // names come from memory (as on screen, typed ones too), so the query works on a database without info_group_name
            matrix.Columns.Add("GROUP_NAME", typeof(string));
            foreach (DataRow row in matrix.Rows)
            {
                row["GROUP_NAME"] = NameOf(ToInt(row["GROUP_ID"]));
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

        private void NewGroupForSpecs()
        {
            int current = _shownGroupId;
            List<DataRow> targets = _newGroupTargets;
            if (current < 0 || targets.Count == 0) return;

            if (!frmConfirm.Ask(this,
                "สร้าง group ใหม่ (ว่าง) ให้ " + targets.Count + " spec ของ " + SelectedCustomerName() + "?" +
                Environment.NewLine + "เพิ่มผู้รับ (มี TO อย่างน้อย 1) แล้วกด Save  spec อื่นยังใช้ Group #" + current + " เหมือนเดิม",
                "สร้าง", "ยกเลิก")) return;

            StageNewGroup(targets);
        }

        /// <summary>New group starts empty (nothing is copied); the specs move to it like any other staged spec change.</summary>
        private void StageNewGroup(List<DataRow> targets)
        {
            int newId = NextGroupId();
            DataTable empty = EmptyMembers();

            _members[newId] = empty;
            _membersLoaded[newId] = empty.Copy();
            _newGroups[newId] = "group ใหม่ของ " + targets.Count + " spec";
            AddGroupToLists(newId, 0);

            foreach (DataRow row in targets)
            {
                row[COL_GROUP] = newId;
                EvaluateRow(row);
            }

            _notice = "สร้าง Group #" + newId + " (ใหม่, ว่าง) ให้ " + targets.Count +
                      " spec แล้ว เพิ่มอีเมลทางขวา แล้วกด Save";
            RefreshAll();
        }

        private void CreateDocumentGroup()
        {
            int documentId = SelectedMailType();
            string customer = SelectedCustomerName();
            if (documentId == DOC_LD_DATA || customer == "") return;

            if (!frmConfirm.Ask(this,
                "สร้าง group ผู้รับ " + cbMailType.Text + " ของ " + customer + " (" + SelectedProductName() + ")?" +
                Environment.NewLine + "เพิ่มผู้รับ แล้วกด Save",
                "สร้าง", "ยกเลิก")) return;

            StageDocumentGroup(documentId);
        }

        private void StageDocumentGroup(int documentId)
        {
            int newId = NextGroupId();
            DataTable empty = EmptyMembers();

            _members[newId] = empty;
            _membersLoaded[newId] = empty.Copy();
            _newGroups[newId] = "group " + cbMailType.Text + " ใหม่ของ " + SelectedCustomerName();
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

        /// <summary>New groups (New Group / Create Group) that have no TO address yet - Save refuses them.</summary>
        private List<int> NewGroupsWithoutTo()
        {
            return _newGroups.Keys
                .Where(id => !_members.ContainsKey(id) ||
                             !_members[id].Rows.Cast<DataRow>().Any(r => ToInt(r[COL_RCP_ID]) == RCP_TO))
                .OrderBy(id => id)
                .ToList();
        }

        /// <summary>Drop every staged recipient edit and the groups made here (Cancel / another customer).</summary>
        private void ResetRecipients()
        {
            bool wasLoading = _loading;
            _loading = true;

            _groupNamesStaged.Clear();

            if (_groups != null)
            {
                foreach (int id in _newGroups.Keys.ToList())
                {
                    RemoveFromList(_groups, id);
                    RemoveFromList(cbGroup.DataSource as DataTable, id);
                }

                // member counts (and names) in the list back to what the database says
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
                    frmError.ShowError(this, SAVE_STOPPED + Environment.NewLine + _specCon.lastError);
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
                        batch.Add(_specSql.logNewGroup(product, groupId, customer, user));
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

                // one log row per address, against the group as it was read (a new group: every address is "added")
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
                frmError.ShowError(this, SAVE_STOPPED + Environment.NewLine + _emailCon.lastError);
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
                        frmError.ShowError(this, SAVE_STOPPED + Environment.NewLine + _emailCon.lastError);
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

        /// <summary>Serial List / Wafer document of a group made here; 0 = not one (a New Group for specs).</summary>
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

        /// <summary>
        /// Everyone else who gets mail from the group (database, not staged): Email specs of every product and the
        /// Serial List / Wafer groups, as "CUSTOMER (product)" / "CUSTOMER (product Serial List)". The mail being
        /// edited is left out. A failed read is listed too, so the warning still shows.
        /// </summary>
        private List<string> OtherCustomersOn(int groupId)
        {
            int documentId = SelectedMailType();
            string cacheKey = groupId + "|" + documentId;

            List<string> names;
            if (_sharing.TryGetValue(cacheKey, out names)) return names;

            DataRowView product = cbProduct.SelectedItem as DataRowView;

            DataTable shared = _emailCon.searchCustomersSharingGroup(new SettingEmailProperty
            {
                productName = SelectedProductName(),
                dbProduct = product == null ? "" : product["DATABASE_NAME"].ToString(),
                customerName = SelectedCustomerName(),
                documentId = documentId,
                groupId = groupId,
                products = AllProducts()
            });

            if (!string.IsNullOrEmpty(_emailCon.lastError))
            {
                // not cached: the next look tries again
                return new List<string> { "(ตรวจไม่ได้ว่าใครใช้ Group #" + groupId + " อีก)" };
            }

            names = shared.Rows.Cast<DataRow>().Select(r =>
            {
                string docName = Convert.ToString(r["DOCUMENT_NAME"]);
                return r["CUSTOMER_NAME"] + " (" + r["PRODUCT"] + (docName == "" ? "" : " " + docName) + ")";
            }).ToList();

            _sharing[cacheKey] = names;
            return names;
        }

        /// <summary>Every product of the Product list (name + database), not only the one selected.</summary>
        private List<SettingEmailProperty> AllProducts()
        {
            List<SettingEmailProperty> products = new List<SettingEmailProperty>();
            DataTable source = cbProduct.DataSource as DataTable;
            if (source == null) return products;

            foreach (DataRow row in source.Rows)
            {
                products.Add(new SettingEmailProperty
                {
                    productName = row["PRODUCT_NAME"].ToString(),
                    dbProduct = row["DATABASE_NAME"].ToString()
                });
            }
            return products;
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

        // ---- group names ----

        /// <summary>Read every name once per list load; a failed read (no info_group_name) = no names, nothing to save.</summary>
        private void LoadGroupNames()
        {
            _groupNames.Clear();

            DataTable names = _specCon.searchGroupNames();
            _groupNamesReadable = _specCon.lastError == "";

            foreach (DataRow row in names.Rows)
            {
                _groupNames[ToInt(row["GROUP_ID"])] = Convert.ToString(row["GROUP_NAME"]).Trim();
            }
        }

        /// <summary>Name on screen: typed here, else as read; "" = none.</summary>
        private string NameOf(int groupId)
        {
            string name;
            if (_groupNamesStaged.TryGetValue(groupId, out name)) return name;
            return _groupNames.TryGetValue(groupId, out name) ? name : "";
        }

        private string LoadedNameOf(int groupId)
        {
            string name;
            return _groupNames.TryGetValue(groupId, out name) ? name : "";
        }

        private void txtGroupName_TextChanged(object sender, EventArgs e)
        {
            if (_fillingGroupName || _shownGroupId < 0 || !_groupNamesReadable) return;

            string name = Regex.Replace(txtGroupName.Text ?? "", @"\s+", " ").Trim();

            // typed back to what the database has = nothing to save for it
            if (name == LoadedNameOf(_shownGroupId)) _groupNamesStaged.Remove(_shownGroupId);
            else _groupNamesStaged[_shownGroupId] = name;

            DataTable members;
            if (_members.TryGetValue(_shownGroupId, out members)) SetGroupListText(_shownGroupId, members.Rows.Count);
            RaiseSaveButtonTextChanged();
        }

        /// <summary>Names typed here that differ from the database (a new group with a name counts once).</summary>
        private int GroupNameChangeCount()
        {
            return _groupNamesStaged.Count(p => p.Value != LoadedNameOf(p.Key));
        }

        /// <summary>One upsert / delete + one log row per changed name; a new group's name goes to the id it gets at Save.</summary>
        private void AddGroupNameStatements(List<string> batch, string user, Dictionary<int, int> groupIds)
        {
            if (!_groupNamesReadable) return;

            string product = SelectedProductName();

            foreach (KeyValuePair<int, string> pair in _groupNamesStaged.OrderBy(p => p.Key))
            {
                string oldName = LoadedNameOf(pair.Key);
                if (pair.Value == oldName) continue;

                int groupId = MapGroup(groupIds, pair.Key);
                batch.Add(_specSql.saveGroupName(groupId, pair.Value, user));
                batch.Add(_specSql.logGroupNameChange(product, groupId, oldName, pair.Value, user));
            }
        }

        // ---- the Recipient Group list on the left follows the member counts and names ----

        /// <summary>"#12   QA Coherent  (3 อีเมล)" - number, name if any, how many addresses; nothing more.</summary>
        private string GroupListText(int groupId, int memberCount)
        {
            string name = NameOf(groupId);
            return "#" + groupId + "   " + (name == "" ? "" : name + "  ") + "(" + memberCount + " อีเมล)";
        }

        private void AddGroupToLists(int groupId, int memberCount)
        {
            bool wasLoading = _loading;
            _loading = true;

            string text = GroupListText(groupId, memberCount);
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

            string text = GroupListText(groupId, memberCount);
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
