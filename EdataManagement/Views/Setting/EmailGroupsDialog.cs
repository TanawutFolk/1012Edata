using EdataManagement.Controllers;
using EdataManagement.Property;
using EdataManagement.SQLFactory;
using EdataManagement.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CheckBoxState = System.Windows.Forms.VisualStyles.CheckBoxState;

namespace EdataManagement.Views.Setting
{
    /// <summary>
    /// "Manage Recipients" window - the only place recipients are edited. Two ways into the same data:
    ///   left   every group; open an address (box on top) and tick / untick / TO-CC it on as many groups as needed
    ///   right  the members of the group clicked on the left; add / remove / TO-CC as many addresses as needed
    /// Every member of every group is held in memory (as read / as edited), so edits from both sides pile up and one
    /// Save writes them all. Also here: the name of a group (double-click GROUP; info_group_name, shown in Setting
    /// only) and, for ADMIN, "+ Group ใหม่" - a new group is written with its first members and needs a TO (the
    /// database has no group table, a group exists through its members). Specs are moved to it on the Spec page.
    /// Writes insert / delete / TO-CC of info_group_email, new info_email rows, info_group_name, and one
    /// log_setting_edata row per change (PRODUCT = BY EMAIL, CUSTOMER = who gets the group's mail), in one transaction.
    /// Opened by ucSpecSetting only while that page has nothing staged; Open returns how many changes were saved.
    /// </summary>
    internal class EmailGroupsDialog : Form
    {
        private const int RCP_TO = 1;
        private const int RCP_CC = 2;
        private const int GROUP_INTERNAL = 0;
        private const int GROUP_NAME_MAX = 100;
        private const int CONFIRM_MAX_LINES = 14;
        private const int USED_BY_MAX_CHARS = 90;
        private const int MEMBER_PANEL_WIDTH = 470;

        private const string LOG_PRODUCT = "BY EMAIL";
        private const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        private const string SAVE_STOPPED = "บันทึกไม่สำเร็จ ยังไม่มีข้อมูลถูกเขียน";
        private const string HINT = "ซ้าย: เลือกอีเมลแล้วติ๊ก group (คลิกหัวคอลัมน์ติ๊ก / TYPE = ทุก group ที่แสดง)  ·  ขวา: คลิก group แล้วเพิ่ม / ลบคน  ·  คลิก TYPE สลับ TO / CC  ·  ดับเบิลคลิก GROUP ตั้งชื่อ";

        // columns of _groups (left grid)
        private const string COL_CHECK = "CHECKED";
        private const string COL_GROUP_ID = "GROUP_ID";
        private const string COL_GROUP = "GROUP_TEXT";
        private const string COL_TYPE = "TYPE";
        private const string COL_MEMBERS = "MEMBERS";
        private const string COL_USED_BY = "USED_BY";
        private const string COL_CHANGE = "CHANGE";

        // columns of _memberRows (right grid)
        private const string COL_ADDRESS = "EMAIL_ADDRESS";
        private const string COL_REMOVED = "REMOVED";
        private const string COL_ACTION = "colAction";

        /// <summary>An address on a group as read: TO / CC and the EMAIL_ID that is really on that group.</summary>
        private class Member
        {
            public int Type;
            public int EmailId;
        }

        /// <summary>One add / remove / TO-CC switch of one address on one group. 0 = not on the group.</summary>
        private class Change
        {
            public int GroupId;     // a group made here: the id shown while staging, mapped at Save
            public string Address;
            public int EmailId;     // the EMAIL_ID on the group; -1 = an add, the id is found / made at Save
            public int Old;
            public int New;
        }

        private class NameChange
        {
            public int GroupId;
            public string Old;
            public string New;
        }

        private readonly SettingEmailControllers _emailCon = new SettingEmailControllers();
        private readonly SettingSpecControllers _specCon = new SettingSpecControllers();
        private readonly SettingEmailSQLFactory _emailSql = new SettingEmailSQLFactory();
        private readonly SettingSpecSQLFactory _specSql = new SettingSpecSQLFactory();

        private readonly List<SettingEmailProperty> _products;

        /// <summary>One row per group, bound to the left grid; _rowOf finds it by GROUP_ID.</summary>
        private readonly DataTable _groups = new DataTable();
        private readonly DataView _view;
        private readonly Dictionary<int, DataRow> _rowOf = new Dictionary<int, DataRow>();

        /// <summary>Members of the group clicked on the left, bound to the right grid (rebuilt on every change).</summary>
        private readonly DataTable _memberRows = new DataTable();

        /// <summary>Members of every group: as read / as edited here (group -> address -> ...). Addresses ignore case.</summary>
        private readonly Dictionary<int, Dictionary<string, Member>> _loaded = new Dictionary<int, Dictionary<string, Member>>();
        private readonly Dictionary<int, Dictionary<string, int>> _now = new Dictionary<int, Dictionary<string, int>>();

        /// <summary>Who gets each group's mail: "1480 OPLINK", "1480 HISILICON (Serial List)" ...</summary>
        private readonly Dictionary<int, List<string>> _usedBy = new Dictionary<int, List<string>>();

        /// <summary>Addresses of info_email as spelled there, and the EMAIL_ID of each (the first when it is there twice).</summary>
        private readonly List<string> _addresses = new List<string>();
        private readonly Dictionary<string, int> _emailIdOf = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Group names as read / typed here and not saved yet ("" = no name). False = no info_group_name table.</summary>
        private readonly Dictionary<int, string> _names = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _namesStaged = new Dictionary<int, string>();
        private bool _namesReadable;

        /// <summary>Groups made here, not in the database yet; after a Save: the id each one was written with.</summary>
        private readonly List<int> _newGroups = new List<int>();
        private Dictionary<int, int> _writtenGroupIds = new Dictionary<int, int>();

        /// <summary>The address the left side ticks ("" = none) and the group the right side shows (-1 = none).</summary>
        private string _address = "";
        private int _selectedGroup = -1;

        private int _savedCount;

        /// <summary>What the loading sheet covers: the opening page first, this window once it is shown.</summary>
        private Control _busyOwner;

        private readonly AddressPicker cbEmail = new AddressPicker();
        private readonly Label lbEmailInfo = new Label();
        private readonly TextBox txtFind = new TextBox();
        private readonly Label lbCount = new Label();
        private readonly DataGridView grid = new DataGridView();
        private readonly Label lbMemberTitle = new Label();
        private readonly Label lbMemberNote = new Label();
        private readonly DataGridView gridMembers = new DataGridView();
        private readonly AddressPicker cbAdd = new AddressPicker();
        private readonly ComboBox cbAddType = new ComboBox();
        private readonly Button btnAdd = new Button();
        private readonly Label lbSummary = new Label();
        private readonly Button btnSave = new Button();
        private readonly ToolTip toolTip = new ToolTip();
        private readonly Font _bold = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Bold);

        /// <summary>
        /// Opens the window (address = the one to open at once, "" = none; groupId = the group to show, -1 = none);
        /// the number of changes written while it was open (0 = nothing saved).
        /// </summary>
        public static int Open(Control owner, List<SettingEmailProperty> products, string address, int groupId)
        {
            using (EmailGroupsDialog dialog = new EmailGroupsDialog(products))
            {
                // until the window is on screen the loading sheet goes over the page that opens it
                dialog._busyOwner = owner;
                dialog.Shown += delegate { dialog._busyOwner = dialog; };

                string error = dialog.LoadAll();
                if (error != "")
                {
                    frmError.ShowError(owner, "อ่านข้อมูล group ไม่ได้" + Environment.NewLine + error);
                    return 0;
                }

                if (dialog._rowOf.ContainsKey(groupId)) dialog._selectedGroup = groupId;
                if (!string.IsNullOrEmpty(address)) dialog.OpenAddress(address);
                dialog.RefreshRows();
                dialog.Shown += delegate { dialog.SelectGroupRow(); };

                dialog.ShowDialog(owner);
                return dialog._savedCount;
            }
        }

