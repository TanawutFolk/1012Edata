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

namespace EdataManagement.Views.Setting
{
    /// <summary>
    /// "Manage by Email" window - the only place recipients are edited: one address and every group, ticked where it
    /// is on. Tick / untick / TO-CC on as many groups as needed and Save once.
    /// Also here: the name of a group (double-click GROUP; info_group_name, shown in Setting only) and, for ADMIN,
    /// "+ Group ใหม่" - a new group; it waits as a row here and is written with its first TO address (the database has
    /// no group table, a group exists through its members). Specs are moved to it on the Spec page.
    /// Writes insert / delete / TO-CC of info_group_email, a new info_email row, info_group_name, and one
    /// log_setting_edata row per change (PRODUCT = BY EMAIL, CUSTOMER = who gets the group's mail), in one transaction.
    /// Opened by ucSpecSetting only while that page has nothing staged; Open returns how many changes were saved.
    /// </summary>
    internal class EmailGroupsDialog : Form
    {
        private const int RCP_TO = 1;
        private const int RCP_CC = 2;
        private const int GROUP_INTERNAL = 0;
        private const int GROUP_NAME_MAX = 100;
        private const int CONFIRM_MAX_LINES = 15;
        private const int USED_BY_MAX_CHARS = 90;

        private const string LOG_PRODUCT = "BY EMAIL";
        private const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        private const string SAVE_STOPPED = "บันทึกไม่สำเร็จ ยังไม่มีข้อมูลถูกเขียน";
        private const string HINT = "ติ๊ก group ที่ต้องการ แล้วกด Save  ·  คลิก TYPE เพื่อสลับ TO / CC  ·  ดับเบิลคลิก GROUP เพื่อตั้งชื่อ";
        private const string NEW_GROUP_USE = "(group ใหม่ - ยังไม่บันทึกจนกว่าจะมี TO: เปิดอีเมล ติ๊ก group นี้ แล้ว Save)";

        // columns of _groups (and of the grid)
        private const string COL_CHECK = "CHECKED";
        private const string COL_GROUP_ID = "GROUP_ID";
        private const string COL_GROUP = "GROUP_TEXT";
        private const string COL_TYPE = "TYPE";
        private const string COL_MEMBERS = "MEMBERS";
        private const string COL_USED_BY = "USED_BY";
        private const string COL_CHANGE = "CHANGE";

        /// <summary>One add / remove / TO-CC switch of the opened address on one group. 0 = not on the group.</summary>
        private class Change
        {
            public int GroupId;     // a group made here: the id shown while staging, mapped at Save
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

        /// <summary>One row per group, bound to the grid; _rowOf finds it by GROUP_ID.</summary>
        private readonly DataTable _groups = new DataTable();
        private readonly DataView _view;
        private readonly Dictionary<int, DataRow> _rowOf = new Dictionary<int, DataRow>();

        /// <summary>TO on each group as read - Save refuses to leave a group that customers use without one.</summary>
        private readonly Dictionary<int, int> _toCount = new Dictionary<int, int>();

        /// <summary>Who gets each group's mail: "1480 OPLINK", "1480 HISILICON (Serial List)" ...</summary>
        private readonly Dictionary<int, List<string>> _usedBy = new Dictionary<int, List<string>>();

        /// <summary>Who is on each group as read, one address per line under "TO" / "CC" - the hover text of MEMBERS.</summary>
        private readonly Dictionary<int, string> _memberList = new Dictionary<int, string>();

        /// <summary>Addresses of info_email as spelled there.</summary>
        private readonly List<string> _addresses = new List<string>();

        /// <summary>Group names as read / typed here and not saved yet ("" = no name). False = no info_group_name table.</summary>
        private readonly Dictionary<int, string> _names = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _namesStaged = new Dictionary<int, string>();
        private bool _namesReadable;

        /// <summary>Groups made here, not in the database yet.</summary>
        private readonly List<int> _newGroups = new List<int>();

        // the opened address: its groups as read (group -> TO / CC, group -> EMAIL_ID there) and as ticked now
        private string _address = "";
        private readonly Dictionary<int, int> _before = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _beforeEmailId = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _now = new Dictionary<int, int>();

        private int _savedCount;

        /// <summary>What the loading sheet covers: the opening page first, this window once it is shown.</summary>
        private Control _busyOwner;

        private readonly ComboBox cbEmail = new ComboBox();
        private readonly Label lbEmailInfo = new Label();
        private readonly TextBox txtFind = new TextBox();
        private readonly Label lbCount = new Label();
        private readonly DataGridView grid = new DataGridView();
        private readonly Label lbSummary = new Label();
        private readonly Button btnSave = new Button();
        private readonly ToolTip toolTip = new ToolTip();
        private readonly Font _bold = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Bold);

        /// <summary>
        /// Opens the window (address = the one to open at once, "" = the user picks);
        /// the number of changes written while it was open (0 = nothing saved).
        /// </summary>
        public static int Open(Control owner, List<SettingEmailProperty> products, string address)
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

                if (!string.IsNullOrEmpty(address))
                {
                    dialog.cbEmail.Text = address;
                    dialog.OpenAddress(address);
                }

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

            BuildUi();
        }

        // =====================================================================
        // layout
        // =====================================================================
        private void BuildUi()
        {
            Text = "Manage by Email - one address on every group (" + MailSafety.ConfigDatabase + ")";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1180, 660);
            MinimumSize = new Size(900, 480);
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Regular);

            // ---- top: the address, search ----
            Panel top = new Panel();
            top.Dock = DockStyle.Top;
            top.Size = new Size(ClientSize.Width, 92);   // full width before + Group ใหม่ is anchored to its right edge
            top.BackColor = Color.White;

            const int labelLeft = 16;
            const int boxLeft = 80;
            const int boxWidth = 360;
            const int buttonLeft = boxLeft + boxWidth + 8;

            top.Controls.Add(MakeLabel("อีเมล", labelLeft, 18));

            // type to find: addresses that start with what is typed (like the customer box of the Spec page)
            cbEmail.DropDownStyle = ComboBoxStyle.DropDown;
            cbEmail.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cbEmail.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbEmail.Location = new Point(boxLeft, 14);
            cbEmail.Size = new Size(boxWidth, 26);
            cbEmail.MaxDropDownItems = 20;
            // SelectionChangeCommitted comes before Text follows the picked item
            cbEmail.SelectionChangeCommitted += delegate { OpenAddress(Convert.ToString(cbEmail.SelectedItem)); };
            cbEmail.KeyDown += delegate (object s, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                OpenAddress(cbEmail.Text);
            };
            top.Controls.Add(cbEmail);

            Button btnOpen = MakeButton("เปิด", buttonLeft, 13, 90);
            btnOpen.Click += delegate { OpenAddress(cbEmail.Text); };
            top.Controls.Add(btnOpen);

            lbEmailInfo.AutoSize = true;
            lbEmailInfo.Location = new Point(buttonLeft + 100, 18);
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
                Button btnNewGroup = MakeButton("+ Group ใหม่", ClientSize.Width - 16 - 130, 53, 130);
                btnNewGroup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnNewGroup.Click += delegate { AddNewGroup(); };
                top.Controls.Add(btnNewGroup);
                toolTip.SetToolTip(btnNewGroup, "สร้าง group ใหม่ที่มีอีเมลที่เปิดอยู่เป็น TO (ADMIN)");
            }

            // ---- grid ----
            grid.Dock = DockStyle.Fill;
            SettingUi.StyleGrid(grid, 32);
            grid.AutoGenerateColumns = false;
            grid.Columns.Add(MakeCheckColumn());
            grid.Columns.Add(MakeTextColumn(COL_GROUP, "GROUP", 150));
            grid.Columns.Add(MakeTextColumn(COL_TYPE, "TYPE", 55));
            grid.Columns.Add(MakeTextColumn(COL_MEMBERS, "MEMBERS", 85));
            grid.Columns.Add(MakeTextColumn(COL_USED_BY, "USED BY (ใครได้เมลของ group นี้)", 380));
            grid.Columns.Add(MakeTextColumn(COL_CHANGE, "CHANGE", 150));
            grid.Columns[COL_TYPE].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.DataSource = _view;

            grid.CellClick += grid_CellClick;
            grid.CellDoubleClick += grid_CellDoubleClick;
            grid.CellFormatting += grid_CellFormatting;
            grid.CellMouseEnter += grid_CellMouseEnter;
            grid.CellMouseLeave += delegate { grid.Cursor = Cursors.Default; };
            grid.DataBindingComplete += delegate { grid.ClearSelection(); };

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

            Controls.Add(grid);
            Controls.Add(top);
            Controls.Add(bottom);

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
                // a new group nobody is ticked on is not written either: it counts as unsaved
                int n = PendingCount() + _newGroups.Count(id => !_now.ContainsKey(id));
                if (n > 0 && !frmConfirm.Ask(this, "ทิ้งการแก้ที่ยังไม่ได้ Save (" + n + " รายการ)?", "ทิ้ง", "แก้ต่อ"))
                {
                    e.Cancel = true;
                }
            };

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
        /// <summary>Addresses, groups, names, members and who uses each group; "" = done, else the error.</summary>
        private string LoadAll()
        {
            DataTable emails, counts, specs, documents, names;

            using (SettingBusy.Begin(_busyOwner, SettingBusy.LOADING))
            {
                emails = Busy(() => _emailCon.searchEmailList());
                if (_emailCon.lastError != "") return _emailCon.lastError;

                counts = Busy(() => _emailCon.searchAllGroups());
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
            _addresses.AddRange(emails.Rows.Cast<DataRow>()
                .Select(r => Convert.ToString(r["EMAIL_ADDRESS"]).Trim())
                .Where(a => a != "")
                .Distinct(StringComparer.OrdinalIgnoreCase));

            string typed = cbEmail.Text;
            cbEmail.Items.Clear();
            cbEmail.Items.AddRange(_addresses.Cast<object>().ToArray());
            cbEmail.Text = typed;

            _toCount.Clear();
            _usedBy.Clear();
            _newGroups.Clear();
            _memberList.Clear();
            Dictionary<int, string> members = new Dictionary<int, string>();

            foreach (DataRow row in counts.Rows)
            {
                int id = ToInt(row["GROUP_ID"]);
                int to = ToInt(row["TO_COUNT"]);
                _toCount[id] = to;
                members[id] = "TO " + to + " / CC " + ToInt(row["CC_COUNT"]);
                _memberList[id] = "Group #" + id + Environment.NewLine +
                    "TO:" + Environment.NewLine + AddressLines(row["TO_LIST"]) + Environment.NewLine +
                    "CC:" + Environment.NewLine + AddressLines(row["CC_LIST"]);
            }

            foreach (DataRow row in specs.Rows)
            {
                AddUse(ToInt(row["GROUP_ID"]), row["PRODUCT"] + " " + row["CUSTOMER_NAME"]);
            }

            foreach (DataRow row in documents.Rows)
            {
                AddUse(ToInt(row["GROUP_ID"]), row["PRODUCT"] + " " + row["CUSTOMER_NAME"] + " (" + row["DOCUMENT_NAME"] + ")");
            }

            // #0 is also read by other programs (uITLA ...), not only by the key tables of 1012
            if (members.ContainsKey(GROUP_INTERNAL) || _usedBy.ContainsKey(GROUP_INTERNAL))
            {
                AddUse(GROUP_INTERNAL, "uITLA / โปรแกรมอื่น");
            }

            _groups.Rows.Clear();
            _rowOf.Clear();

            foreach (int id in members.Keys.Union(_usedBy.Keys).OrderBy(id => id))
            {
                string used = UsedByText(id, USED_BY_MAX_CHARS);

                string membersText;
                if (!members.TryGetValue(id, out membersText)) membersText = "ว่าง";

                _rowOf[id] = _groups.Rows.Add(false, id, GroupText(id), "", membersText, used == "" ? "(ไม่มีลูกค้าใช้)" : used, "");
            }

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
        // the opened address
        // =====================================================================
        private void OpenAddress(string typed)
        {
            string address = (typed ?? "").Trim();
            if (address == "" || SameAddress(address, _address)) return;

            if (!Regex.IsMatch(address, EMAIL_PATTERN))
            {
                frmError.ShowError(this, "รูปแบบอีเมลไม่ถูกต้อง");
                return;
            }

            // names belong to the groups, not to the address: they stay staged
            int pending = Changes().Count;
            if (pending > 0 && !frmConfirm.Ask(this,
                    "ทิ้งการแก้ของ " + _address + " (" + pending + " รายการ) แล้วเปิด " + address + "?", "ทิ้ง", "ยกเลิก"))
            {
                cbEmail.Text = _address;
                return;
            }

            DataTable rows = Busy(() => _emailCon.searchGroupsOfAddress(new SettingEmailProperty { emailAddress = address }));
            if (_emailCon.lastError != "")
            {
                frmError.ShowError(this, "อ่าน group ของ " + address + " ไม่ได้" + Environment.NewLine + _emailCon.lastError);
                return;
            }

            _address = KnownAddress(address) ?? address;
            cbEmail.Text = _address;

            // matched by the address: each row brings the EMAIL_ID that is really on that group
            _before.Clear();
            _beforeEmailId.Clear();
            _now.Clear();
            foreach (DataRow row in rows.Rows)
            {
                int groupId = ToInt(row["GROUP_ID"]);
                _before[groupId] = ToInt(row["RECIPIENT_ID"]);
                _beforeEmailId[groupId] = ToInt(row["EMAIL_ID"]);
                _now[groupId] = _before[groupId];
            }

            if (KnownAddress(_address) == null)
            {
                lbEmailInfo.Text = "อีเมลใหม่ ยังไม่มีในระบบ จะถูกเพิ่มตอน Save";
                lbEmailInfo.ForeColor = SettingUi.TAB_ACTIVE_BACK;
            }
            else
            {
                lbEmailInfo.Text = _before.Count == 0 ? "ยังไม่อยู่ใน group ไหน" : "อยู่ใน " + _before.Count + " group";
                lbEmailInfo.ForeColor = SettingUi.TYPE_CC_TEXT;
            }

            RefreshRows();
            UpdateSummary();
        }

        // =====================================================================
        // new group / group name
        // =====================================================================
        /// <summary>
        /// A new row to tick addresses on: the opened address starts on it as TO, with none open it waits empty.
        /// It is written (with its real id) by the Save that puts its first TO on it.
        /// </summary>
        private void AddNewGroup()
        {
            if (!SettingUi.IsAdmin()) return;

            string first = _address == "" ? "เปิดอีเมลแล้วติ๊ก group นี้เป็น TO เพื่อบันทึก" : _address + " จะเป็น TO";

            string name = "";
            if (_namesReadable)
            {
                name = AskText("Group ใหม่", "ชื่อ group ใหม่ (เว้นว่างได้) - " + first, "");
                if (name == null) return;
                name = CleanName(name);
            }
            else if (!frmConfirm.Ask(this, "สร้าง group ใหม่?" + Environment.NewLine + first, "สร้าง", "ยกเลิก"))
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
            if (_address != "") _now[id] = RCP_TO;
            _rowOf[id] = _groups.Rows.Add(false, id, GroupText(id), "", "ใหม่", NEW_GROUP_USE, "");

            txtFind.Text = "";
            RefreshRows();
            UpdateSummary();
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
        // grid
        // =====================================================================
        private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _address == "") return;

            DataRowView item = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item == null) return;

            int groupId = ToInt(item[COL_GROUP_ID]);
            string column = grid.Columns[e.ColumnIndex].Name;

            if (column == COL_CHECK)
            {
                if (_now.ContainsKey(groupId))
                {
                    _now.Remove(groupId);
                }
                else
                {
                    // ticked again: as it was read, else TO (click TYPE for CC)
                    int type;
                    _now[groupId] = _before.TryGetValue(groupId, out type) ? type : RCP_TO;
                }
            }
            else if (column == COL_TYPE && _now.ContainsKey(groupId))
            {
                _now[groupId] = _now[groupId] == RCP_TO ? RCP_CC : RCP_TO;
            }
            else
            {
                return;
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
                fore = value.StartsWith("- ") ? SettingUi.REMOVE_TEXT : SettingUi.TAB_ACTIVE_BACK;
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

        /// <summary>Hand on what can be clicked; the full customer list of a group on hover.</summary>
        private void grid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            DataRowView item = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item == null) return;

            string column = grid.Columns[e.ColumnIndex].Name;
            int groupId = ToInt(item[COL_GROUP_ID]);

            bool clickable = _address != "" && (column == COL_CHECK || (column == COL_TYPE && _now.ContainsKey(groupId)));
            grid.Cursor = clickable ? Cursors.Hand : Cursors.Default;

            if (column == COL_USED_BY)
            {
                List<string> names;
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = _usedBy.TryGetValue(groupId, out names)
                    ? "Group #" + groupId + " ส่งเมลของ:" + Environment.NewLine + FullList(names)
                    : "";
            }
            else if (column == COL_MEMBERS)
            {
                string list;
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = _memberList.TryGetValue(groupId, out list) ? list : "";
            }
            else if (column == COL_GROUP)
            {
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = "ดับเบิลคลิกเพื่อตั้งชื่อ group";
            }
        }

        private void RefreshRows()
        {
            foreach (KeyValuePair<int, DataRow> pair in _rowOf)
            {
                int now = Get(_now, pair.Key);
                pair.Value[COL_CHECK] = now != 0;
                pair.Value[COL_GROUP] = GroupText(pair.Key);
                pair.Value[COL_TYPE] = TypeName(now);
                pair.Value[COL_CHANGE] = ChangeText(pair.Key);
            }
            grid.Invalidate();
        }

        /// <summary>"+ เพิ่ม TO" / "- ลบ" / "TO → CC", and "ชื่อใหม่" when the name was typed here.</summary>
        private string ChangeText(int groupId)
        {
            int before = Get(_before, groupId);
            int now = Get(_now, groupId);

            string text = "";
            if (before == 0 && now != 0) text = "+ เพิ่ม " + TypeName(now);
            else if (before != 0 && now == 0) text = "- ลบ";
            else if (before != now) text = TypeName(before) + " → " + TypeName(now);

            if (!_newGroups.Contains(groupId) && _namesStaged.ContainsKey(groupId))
            {
                text = (text == "" ? "" : text + ", ") + "ชื่อใหม่";
            }
            return text;
        }

        private void ApplyFilter()
        {
            // the filter is a DataView expression: quotes doubled, the wildcard / bracket characters dropped
            string keyword = Regex.Replace(txtFind.Text.Trim(), @"[\[\]\*%]", "").Replace("'", "''");

            _view.RowFilter = keyword.Length == 0
                ? ""
                : "GROUP_TEXT LIKE '%" + keyword + "%' OR USED_BY LIKE '%" + keyword + "%'";

            lbCount.Text = _view.Count + " group" + (keyword.Length == 0 ? "" : " จาก " + _groups.Rows.Count);
        }

        // =====================================================================
        // changes / Save
        // =====================================================================
        /// <summary>Every add / remove / TO-CC switch of the opened address Save would write, by group.</summary>
        private List<Change> Changes()
        {
            List<Change> changes = new List<Change>();
            if (_address == "") return changes;

            foreach (int groupId in _before.Keys.Union(_now.Keys).OrderBy(id => id))
            {
                int before = Get(_before, groupId);
                int now = Get(_now, groupId);
                if (before == now) continue;

                changes.Add(new Change
                {
                    GroupId = groupId,
                    EmailId = before == 0 ? -1 : _beforeEmailId[groupId],
                    Old = before,
                    New = now
                });
            }

            return changes;
        }

        /// <summary>Names typed here that differ from the database; a new group counts only while the address is ticked on it.</summary>
        private List<NameChange> NameChanges()
        {
            return _namesStaged
                .Where(p => p.Value != LoadedNameOf(p.Key))
                .Where(p => !_newGroups.Contains(p.Key) || _now.ContainsKey(p.Key))
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
                lbSummary.Text = _address == ""
                    ? "เลือกหรือพิมพ์อีเมลในช่องด้านบน แล้วกด เปิด  (อีเมลใหม่ก็พิมพ์ได้)"
                    : HINT;
            }
            else
            {
                int added = changes.Count(c => c.Old == 0);
                int removed = changes.Count(c => c.New == 0);
                int switched = changes.Count - added - removed;

                lbSummary.Text = "จะบันทึก " + total + " รายการ: เพิ่ม " + added + ", ลบ " + removed +
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
                    ? "group ใหม่ยังไม่มี TO: เปิดอีเมล ติ๊ก group ใหม่ แล้วกด Save"
                    : "ไม่มีอะไรเปลี่ยน") + Environment.NewLine + HINT;
                return;
            }

            List<int> noTo = GroupsLeftWithoutTo(changes);
            if (noTo.Count > 0)
            {
                frmError.ShowError(this, SAVE_STOPPED + Environment.NewLine +
                    string.Join(", ", noTo.Select(id => "Group #" + id).ToArray()) + " จะไม่มีผู้รับ TO เหลือ" + Environment.NewLine +
                    "เพิ่ม TO คนอื่นใน group นั้นก่อน หรือเปลี่ยนเป็น CC แทนการลบ");
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

            // read everything again, so the window shows what the database now holds
            string address = _address;
            _address = "";
            _before.Clear();
            _beforeEmailId.Clear();
            _now.Clear();

            string error = LoadAll();
            if (error != "")
            {
                frmError.ShowError(this, "บันทึกแล้ว แต่อ่านข้อมูลใหม่ไม่ได้ ปิดหน้าต่างแล้วเปิดใหม่" + Environment.NewLine + error);
                Close();
                return;
            }

            if (address != "") OpenAddress(address);
            else UpdateSummary();

            lbSummary.Text = "บันทึกลง " + MailSafety.ConfigDatabase + " แล้ว " + saved + " รายการ" + Environment.NewLine + HINT;
        }

        /// <summary>
        /// Groups whose mail would go to nobody: used by customers, with a TO now and none after the changes -
        /// and a group made here whose first address is not TO.
        /// </summary>
        private List<int> GroupsLeftWithoutTo(List<Change> changes)
        {
            List<int> noTo = new List<int>();
            foreach (Change change in changes)
            {
                if (_newGroups.Contains(change.GroupId))
                {
                    if (change.New != RCP_TO) noTo.Add(change.GroupId);
                    continue;
                }

                int before = Get(_toCount, change.GroupId);
                int after = before + (change.New == RCP_TO ? 1 : 0) - (change.Old == RCP_TO ? 1 : 0);

                if (before > 0 && after <= 0 && _usedBy.ContainsKey(change.GroupId)) noTo.Add(change.GroupId);
            }
            return noTo;
        }

        /// <summary>
        /// Statements of one Save in the order they run (one transaction); null = a read failed (shown).
        /// A group made here gets its id now (the next free one), not the one shown while staging.
        /// </summary>
        private List<string> SaveStatements(List<Change> changes, List<NameChange> names, string user)
        {
            List<string> batch = new List<string>();
            Dictionary<int, int> groupIds = new Dictionary<int, int>();

            List<int> newGroups = changes.Select(c => c.GroupId).Where(id => _newGroups.Contains(id)).OrderBy(id => id).ToList();
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

            int newEmailId = int.MinValue;   // the address's id for its adds, found / made once
            foreach (Change change in changes)
            {
                int groupId = MapGroup(groupIds, change.GroupId);

                if (change.Old == 0)
                {
                    if (newEmailId == int.MinValue)
                    {
                        newEmailId = EmailIdForAdd(batch, user);
                        if (newEmailId == int.MinValue) return null;
                    }
                    batch.Add(_emailSql.insertGroupMember(Member(groupId, newEmailId, change.New)));
                }
                else if (change.New == 0)
                {
                    batch.Add(_emailSql.deleteGroupMember(Member(groupId, change.EmailId, 0)));
                }
                else
                {
                    batch.Add(_emailSql.updateRecipientType(Member(groupId, change.EmailId, change.New)));
                }
            }

            foreach (NameChange name in names)
            {
                // a new group without a name has no row to write
                if (name.New == "" && _newGroups.Contains(name.GroupId)) continue;
                batch.Add(_specSql.saveGroupName(MapGroup(groupIds, name.GroupId), name.New, user));
            }

            // log: one row per new group, per address per group (CUSTOMER = who gets the group's mail), per name
            foreach (Change change in changes)
            {
                int groupId = MapGroup(groupIds, change.GroupId);

                if (_newGroups.Contains(change.GroupId)) batch.Add(_specSql.logNewGroup(LOG_PRODUCT, groupId, "", user));

                batch.Add(_specSql.logRecipientChange(LOG_PRODUCT, groupId, UsedByText(change.GroupId, 100), _address,
                    change.Old == 0 ? (int?)null : change.Old, change.New == 0 ? (int?)null : change.New, user));
            }

            foreach (NameChange name in names)
            {
                if (name.New == "" && _newGroups.Contains(name.GroupId)) continue;
                batch.Add(_specSql.logGroupNameChange(LOG_PRODUCT, MapGroup(groupIds, name.GroupId), name.Old, name.New, user));
            }

            batch.Add(_emailSql.clearAppUser());
            return batch;
        }

        /// <summary>What Save writes and whose mail it touches, before anything is written.</summary>
        private string SaveQuestion(List<Change> changes, List<NameChange> names)
        {
            List<string> lines = new List<string>();
            lines.Add("บันทึกลง " + MailSafety.ConfigDatabase + "?");

            if (changes.Count > 0)
            {
                lines.Add("");
                lines.Add(_address + (KnownAddress(_address) == null ? "  (อีเมลใหม่)" : ""));

                List<Change> created = changes.Where(c => _newGroups.Contains(c.GroupId)).ToList();
                List<Change> added = changes.Where(c => c.Old == 0 && !_newGroups.Contains(c.GroupId)).ToList();
                List<Change> removed = changes.Where(c => c.New == 0).ToList();
                List<Change> switched = changes.Where(c => c.Old != 0 && c.New != 0).ToList();

                if (created.Count > 0) lines.Add("  สร้าง group ใหม่ (เป็น TO): " + string.Join(", ", created.Select(c => GroupText(c.GroupId)).ToArray()));
                if (added.Count > 0) lines.Add("  เพิ่มเข้า: " + string.Join(", ", added.Select(c => "#" + c.GroupId + " " + TypeName(c.New)).ToArray()));
                if (removed.Count > 0) lines.Add("  ลบออกจาก: " + string.Join(", ", removed.Select(c => "#" + c.GroupId).ToArray()));
                if (switched.Count > 0) lines.Add("  สลับ TO/CC: " + string.Join(", ", switched.Select(c => "#" + c.GroupId + " → " + TypeName(c.New)).ToArray()));

                List<int> groups = changes.Select(c => c.GroupId).Where(id => !_newGroups.Contains(id)).OrderBy(id => id).ToList();
                if (groups.Count > 0)
                {
                    lines.Add("");
                    lines.Add("มีผลกับเมลของ:");
                    foreach (int groupId in groups.Take(CONFIRM_MAX_LINES))
                    {
                        string used = UsedByText(groupId, USED_BY_MAX_CHARS);
                        lines.Add("  - #" + groupId + ": " + (used == "" ? "(ไม่มีลูกค้าใช้)" : used));
                    }
                    if (groups.Count > CONFIRM_MAX_LINES) lines.Add("  ... และอีก " + (groups.Count - CONFIRM_MAX_LINES) + " group");

                    if (groups.Contains(GROUP_INTERNAL)) lines.Add("#0 ใช้ร่วมกับลูกค้าอื่นและโปรแกรมอื่น (เช่น uITLA)");
                }
            }

            List<NameChange> renamed = names.Where(n => !_newGroups.Contains(n.GroupId)).ToList();
            if (renamed.Count > 0)
            {
                lines.Add("");
                lines.Add("ชื่อ group: " + string.Join(", ", renamed.Take(CONFIRM_MAX_LINES)
                    .Select(n => "#" + n.GroupId + " = " + (n.New == "" ? "(ไม่มีชื่อ)" : n.New)).ToArray()));
            }

            return string.Join(Environment.NewLine, lines.ToArray());
        }

        /// <summary>
        /// EMAIL_ID for the groups the opened address joins: the one it already has on a group, else the one info_email
        /// has, else a new one (its INSERT goes into the batch). int.MinValue = a read failed (shown).
        /// </summary>
        private int EmailIdForAdd(List<string> batch, string user)
        {
            if (_beforeEmailId.Count > 0) return _beforeEmailId.Values.First();

            DataTable found = Busy(() => _emailCon.searchEmailByAddress(new SettingEmailProperty { emailAddress = _address }));
            if (_emailCon.lastError != "")
            {
                frmError.ShowError(this, SAVE_STOPPED + Environment.NewLine + _emailCon.lastError);
                return int.MinValue;
            }

            if (found.Rows.Count > 0) return ToInt(found.Rows[0]["EMAIL_ID"]);

            int id = Busy(() => _emailCon.maxEmailId());
            if (_emailCon.lastError != "")
            {
                frmError.ShowError(this, SAVE_STOPPED + Environment.NewLine + _emailCon.lastError);
                return int.MinValue;
            }

            id++;
            batch.Add(_emailSql.insertEmail(new SettingEmailProperty { emailId = id, emailAddress = _address, createUser = user }));
            return id;
        }

        // =====================================================================
        // helpers
        // =====================================================================
        private static SettingEmailProperty Member(int groupId, int emailId, int recipientId)
        {
            return new SettingEmailProperty { groupId = groupId, emailId = emailId, recipientId = recipientId };
        }

        private static int MapGroup(Dictionary<int, int> groupIds, int groupId)
        {
            int written;
            return groupIds.TryGetValue(groupId, out written) ? written : groupId;
        }

        private static int Get(Dictionary<int, int> map, int key)
        {
            int value;
            return map.TryGetValue(key, out value) ? value : 0;
        }

        private static string TypeName(int type)
        {
            return type == RCP_TO ? "TO" : type == RCP_CC ? "CC" : "";
        }

        private static bool SameAddress(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>One name per line, capped so the tooltip stays on screen.</summary>
        private static string FullList(List<string> names)
        {
            string text = string.Join(Environment.NewLine, names.Take(CONFIRM_MAX_LINES * 2).Select(n => "  - " + n).ToArray());
            if (names.Count > CONFIRM_MAX_LINES * 2) text += Environment.NewLine + "  ... และอีก " + (names.Count - CONFIRM_MAX_LINES * 2);
            return text;
        }

        /// <summary>"a@x, b@x" (GROUP_CONCAT) as one indented address per line; "  -" when there is none.</summary>
        private static string AddressLines(object list)
        {
            string text = list == null || list == DBNull.Value ? "" : Convert.ToString(list);
            if (text.Trim() == "") return "  -";

            return string.Join(Environment.NewLine,
                text.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Select(a => "  " + a).ToArray());
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