        private EmailGroupsDialog(List<SettingEmailProperty> products)
        {
            _products = products;

            _groups.Columns.Add(COL_CHECK, typeof(bool));
            _groups.Columns.Add(COL_GROUP_ID, typeof(int));
            _groups.Columns.Add(COL_GROUP, typeof(string));
            _groups.Columns.Add(COL_TYPE, typeof(string));
            _groups.Columns.Add(COL_MEMBERS, typeof(string));
            _groups.Columns.Add(COL_USED_BY, typeof(string));
            _groups.Columns.Add(COL_CHANGE, typeof(string));
            _view = new DataView(_groups);

            _memberRows.Columns.Add(COL_ADDRESS, typeof(string));
            _memberRows.Columns.Add(COL_TYPE, typeof(string));
            _memberRows.Columns.Add(COL_CHANGE, typeof(string));
            _memberRows.Columns.Add(COL_REMOVED, typeof(bool));

            BuildUi();
        }

        // =====================================================================
        // layout
        // =====================================================================
        private void BuildUi()
        {
            Text = "Manage Recipients (" + MailSafety.ConfigDatabase + ")";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1340, 680);
            MinimumSize = new Size(1100, 500);
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Regular);

            // ---- top: the address for the left side, search ----
            Panel top = new Panel();
            top.Dock = DockStyle.Top;
            top.Size = new Size(ClientSize.Width, 92);   // full width before + Group ใหม่ is anchored to its right edge
            top.BackColor = Color.White;

            const int labelLeft = 16;
            const int boxLeft = 80;
            const int boxWidth = 360;
            const int buttonLeft = boxLeft + boxWidth + 8;

            top.Controls.Add(MakeLabel("อีเมล", labelLeft, 18));

            // picked in the list or typed + Enter: show on the left where the address is
            cbEmail.SetBounds(boxLeft, 13, boxWidth + 98, 28);
            cbEmail.Placeholder = "ค้นหา Email ที่มีในระบบ หรือ ใส่ Email ใหม่สำหรับเพิ่ม Email ใหม่";
            cbEmail.InfoOf = delegate (string address) { return GroupCountText(address); };
            cbEmail.DimOf = delegate (string address) { return GroupCount(address) == 0; };
            cbEmail.Chosen += delegate (string address, bool fromList) { OpenAddress(address); };
            top.Controls.Add(cbEmail);

            lbEmailInfo.AutoSize = true;
            lbEmailInfo.Location = new Point(buttonLeft + 106, 18);
            lbEmailInfo.ForeColor = SettingUi.TYPE_CC_TEXT;
            top.Controls.Add(lbEmailInfo);

            top.Controls.Add(MakeLabel("ค้นหา", labelLeft, 58));
            txtFind.Location = new Point(boxLeft, 54);
            txtFind.Size = new Size(boxWidth, 26);
            txtFind.TextChanged += delegate { ApplyFilter(); };
            top.Controls.Add(txtFind);
            toolTip.SetToolTip(txtFind, "เลข group, ชื่อ group, product หรือลูกค้า");

            lbCount.AutoSize = true;
            lbCount.Location = new Point(buttonLeft, 58);
            lbCount.ForeColor = SettingUi.TYPE_CC_TEXT;
            top.Controls.Add(lbCount);

            if (SettingUi.IsAdmin())
            {
                Button btnNewGroup = MakeButton("+ Group ใหม่", buttonLeft + 110, 53, 130);
                btnNewGroup.Click += delegate { AddNewGroup(); };
                top.Controls.Add(btnNewGroup);
                toolTip.SetToolTip(btnNewGroup, "สร้าง group ใหม่ แล้วเพิ่มคนทางขวา (ต้องมี TO อย่างน้อย 1) - ADMIN");
            }

            // ---- left: groups ----
            grid.Dock = DockStyle.Fill;
            SettingUi.StyleGrid(grid, 32);
            grid.AutoGenerateColumns = false;
            grid.Columns.Add(MakeCheckColumn());
            grid.Columns.Add(MakeTextColumn(COL_GROUP, "GROUP", 150));
            grid.Columns.Add(MakeTextColumn(COL_TYPE, "TYPE ⇄", 55));
            grid.Columns.Add(MakeTextColumn(COL_MEMBERS, "MEMBERS", 85));
            grid.Columns.Add(MakeTextColumn(COL_USED_BY, "USED BY (ใครได้เมลของ group นี้)", 250));
            grid.Columns.Add(MakeTextColumn(COL_CHANGE, "CHANGE", 110));
            grid.Columns[COL_TYPE].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.DataSource = _view;

            grid.Columns[COL_CHECK].ToolTipText = "ติ๊ก / เอาติ๊กออก ทุก group ที่แสดงอยู่ (ใช้คู่กับช่องค้นหาได้)";
            grid.Columns[COL_TYPE].ToolTipText = "สลับ TO / CC ทุก group ที่ติ๊กและแสดงอยู่: ทุกอันเป็น TO อยู่ = เปลี่ยนเป็น CC, นอกนั้น = เปลี่ยนเป็น TO";

            grid.CellClick += grid_CellClick;
            grid.CellPainting += grid_CellPainting;
            grid.ColumnHeaderMouseClick += grid_ColumnHeaderMouseClick;
            grid.CellDoubleClick += grid_CellDoubleClick;
            grid.CellFormatting += grid_CellFormatting;
            grid.CellMouseEnter += grid_CellMouseEnter;
            grid.CellMouseLeave += delegate { grid.Cursor = Cursors.Default; };

            // ---- right: members of the clicked group ----
            Panel right = new Panel();
            right.Dock = DockStyle.Right;
            right.Size = new Size(MEMBER_PANEL_WIDTH, 400);
            right.BackColor = SettingUi.GRID_ALT_ROW;

            const int pad = 12;
            int inner = MEMBER_PANEL_WIDTH - pad * 2;

            lbMemberTitle.AutoSize = false;
            lbMemberTitle.AutoEllipsis = true;
            lbMemberTitle.Font = new Font(SettingUi.FONT_NAME, 11.25F, FontStyle.Bold);
            lbMemberTitle.ForeColor = SettingUi.GRID_TEXT;
            lbMemberTitle.SetBounds(pad, 8, inner, 24);
            right.Controls.Add(lbMemberTitle);

            lbMemberNote.AutoSize = false;
            lbMemberNote.AutoEllipsis = true;
            lbMemberNote.Font = new Font(SettingUi.FONT_NAME, 8.25F, FontStyle.Regular);
            lbMemberNote.ForeColor = SettingUi.TYPE_CC_TEXT;
            lbMemberNote.SetBounds(pad, 34, inner, 18);
            right.Controls.Add(lbMemberNote);

            gridMembers.SetBounds(pad, 58, inner, 400 - 58 - 50);
            gridMembers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            SettingUi.StyleGrid(gridMembers, 30, 9F, 32);
            gridMembers.AutoGenerateColumns = false;
            gridMembers.Columns.Add(MakeTextColumn(COL_ADDRESS, "EMAIL ADDRESS", 250));
            gridMembers.Columns.Add(MakeTextColumn(COL_TYPE, "TYPE", 50));
            gridMembers.Columns.Add(MakeTextColumn(COL_CHANGE, "CHANGE", 70));
            gridMembers.Columns.Add(SettingUi.MakeButtonColumn(COL_ACTION, "", 55, SettingUi.REMOVE_TEXT));
            gridMembers.Columns[COL_TYPE].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridMembers.DataSource = _memberRows;

            gridMembers.CellClick += gridMembers_CellClick;
            gridMembers.CellDoubleClick += gridMembers_CellDoubleClick;
            gridMembers.CellFormatting += gridMembers_CellFormatting;
            gridMembers.CellMouseEnter += gridMembers_CellMouseEnter;
            gridMembers.CellMouseLeave += delegate { gridMembers.Cursor = Cursors.Default; };
            gridMembers.DataBindingComplete += delegate { gridMembers.ClearSelection(); };
            right.Controls.Add(gridMembers);

            // add row under the grid: address, TO / CC, เพิ่ม
            const int addTop = 400 - 40;
            // picking in the list only fills the box (TO / CC may still be changed); Enter on it or เพิ่ม adds
            cbAdd.SetBounds(pad, addTop + 1, inner - 64 - 80 - 12, 28);
            cbAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cbAdd.Placeholder = "อีเมลที่จะเพิ่มเข้า group นี้";
            cbAdd.InfoOf = delegate (string address)
            {
                return NowType(_selectedGroup, address) != 0 ? "อยู่ใน group นี้แล้ว" : GroupCountText(address);
            };
            cbAdd.DimOf = delegate (string address) { return NowType(_selectedGroup, address) != 0; };
            cbAdd.Chosen += delegate (string address, bool fromList)
            {
                if (!fromList) AddMember();
            };
            right.Controls.Add(cbAdd);

            cbAddType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbAddType.Items.Add("TO");
            cbAddType.Items.Add("CC");
            cbAddType.SelectedIndex = 0;
            cbAddType.SetBounds(cbAdd.Right + 6, addTop + 2, 64, 26);
            cbAddType.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            right.Controls.Add(cbAddType);

            btnAdd.Text = "เพิ่ม";
            btnAdd.SetBounds(cbAddType.Right + 6, addTop, 80, 30);
            btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAdd.Click += delegate { AddMember(); };
            right.Controls.Add(btnAdd);
            SettingUi.StyleButton(btnAdd);

            // ---- bottom: what Save will do ----
            Panel bottom = new Panel();
            bottom.Dock = DockStyle.Bottom;
            bottom.Size = new Size(ClientSize.Width, 64);   // full width before the buttons are anchored to its right edge
            bottom.BackColor = Color.White;

            lbSummary.AutoSize = false;
            lbSummary.Location = new Point(16, 10);
            lbSummary.Size = new Size(ClientSize.Width - 300, 44);
            lbSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lbSummary.ForeColor = SettingUi.TYPE_CC_TEXT;
            lbSummary.TextAlign = ContentAlignment.MiddleLeft;
            bottom.Controls.Add(lbSummary);

            Button btnClose = MakeButton("ปิด", ClientSize.Width - 256, 15, 110);
            btnClose.Height = 34;
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click += delegate { Close(); };
            bottom.Controls.Add(btnClose);

            btnSave.Text = "Save";
            btnSave.Location = new Point(ClientSize.Width - 130, 15);
            btnSave.Size = new Size(110, 34);
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Click += delegate { SaveChanges(); };
            bottom.Controls.Add(btnSave);
            SettingUi.StyleButton(btnSave);
            btnSave.BackColor = SettingUi.BUTTON_PRIMARY;
            btnSave.ForeColor = SettingUi.BUTTON_PRIMARY_TEXT;

            // docking goes from the last one added inwards: (banner) top, bottom, right, the grid takes the rest
            Controls.Add(grid);
            Controls.Add(right);
            Controls.Add(bottom);
            Controls.Add(top);

            // on qa_oc a Save here changes who the customers' mail really goes to
            if (!MailSafety.IsTestMode)
            {
                Label banner = new Label();
                banner.Dock = DockStyle.Top;
                banner.Height = 30;
                banner.BackColor = SettingUi.REMOVE_TEXT;
                banner.ForeColor = Color.White;
                banner.Font = _bold;
                banner.TextAlign = ContentAlignment.MiddleCenter;
                banner.Text = "PRODUCTION (" + MailSafety.ConfigDatabase + ") - Save ที่นี่เปลี่ยนผู้รับเมลที่ส่งให้ลูกค้าจริง";
                Controls.Add(banner);
            }

            CancelButton = btnClose;

            FormClosing += delegate (object s, FormClosingEventArgs e)
            {
                // a new group nobody is on is not written either: it counts as unsaved
                int n = PendingCount() + _newGroups.Count(id => NowOf(id).Count == 0);
                if (n > 0 && !frmConfirm.Ask(this, "ทิ้งการแก้ที่ยังไม่ได้ Save (" + n + " รายการ)?", "ทิ้ง", "แก้ต่อ"))
                {
                    e.Cancel = true;
                }
            };

            RefreshMembers();
            UpdateSummary();
        }

        private static Label MakeLabel(string text, int left, int top)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.Location = new Point(left, top);
            label.ForeColor = SettingUi.GRID_TEXT;
            return label;
        }

        private static Button MakeButton(string text, int left, int top, int width)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new Point(left, top);
            button.Size = new Size(width, 30);
            SettingUi.StyleButton(button);
            return button;
        }

        private static DataGridViewCheckBoxColumn MakeCheckColumn()
        {
            DataGridViewCheckBoxColumn column = new DataGridViewCheckBoxColumn();
            column.Name = COL_CHECK;
            column.DataPropertyName = COL_CHECK;
            column.HeaderText = "";
            column.FillWeight = 28;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            return column;
        }

        private static DataGridViewTextBoxColumn MakeTextColumn(string name, string header, int weight)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.DataPropertyName = name;
            column.HeaderText = header;
            column.FillWeight = weight;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            return column;
        }

        /// <summary>A one-line text prompt; null = cancelled.</summary>
        private string AskText(string title, string question, string value)
        {
            using (Form prompt = new Form())
            {
                prompt.Text = title;
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;
                prompt.ClientSize = new Size(460, 132);
                prompt.BackColor = Color.White;
                prompt.Font = Font;

                Label label = MakeLabel(question, 16, 16);

                TextBox box = new TextBox();
                box.Location = new Point(16, 44);
                box.Size = new Size(428, 26);
                box.MaxLength = GROUP_NAME_MAX;
                box.Text = value;

                Button ok = MakeButton("ตกลง", 224, 88, 105);
                ok.DialogResult = DialogResult.OK;
                Button cancel = MakeButton("ยกเลิก", 339, 88, 105);
                cancel.DialogResult = DialogResult.Cancel;

                prompt.Controls.Add(label);
                prompt.Controls.Add(box);
                prompt.Controls.Add(ok);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = ok;
                prompt.CancelButton = cancel;

                return prompt.ShowDialog(this) == DialogResult.OK ? box.Text : null;
            }
        }

        // =====================================================================
        // read
        // =====================================================================
        /// <summary>Addresses, every member of every group, names and who uses each group; "" = done, else the error.</summary>
        private string LoadAll()
        {
            DataTable emails, members, specs, documents, names;

            using (SettingBusy.Begin(_busyOwner, SettingBusy.LOADING))
            {
                emails = Busy(() => _emailCon.searchEmailList());
                if (_emailCon.lastError != "") return _emailCon.lastError;

                members = Busy(() => _emailCon.searchAllMembers());
                if (_emailCon.lastError != "") return _emailCon.lastError;

                specs = Busy(() => _emailCon.searchGroupMatrix(_products));
                if (_emailCon.lastError != "") return _emailCon.lastError;

                documents = Busy(() => _emailCon.searchAllDocumentGroups());
                if (_emailCon.lastError != "") return _emailCon.lastError;

                // a failed read = a database without info_group_name: no names, none can be typed
                names = Busy(() => _specCon.searchGroupNames());
                _namesReadable = _specCon.lastError == "";
            }

            _names.Clear();
            _namesStaged.Clear();
            foreach (DataRow row in names.Rows)
            {
                _names[ToInt(row["GROUP_ID"])] = Convert.ToString(row["GROUP_NAME"]).Trim();
            }

            _addresses.Clear();
            _emailIdOf.Clear();
            foreach (DataRow row in emails.Rows)
            {
                string address = Convert.ToString(row["EMAIL_ADDRESS"]).Trim();
                if (address == "" || _emailIdOf.ContainsKey(address)) continue;

                _addresses.Add(address);
                _emailIdOf[address] = ToInt(row["EMAIL_ID"]);
            }

            cbEmail.SetItems(_addresses);
            cbAdd.SetItems(_addresses);

            _loaded.Clear();
            _now.Clear();
            _newGroups.Clear();
            foreach (DataRow row in members.Rows)
            {
                int id = ToInt(row["GROUP_ID"]);
                string address = Convert.ToString(row["EMAIL_ADDRESS"]).Trim();
                int type = ToInt(row["RECIPIENT_ID"]);

                Dictionary<string, Member> group;
                if (!_loaded.TryGetValue(id, out group))
                {
                    group = new Dictionary<string, Member>(StringComparer.OrdinalIgnoreCase);
                    _loaded[id] = group;
                }

                // info_email holds a few addresses twice: the first one found stands for the address
                if (group.ContainsKey(address)) continue;

                group[address] = new Member { Type = type, EmailId = ToInt(row["EMAIL_ID"]) };
                NowOf(id)[address] = type;
            }

            _usedBy.Clear();
            foreach (DataRow row in specs.Rows)
            {
                AddUse(ToInt(row["GROUP_ID"]), row["PRODUCT"] + " " + row["CUSTOMER_NAME"]);
            }

            foreach (DataRow row in documents.Rows)
            {
                AddUse(ToInt(row["GROUP_ID"]), row["PRODUCT"] + " " + row["CUSTOMER_NAME"] + " (" + row["DOCUMENT_NAME"] + ")");
            }

            // #0 is also read by other programs (uITLA ...), not only by the key tables of 1012
            if (_loaded.ContainsKey(GROUP_INTERNAL) || _usedBy.ContainsKey(GROUP_INTERNAL))
            {
                AddUse(GROUP_INTERNAL, "uITLA / โปรแกรมอื่น");
            }

            _groups.Rows.Clear();
            _rowOf.Clear();

            foreach (int id in _loaded.Keys.Union(_usedBy.Keys).OrderBy(id => id))
            {
                string used = UsedByText(id, USED_BY_MAX_CHARS);
                _rowOf[id] = _groups.Rows.Add(false, id, GroupText(id), "", "", used == "" ? "(ไม่มีลูกค้าใช้)" : used, "");
            }

            if (!_rowOf.ContainsKey(_selectedGroup)) _selectedGroup = -1;

            ApplyFilter();
            return "";
        }

        /// <summary>Run a database call under the loading sheet (the call must not touch a control).</summary>
        private T Busy<T>(Func<T> work)
        {
            return SettingBusy.Run(_busyOwner, SettingBusy.LOADING, work);
        }

        private void AddUse(int groupId, string name)
        {
            List<string> names;
            if (!_usedBy.TryGetValue(groupId, out names))
            {
                names = new List<string>();
                _usedBy[groupId] = names;
            }
            if (!names.Contains(name)) names.Add(name);
        }

        /// <summary>Who gets the group's mail on one line, "[21] 1480 HISILICON, 1480 OPLINK, ..." - cut at maxChars.</summary>
        private string UsedByText(int groupId, int maxChars)
        {
            List<string> names;
            if (!_usedBy.TryGetValue(groupId, out names) || names.Count == 0) return "";

            string text = "[" + names.Count + "] " + string.Join(", ", names.ToArray());
            return text.Length <= maxChars ? text : text.Substring(0, maxChars - 3) + "...";
        }

        /// <summary>The address as info_email spells it; null = not in the list (a new address).</summary>
        private string KnownAddress(string address)
        {
            return _addresses.FirstOrDefault(a => SameAddress(a, address));
        }

        // ---- members in memory ----

        /// <summary>Members of a group as edited (made empty the first time it is asked for).</summary>
        private Dictionary<string, int> NowOf(int groupId)
        {
            Dictionary<string, int> group;
            if (!_now.TryGetValue(groupId, out group))
            {
                group = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                _now[groupId] = group;
            }
            return group;
        }

        private int NowType(int groupId, string address)
        {
            int type;
            return address != "" && NowOf(groupId).TryGetValue(address, out type) ? type : 0;
        }

        private Member LoadedMember(int groupId, string address)
        {
            Dictionary<string, Member> group;
            Member member;
            return _loaded.TryGetValue(groupId, out group) && group.TryGetValue(address, out member) ? member : null;
        }

        private int LoadedType(int groupId, string address)
        {
            Member member = address == "" ? null : LoadedMember(groupId, address);
            return member == null ? 0 : member.Type;
        }

        private IEnumerable<string> LoadedAddresses(int groupId)
        {
            Dictionary<string, Member> group;
            return _loaded.TryGetValue(groupId, out group) ? group.Keys : Enumerable.Empty<string>();
        }

        private int CountNow(int groupId, int type)
        {
            return NowOf(groupId).Values.Count(t => t == type);
        }

        // ---- names ----

        /// <summary>Name on screen: typed here, else as read; "" = none.</summary>
        private string NameOf(int groupId)
        {
            string name;
            if (_namesStaged.TryGetValue(groupId, out name)) return name;
            return _names.TryGetValue(groupId, out name) ? name : "";
        }

        private string LoadedNameOf(int groupId)
        {
            string name;
            return _names.TryGetValue(groupId, out name) ? name : "";
        }

        /// <summary>"#12   QA Coherent" / "#1003   (ใหม่)".</summary>
        private string GroupText(int groupId)
        {
            string name = NameOf(groupId);
            return "#" + groupId + (name == "" ? "" : "   " + name) + (_newGroups.Contains(groupId) ? "   (ใหม่)" : "");
        }

        // =====================================================================
        // left side: the opened address
        // =====================================================================
        /// <summary>Show on the left where the address is; nothing staged is lost by opening another one.</summary>
        private void OpenAddress(string typed)
        {
            string address = (typed ?? "").Trim();
            if (address == "") return;

            if (!Regex.IsMatch(address, EMAIL_PATTERN))
            {
                frmError.ShowError(this, "รูปแบบอีเมลไม่ถูกต้อง");
                return;
            }

            _address = KnownAddress(address) ?? address;
            cbEmail.Value = _address;

            RefreshRows();
            UpdateSummary();
        }

        /// <summary>On how many groups an address is now (as edited).</summary>
        private int GroupCount(string address)
        {
            return _now.Count(p => p.Value.ContainsKey(address));
        }

        private string GroupCountText(string address)
        {
            int groups = GroupCount(address);
            return groups == 0 ? "ไม่อยู่ group ไหน" : groups + " group";
        }

        private void UpdateEmailInfo()
        {
            if (_address == "")
            {
                lbEmailInfo.Text = "";
                return;
            }

            int groups = GroupCount(_address);
            bool isNew = KnownAddress(_address) == null;

            lbEmailInfo.Text = (isNew ? "อีเมลใหม่ จะถูกเพิ่มตอน Save  ·  " : "") +
                               (groups == 0 ? "ยังไม่อยู่ใน group ไหน" : "อยู่ใน " + groups + " group");
            lbEmailInfo.ForeColor = isNew ? SettingUi.TAB_ACTIVE_BACK : SettingUi.TYPE_CC_TEXT;
        }

        // =====================================================================
        // new group / group name
        // =====================================================================
        /// <summary>
        /// A new, empty group, shown on the right to be filled. It is written (with its real id) by the Save that
        /// finds members on it; one of them must be TO.
        /// </summary>
        private void AddNewGroup()
        {
            if (!SettingUi.IsAdmin()) return;

            string name = "";
            if (_namesReadable)
            {
                name = AskText("Group ใหม่", "ชื่อ group ใหม่ (เว้นว่างได้) - เพิ่มคนทางขวา ต้องมี TO อย่างน้อย 1", "");
                if (name == null) return;
                name = CleanName(name);
            }
            else if (!frmConfirm.Ask(this, "สร้าง group ใหม่?" + Environment.NewLine + "เพิ่มคนทางขวา ต้องมี TO อย่างน้อย 1", "สร้าง", "ยกเลิก"))
            {
                return;
            }

            int max = Busy(() => _specCon.maxGroupId());
            if (_specCon.lastError != "")
            {
                frmError.ShowError(this, "อ่านเลข group ล่าสุดไม่ได้" + Environment.NewLine + _specCon.lastError);
                return;
            }

            int id = Math.Max(max, _rowOf.Keys.DefaultIfEmpty(-1).Max()) + 1;

            _newGroups.Add(id);
            if (name != "") _namesStaged[id] = name;
            _rowOf[id] = _groups.Rows.Add(false, id, GroupText(id), "", "", "(group ใหม่ - หลัง Save ไปเลือกให้ spec ที่หน้า Spec Setting)", "");

            txtFind.Text = "";
            _selectedGroup = id;
            RefreshRows();
            SelectGroupRow();
            UpdateSummary();
            cbAdd.FocusBox();
        }

        private void RenameGroup(int groupId)
        {
            if (!_namesReadable)
            {
                frmError.ShowError(this, "ฐานข้อมูล " + MailSafety.ConfigDatabase + " ยังไม่มีตาราง info_group_name ตั้งชื่อ group ไม่ได้");
                return;
            }

            string name = AskText("ชื่อ group", "ชื่อของ Group #" + groupId + " (แสดงในหน้า Setting เท่านั้น, เว้นว่าง = ไม่มีชื่อ)", NameOf(groupId));
            if (name == null) return;
            name = CleanName(name);

            // typed back to what the database has = nothing to save for it
            if (name == LoadedNameOf(groupId)) _namesStaged.Remove(groupId);
            else _namesStaged[groupId] = name;

            RefreshRows();
            UpdateSummary();
        }

        private static string CleanName(string name)
        {
            return Regex.Replace(name ?? "", @"\s+", " ").Trim();
        }

        // =====================================================================
        // left grid
        // =====================================================================
        /// <summary>Any click shows the group on the right; the tick / TYPE cells also edit the opened address.</summary>
        private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            DataRowView item = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item == null) return;

            int groupId = ToInt(item[COL_GROUP_ID]);
            string column = grid.Columns[e.ColumnIndex].Name;
            _selectedGroup = groupId;

            if (_address != "")
            {
                Dictionary<string, int> members = NowOf(groupId);

                if (column == COL_CHECK)
                {
                    if (members.ContainsKey(_address))
                    {
                        members.Remove(_address);
                    }
                    else
                    {
                        // ticked again: as it was read, else TO (click TYPE for CC)
                        int loaded = LoadedType(groupId, _address);
                        members[_address] = loaded != 0 ? loaded : RCP_TO;
                    }
                }
                else if (column == COL_TYPE && members.ContainsKey(_address))
                {
                    members[_address] = members[_address] == RCP_TO ? RCP_CC : RCP_TO;
                }
            }
            else if (column == COL_CHECK)
            {
                lbSummary.Text = "เลือกอีเมลในช่องด้านบนก่อน ถึงจะติ๊ก group ทางซ้ายได้  (หรือเพิ่มคนเข้า group นี้ทางขวา)" +
                                 Environment.NewLine + HINT;
                RefreshRows();
                return;
            }

            RefreshRows();
            UpdateSummary();
        }

        // ---- the tick box in the header: every group on screen (what the search shows) at once ----

        private List<int> ShownGroups()
        {
            return _view.Cast<DataRowView>().Select(v => ToInt(v[COL_GROUP_ID])).ToList();
        }

        /// <summary>The opened address ticked on none / some / all of the groups on screen.</summary>
        private CheckBoxState HeaderCheckState()
        {
            if (_address == "") return CheckBoxState.UncheckedDisabled;

            List<int> shown = ShownGroups();
            int ticked = shown.Count(id => NowType(id, _address) != 0);
            if (ticked == 0) return CheckBoxState.UncheckedNormal;
            return ticked == shown.Count ? CheckBoxState.CheckedNormal : CheckBoxState.MixedNormal;
        }

        private void grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex != -1 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != COL_CHECK) return;

            e.PaintBackground(e.CellBounds, false);

            // in line with the row boxes, which sit in the cell less its left padding
            int padLeft = grid.DefaultCellStyle.Padding.Left;
            Size box = CheckBoxRenderer.GetGlyphSize(e.Graphics, CheckBoxState.UncheckedNormal);
            Point at = new Point(e.CellBounds.Left + padLeft + (e.CellBounds.Width - padLeft - box.Width) / 2,
                                 e.CellBounds.Top + (e.CellBounds.Height - box.Height) / 2);
            CheckBoxRenderer.DrawCheckBox(e.Graphics, at, HeaderCheckState());

            e.Handled = true;
        }

        /// <summary>
        /// Header of the tick column: none or some ticked = tick every group on screen, all ticked = take every one off.
        /// Header of TYPE: the ticked groups on screen all TO = all CC, else all TO. Nothing is written yet.
        /// </summary>
        private void grid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            string column = grid.Columns[e.ColumnIndex].Name;
            if (column != COL_CHECK && column != COL_TYPE) return;

            if (_address == "")
            {
                lbSummary.Text = "เลือกอีเมลในช่องด้านบนก่อน ถึงจะติ๊ก group ได้" + Environment.NewLine + HINT;
                return;
            }

            if (column == COL_TYPE)
            {
                List<int> ticked = ShownGroups().Where(id => NowType(id, _address) != 0).ToList();
                if (ticked.Count == 0)
                {
                    lbSummary.Text = "ยังไม่ได้ติ๊ก group ไหน - ติ๊กก่อนแล้วค่อยสลับ TO / CC" + Environment.NewLine + HINT;
                    return;
                }

                int type = ticked.All(id => NowType(id, _address) == RCP_TO) ? RCP_CC : RCP_TO;
                foreach (int groupId in ticked) NowOf(groupId)[_address] = type;

                RefreshRows();
                UpdateSummary();
                return;
            }

            bool tickAll = HeaderCheckState() != CheckBoxState.CheckedNormal;

            foreach (int groupId in ShownGroups())
            {
                Dictionary<string, int> members = NowOf(groupId);

                if (!tickAll)
                {
                    members.Remove(_address);
                }
                else if (!members.ContainsKey(_address))
                {
                    // as it was read, else TO (click TYPE for CC)
                    int loaded = LoadedType(groupId, _address);
                    members[_address] = loaded != 0 ? loaded : RCP_TO;
                }
            }

            RefreshRows();
            UpdateSummary();
        }

        private void grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != COL_GROUP) return;

            DataRowView item = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item != null) RenameGroup(ToInt(item[COL_GROUP_ID]));
        }

        /// <summary>TO blue / CC grey like the member grid; adds blue, removals red.</summary>
        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string column = grid.Columns[e.ColumnIndex].Name;
            string value = Convert.ToString(e.Value);
            Color fore;

            if (column == COL_TYPE && value != "")
            {
                fore = value == "TO" ? SettingUi.TYPE_TO_TEXT : SettingUi.TYPE_CC_TEXT;
            }
            else if (column == COL_CHANGE && value != "")
            {
                fore = value.StartsWith("-") ? SettingUi.REMOVE_TEXT : SettingUi.TAB_ACTIVE_BACK;
            }
            else if (column == COL_USED_BY && value.StartsWith("("))
            {
                e.CellStyle.ForeColor = SettingUi.TYPE_CC_TEXT;
                e.CellStyle.SelectionForeColor = SettingUi.TYPE_CC_TEXT;
                return;
            }
            else
            {
                return;
            }

            e.CellStyle.Font = _bold;
            e.CellStyle.ForeColor = fore;
            e.CellStyle.SelectionForeColor = fore;
        }

        /// <summary>Hand on what can be clicked; the full customer / member list of a group on hover.</summary>
        private void grid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            DataRowView item = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item == null) return;

            string column = grid.Columns[e.ColumnIndex].Name;
            int groupId = ToInt(item[COL_GROUP_ID]);

            bool clickable = _address != "" && (column == COL_CHECK || (column == COL_TYPE && NowType(groupId, _address) != 0));
            grid.Cursor = clickable ? Cursors.Hand : Cursors.Default;

            DataGridViewCell cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (column == COL_USED_BY)
            {
                List<string> names;
                cell.ToolTipText = _usedBy.TryGetValue(groupId, out names)
                    ? "Group #" + groupId + " ส่งเมลของ:" + Environment.NewLine + FullList(names)
                    : "";
            }
            else if (column == COL_GROUP)
            {
                cell.ToolTipText = "คลิกเพื่อดูสมาชิกทางขวา  ·  ดับเบิลคลิกเพื่อตั้งชื่อ group";
            }
            else if (column == COL_CHECK || column == COL_TYPE)
            {
                cell.ToolTipText = _address == "" ? "เลือกอีเมลในช่องด้านบนก่อน" : _address;
            }
        }

        /// <summary>Put what is in memory on both grids.</summary>
        private void RefreshRows()
        {
            foreach (KeyValuePair<int, DataRow> pair in _rowOf)
            {
                int id = pair.Key;
                int mine = NowType(id, _address);
                int total = NowOf(id).Count;

                pair.Value[COL_CHECK] = mine != 0;
                pair.Value[COL_GROUP] = GroupText(id);
                pair.Value[COL_TYPE] = TypeName(mine);
                pair.Value[COL_MEMBERS] = total == 0 ? "ว่าง" : "TO " + CountNow(id, RCP_TO) + " / CC " + CountNow(id, RCP_CC);
                pair.Value[COL_CHANGE] = GroupChangeText(id);
            }

            grid.Invalidate();
            UpdateEmailInfo();
            RefreshMembers();
        }

        /// <summary>"+2  -1  สลับ 1  ชื่อใหม่" - what is staged on the group, from either side.</summary>
        private string GroupChangeText(int groupId)
        {
            List<Change> changes = ChangesOf(groupId);
            int added = changes.Count(c => c.Old == 0);
            int removed = changes.Count(c => c.New == 0);
            int switched = changes.Count - added - removed;

            List<string> parts = new List<string>();
            if (removed > 0) parts.Add("-" + removed);
            if (added > 0) parts.Add("+" + added);
            if (switched > 0) parts.Add("สลับ " + switched);
            if (!_newGroups.Contains(groupId) && _namesStaged.ContainsKey(groupId)) parts.Add("ชื่อใหม่");

            return string.Join("  ", parts.ToArray());
        }

        /// <summary>Keep the highlighted row of the left grid on the group shown on the right.</summary>
        private void SelectGroupRow()
        {
            grid.ClearSelection();
            foreach (DataGridViewRow row in grid.Rows)
            {
                DataRowView item = row.DataBoundItem as DataRowView;
                if (item == null || ToInt(item[COL_GROUP_ID]) != _selectedGroup) continue;

                row.Selected = true;
                if (!row.Displayed) grid.FirstDisplayedScrollingRowIndex = row.Index;
                return;
            }
        }

        private void ApplyFilter()
        {
            // the filter is a DataView expression: quotes doubled, the wildcard / bracket characters dropped
            string keyword = Regex.Replace(txtFind.Text.Trim(), @"[\[\]\*%]", "").Replace("'", "''");

            _view.RowFilter = keyword.Length == 0
                ? ""
                : "GROUP_TEXT LIKE '%" + keyword + "%' OR USED_BY LIKE '%" + keyword + "%'";

            lbCount.Text = _view.Count + " group" + (keyword.Length == 0 ? "" : " จาก " + _groups.Rows.Count);
            SelectGroupRow();
        }

        // =====================================================================
        // right side: members of the clicked group
        // =====================================================================
        /// <summary>Rebuild the member list of the group shown: TO, then CC, then the ones being removed.</summary>
        private void RefreshMembers()
        {
            bool has = _rowOf.ContainsKey(_selectedGroup);

            cbAdd.Enabled = has;
            cbAddType.Enabled = has;
            btnAdd.Enabled = has;
            btnAdd.BackColor = has ? SettingUi.BUTTON_BACK : SettingUi.BUTTON_DISABLED;

            _memberRows.Rows.Clear();

            if (!has)
            {
                lbMemberTitle.Text = "สมาชิกของ group";
                lbMemberNote.Text = "คลิก group ทางซ้ายเพื่อดูและแก้สมาชิก";
                return;
            }

            int id = _selectedGroup;
            Dictionary<string, int> now = NowOf(id);

            lbMemberTitle.Text = "Group " + GroupText(id);
            toolTip.SetToolTip(lbMemberTitle, lbMemberTitle.Text);

            string used = UsedByText(id, 200);
            lbMemberNote.Text = _newGroups.Contains(id)
                ? "group ใหม่ - เพิ่มคนด้านล่าง ต้องมี TO อย่างน้อย 1 แล้วกด Save"
                : now.Count + " คน  ·  " + (used == "" ? "ไม่มีลูกค้าใช้" : "ส่งเมลของ " + used);
            lbMemberNote.ForeColor = id == GROUP_INTERNAL ? SettingUi.ALERT_WARN_TEXT : SettingUi.TYPE_CC_TEXT;
            toolTip.SetToolTip(lbMemberNote, lbMemberNote.Text);

            foreach (string address in now.Keys.OrderBy(a => now[a]).ThenBy(a => a, StringComparer.OrdinalIgnoreCase))
            {
                int loaded = LoadedType(id, address);
                string change = loaded == 0 ? "+ เพิ่ม" : loaded != now[address] ? TypeName(loaded) + " → " + TypeName(now[address]) : "";
                _memberRows.Rows.Add(address, TypeName(now[address]), change, false);
            }

            foreach (string address in LoadedAddresses(id).Where(a => !now.ContainsKey(a)).OrderBy(a => a, StringComparer.OrdinalIgnoreCase))
            {
                _memberRows.Rows.Add(address, TypeName(LoadedType(id, address)), "- ลบ", true);
            }

            gridMembers.ClearSelection();
        }

        private void AddMember()
        {
            if (!_rowOf.ContainsKey(_selectedGroup)) return;

            string address = (cbAdd.Value ?? "").Trim();
            if (address == "") return;

            if (!Regex.IsMatch(address, EMAIL_PATTERN))
            {
                frmError.ShowError(this, "รูปแบบอีเมลไม่ถูกต้อง");
                return;
            }

            address = KnownAddress(address) ?? address;
            Dictionary<string, int> members = NowOf(_selectedGroup);

            if (members.ContainsKey(address))
            {
                frmError.ShowError(this, address + " อยู่ใน Group #" + _selectedGroup + " แล้ว");
                return;
            }

            members[address] = Convert.ToString(cbAddType.SelectedItem) == "CC" ? RCP_CC : RCP_TO;
            cbAdd.Value = "";

            RefreshRows();
            UpdateSummary();
            cbAdd.FocusBox();
        }

        /// <summary>TYPE switches TO / CC, ลบ takes the address out, คืน puts a removed one back; nothing is written yet.</summary>
        private void gridMembers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || !_rowOf.ContainsKey(_selectedGroup)) return;

            DataRowView item = gridMembers.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item == null) return;

            string column = gridMembers.Columns[e.ColumnIndex].Name;
            string address = Convert.ToString(item[COL_ADDRESS]);
            bool removed = (bool)item[COL_REMOVED];
            Dictionary<string, int> members = NowOf(_selectedGroup);

            if (column == COL_ACTION)
            {
                if (removed) members[address] = LoadedType(_selectedGroup, address);
                else members.Remove(address);
            }
            else if (column == COL_TYPE && !removed)
            {
                members[address] = members[address] == RCP_TO ? RCP_CC : RCP_TO;
            }
            else
            {
                return;
            }

            RefreshRows();
            UpdateSummary();
        }

        /// <summary>Double-click an address: open it on the left, to see / tick its other groups.</summary>
        private void gridMembers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || gridMembers.Columns[e.ColumnIndex].Name != COL_ADDRESS) return;

            DataRowView item = gridMembers.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item != null) OpenAddress(Convert.ToString(item[COL_ADDRESS]));
        }

        private void gridMembers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= gridMembers.Rows.Count) return;

            DataRowView item = gridMembers.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item == null) return;

            string column = gridMembers.Columns[e.ColumnIndex].Name;
            bool removed = (bool)item[COL_REMOVED];
            string change = Convert.ToString(item[COL_CHANGE]);

            if (column == COL_ACTION)
            {
                // caption per row: take out / put back
                e.Value = removed ? "คืน" : "ลบ";
                e.CellStyle.ForeColor = removed ? SettingUi.TYPE_TO_TEXT : SettingUi.REMOVE_TEXT;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                e.FormattingApplied = true;
                return;
            }

            Color fore;
            if (removed)
            {
                fore = column == COL_CHANGE ? SettingUi.REMOVE_TEXT : SettingUi.TYPE_CC_TEXT;
            }
            else if (column == COL_TYPE)
            {
                fore = Convert.ToString(e.Value) == "TO" ? SettingUi.TYPE_TO_TEXT : SettingUi.TYPE_CC_TEXT;
                e.CellStyle.Font = _bold;
            }
            else if (change != "" && (column == COL_CHANGE || column == COL_ADDRESS))
            {
                fore = SettingUi.TAB_ACTIVE_BACK;
            }
            else
            {
                return;
            }

            e.CellStyle.ForeColor = fore;
            e.CellStyle.SelectionForeColor = fore;
        }

        private void gridMembers_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string column = gridMembers.Columns[e.ColumnIndex].Name;
            gridMembers.Cursor = column == COL_ACTION || column == COL_TYPE ? Cursors.Hand : Cursors.Default;

            if (column == COL_ADDRESS)
            {
                gridMembers.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = "ดับเบิลคลิกเพื่อดู group อื่นของคนนี้ทางซ้าย";
            }
        }

        // =====================================================================
        // changes / Save
        // =====================================================================
        /// <summary>Adds / removes / TO-CC switches staged on one group, by address.</summary>
        private List<Change> ChangesOf(int groupId)
        {
            List<Change> changes = new List<Change>();
            Dictionary<string, int> now = NowOf(groupId);

            foreach (string address in LoadedAddresses(groupId).Union(now.Keys, StringComparer.OrdinalIgnoreCase)
                         .OrderBy(a => a, StringComparer.OrdinalIgnoreCase))
            {
                Member loaded = LoadedMember(groupId, address);
                int before = loaded == null ? 0 : loaded.Type;
                int after = NowType(groupId, address);
                if (before == after) continue;

                changes.Add(new Change
                {
                    GroupId = groupId,
                    Address = address,
                    EmailId = loaded == null ? -1 : loaded.EmailId,
                    Old = before,
                    New = after
                });
            }

            return changes;
        }

        /// <summary>Everything Save would write to info_group_email, by group.</summary>
        private List<Change> Changes()
        {
            return _loaded.Keys.Union(_now.Keys).OrderBy(id => id).SelectMany(id => ChangesOf(id)).ToList();
        }

        /// <summary>Names typed here that differ from the database; a new group counts only once somebody is on it.</summary>
        private List<NameChange> NameChanges()
        {
            return _namesStaged
                .Where(p => p.Value != LoadedNameOf(p.Key))
                .Where(p => !_newGroups.Contains(p.Key) || NowOf(p.Key).Count > 0)
                .OrderBy(p => p.Key)
                .Select(p => new NameChange { GroupId = p.Key, Old = LoadedNameOf(p.Key), New = p.Value })
                .ToList();
        }

        private int PendingCount()
        {
            return Changes().Count + NameChanges().Count;
        }

        private void UpdateSummary()
        {
            List<Change> changes = Changes();
            int names = NameChanges().Count;
            int total = changes.Count + names;

            if (total == 0)
            {
                lbSummary.Text = HINT;
            }
            else
            {
                int added = changes.Count(c => c.Old == 0);
                int removed = changes.Count(c => c.New == 0);
                int switched = changes.Count - added - removed;
                int groups = changes.Select(c => c.GroupId).Distinct().Count();

                lbSummary.Text = "จะบันทึก " + total + " รายการ ใน " + groups + " group: เพิ่ม " + added + ", ลบ " + removed +
                                 ", สลับ TO/CC " + switched + (names > 0 ? ", ชื่อ group " + names : "") +
                                 Environment.NewLine + HINT;
            }

            btnSave.Text = total == 0 ? "Save" : "Save (" + total + ")";
        }

        private void SaveChanges()
        {
            List<Change> changes = Changes();
            List<NameChange> names = NameChanges();

            if (changes.Count + names.Count == 0)
            {
                lbSummary.Text = (_newGroups.Count > 0
                    ? "group ใหม่ยังไม่มีคน: คลิก group ใหม่ แล้วเพิ่มคนทางขวา (ต้องมี TO)"
                    : "ไม่มีอะไรเปลี่ยน") + Environment.NewLine + HINT;
                return;
            }

            List<int> noTo = GroupsLeftWithoutTo(changes);
            if (noTo.Count > 0)
            {
                frmError.ShowError(this, SAVE_STOPPED + Environment.NewLine +
                    string.Join(", ", noTo.Select(id => "Group #" + id).ToArray()) + " จะไม่มีผู้รับ TO" + Environment.NewLine +
                    "group ต้องมี TO อย่างน้อย 1 คน: เพิ่ม TO หรือเปลี่ยนคนใน group เป็น TO ก่อน");
                return;
            }

            if (!frmConfirm.Ask(this, SaveQuestion(changes, names), "บันทึก", "ยกเลิก")) return;

            // one sheet for the write and the reload after it
            using (SettingBusy.Begin(this, SettingBusy.SAVING))
            {
                WriteAndReload(changes, names);
            }
        }

        private void WriteAndReload(List<Change> changes, List<NameChange> names)
        {
            List<string> batch = SaveStatements(changes, names, SettingUi.CurrentUserCode());
            if (batch == null) return;

            if (!SettingBusy.Run(this, SettingBusy.SAVING, () => _emailCon.saveChanges(batch)))
            {
                frmError.ShowError(this, SAVE_STOPPED + Environment.NewLine + _emailCon.lastError);
                return;
            }

            int saved = changes.Count + names.Count;
            _savedCount += saved;

            // read everything again, so the window shows what the database now holds;
            // the group on the right stays (a new one under the id it was written with)
            _selectedGroup = MapGroup(_writtenGroupIds, _selectedGroup);

            string error = LoadAll();
            if (error != "")
            {
                frmError.ShowError(this, "บันทึกแล้ว แต่อ่านข้อมูลใหม่ไม่ได้ ปิดหน้าต่างแล้วเปิดใหม่" + Environment.NewLine + error);
                Close();
                return;
            }

            if (_address != "") _address = KnownAddress(_address) ?? _address;

            RefreshRows();
            SelectGroupRow();
            UpdateSummary();
            lbSummary.Text = "บันทึกลง " + MailSafety.ConfigDatabase + " แล้ว " + saved + " รายการ" + Environment.NewLine + HINT;
        }

        /// <summary>
        /// Groups whose mail would go to nobody after the changes: a group made here with nobody on TO, and a group
        /// customers use that has a TO now and would have none.
        /// </summary>
        private List<int> GroupsLeftWithoutTo(List<Change> changes)
        {
            List<int> noTo = new List<int>();
            foreach (int groupId in changes.Select(c => c.GroupId).Distinct().OrderBy(id => id))
            {
                if (CountNow(groupId, RCP_TO) > 0) continue;

                bool hadTo = LoadedAddresses(groupId).Any(a => LoadedType(groupId, a) == RCP_TO);
                if (_newGroups.Contains(groupId) || (hadTo && _usedBy.ContainsKey(groupId))) noTo.Add(groupId);
            }
            return noTo;
        }

        /// <summary>
        /// Statements of one Save in the order they run (one transaction); null = a read failed (shown).
        /// A group made here gets its id now (the next free one), not the one shown while staging; a new address
        /// gets its info_email row first.
        /// </summary>
        private List<string> SaveStatements(List<Change> changes, List<NameChange> names, string user)
        {
            List<string> batch = new List<string>();
            Dictionary<int, int> groupIds = new Dictionary<int, int>();

            List<int> newGroups = changes.Select(c => c.GroupId).Where(id => _newGroups.Contains(id)).Distinct().OrderBy(id => id).ToList();
            if (newGroups.Count > 0)
            {
                int max = Busy(() => _specCon.maxGroupId());
                if (_specCon.lastError != "")
                {
                    frmError.ShowError(this, SAVE_STOPPED + Environment.NewLine + _specCon.lastError);
                    return null;
                }

                foreach (int staged in newGroups)
                {
                    max++;
                    groupIds[staged] = max;
                }
            }

            // one connection for the whole Save: the audit triggers pick up who is saving from @app_user
            batch.Add(_emailSql.setAppUser(user));

            // EMAIL_ID of every address that joins a group: the one it has, else a new info_email row
            Dictionary<string, int> emailIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int lastEmailId = int.MinValue;   // read once, when the first new address needs an id

            foreach (string address in changes.Where(c => c.Old == 0).Select(c => c.Address).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                int id;
                if (!_emailIdOf.TryGetValue(address, out id))
                {
                    if (lastEmailId == int.MinValue)
                    {
                        lastEmailId = Busy(() => _emailCon.maxEmailId());
                        if (_emailCon.lastError != "")
                        {
                            frmError.ShowError(this, SAVE_STOPPED + Environment.NewLine + _emailCon.lastError);
                            return null;
                        }
                    }

                    lastEmailId++;
                    id = lastEmailId;
                    batch.Add(_emailSql.insertEmail(new SettingEmailProperty { emailId = id, emailAddress = address, createUser = user }));
                }
                emailIds[address] = id;
            }

            // removals first, so an address taken out and another put in never meet on the unique key
            foreach (Change change in changes.Where(c => c.New == 0))
            {
                batch.Add(_emailSql.deleteGroupMember(Member2(MapGroup(groupIds, change.GroupId), change.EmailId, 0)));
            }

            foreach (Change change in changes.Where(c => c.Old != 0 && c.New != 0))
            {
                batch.Add(_emailSql.updateRecipientType(Member2(MapGroup(groupIds, change.GroupId), change.EmailId, change.New)));
            }

            foreach (Change change in changes.Where(c => c.Old == 0))
            {
                batch.Add(_emailSql.insertGroupMember(Member2(MapGroup(groupIds, change.GroupId), emailIds[change.Address], change.New)));
            }

            foreach (NameChange name in names)
            {
                // a new group without a name has no row to write
                if (name.New == "" && _newGroups.Contains(name.GroupId)) continue;
                batch.Add(_specSql.saveGroupName(MapGroup(groupIds, name.GroupId), name.New, user));
            }

            // log: one row per new group, per address per group (CUSTOMER = who gets the group's mail), per name
            foreach (int staged in newGroups)
            {
                batch.Add(_specSql.logNewGroup(LOG_PRODUCT, groupIds[staged], "", user));
            }

            foreach (Change change in changes)
            {
                batch.Add(_specSql.logRecipientChange(LOG_PRODUCT, MapGroup(groupIds, change.GroupId),
                    UsedByText(change.GroupId, 100), change.Address,
                    change.Old == 0 ? (int?)null : change.Old, change.New == 0 ? (int?)null : change.New, user));
            }

            foreach (NameChange name in names)
            {
                if (name.New == "" && _newGroups.Contains(name.GroupId)) continue;
                batch.Add(_specSql.logGroupNameChange(LOG_PRODUCT, MapGroup(groupIds, name.GroupId), name.Old, name.New, user));
            }

            batch.Add(_emailSql.clearAppUser());

            _writtenGroupIds = groupIds;
            return batch;
        }

        /// <summary>What Save writes, group by group, and whose mail it touches - before anything is written.</summary>
        private string SaveQuestion(List<Change> changes, List<NameChange> names)
        {
            List<string> lines = new List<string>();
            lines.Add("บันทึกลง " + MailSafety.ConfigDatabase + "?");

            List<int> groups = changes.Select(c => c.GroupId).Distinct().OrderBy(id => id).ToList();
            foreach (int groupId in groups.Take(CONFIRM_MAX_LINES))
            {
                List<Change> mine = changes.Where(c => c.GroupId == groupId).ToList();
                List<string> parts = new List<string>();

                List<Change> added = mine.Where(c => c.Old == 0).ToList();
                List<Change> removed = mine.Where(c => c.New == 0).ToList();
                List<Change> switched = mine.Where(c => c.Old != 0 && c.New != 0).ToList();

                if (added.Count > 0) parts.Add("เพิ่ม " + People(added.Select(c => ShortName(c.Address) + " (" + TypeName(c.New) + ")")));
                if (removed.Count > 0) parts.Add("ลบ " + People(removed.Select(c => ShortName(c.Address))));
                if (switched.Count > 0) parts.Add("สลับ " + People(switched.Select(c => ShortName(c.Address) + " → " + TypeName(c.New))));

                string used = _newGroups.Contains(groupId) ? "group ใหม่" : UsedByCount(groupId);
                lines.Add("");
                lines.Add(GroupText(groupId) + "  [" + used + "]");
                lines.Add("  " + string.Join("  ·  ", parts.ToArray()));
            }
            if (groups.Count > CONFIRM_MAX_LINES) lines.Add("... และอีก " + (groups.Count - CONFIRM_MAX_LINES) + " group");

            List<string> newAddresses = changes.Where(c => c.Old == 0 && !_emailIdOf.ContainsKey(c.Address))
                .Select(c => c.Address).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (newAddresses.Count > 0)
            {
                lines.Add("");
                lines.Add("อีเมลใหม่ในระบบ: " + People(newAddresses));
            }

            List<NameChange> renamed = names.Where(n => !_newGroups.Contains(n.GroupId)).ToList();
            if (renamed.Count > 0)
            {
                lines.Add("");
                lines.Add("ชื่อ group: " + string.Join(", ", renamed.Take(CONFIRM_MAX_LINES)
                    .Select(n => "#" + n.GroupId + " = " + (n.New == "" ? "(ไม่มีชื่อ)" : n.New)).ToArray()));
            }

            if (groups.Contains(GROUP_INTERNAL))
            {
                lines.Add("");
                lines.Add("#0 ใช้ร่วมกับลูกค้าอื่นและโปรแกรมอื่น (เช่น uITLA)");
            }

            return string.Join(Environment.NewLine, lines.ToArray());
        }

        /// <summary>"a, b, c และอีก 4" - a few names, the rest counted.</summary>
        private static string People(IEnumerable<string> names)
        {
            List<string> all = names.ToList();
            return string.Join(", ", all.Take(4).ToArray()) + (all.Count > 4 ? " และอีก " + (all.Count - 4) : "");
        }

        private string UsedByCount(int groupId)
        {
            List<string> names;
            return _usedBy.TryGetValue(groupId, out names) && names.Count > 0 ? "ใช้กับ " + names.Count + " ลูกค้า" : "ไม่มีลูกค้าใช้";
        }

        // =====================================================================
        // helpers
        // =====================================================================
        private static SettingEmailProperty Member2(int groupId, int emailId, int recipientId)
        {
            return new SettingEmailProperty { groupId = groupId, emailId = emailId, recipientId = recipientId };
        }

        private static int MapGroup(Dictionary<int, int> groupIds, int groupId)
        {
            int written;
            return groupIds.TryGetValue(groupId, out written) ? written : groupId;
        }

        private static string TypeName(int type)
        {
            return type == RCP_TO ? "TO" : type == RCP_CC ? "CC" : "";
        }

        private static bool SameAddress(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string ShortName(string address)
        {
            int at = address.IndexOf('@');
            return at > 0 ? address.Substring(0, at) : address;
        }

        /// <summary>One name per line, capped so the tooltip stays on screen.</summary>
        private static string FullList(List<string> names)
        {
            string text = string.Join(Environment.NewLine, names.Take(30).Select(n => "  - " + n).ToArray());
            if (names.Count > 30) text += Environment.NewLine + "  ... และอีก " + (names.Count - 30);
            return text;
        }

        private static int ToInt(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return Convert.ToInt32(value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bold.Dispose();
                toolTip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
