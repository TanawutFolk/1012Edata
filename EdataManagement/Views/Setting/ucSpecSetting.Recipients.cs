using EdataManagement.Controllers;
using EdataManagement.Property;
using EdataManagement.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EdataManagement.Views.Setting
{
    /// <summary>
    /// Recipients panel of ucSpecSetting: who is on the group of the ticked specs - the Recipient Group list decides
    /// which - or, with nothing ticked, on the group most Email specs of the customer use. Read only.
    /// Members, group names and new groups are edited in Manage Recipients (EmailGroupsDialog: by address or by
    /// group), opened by the button here - on the group shown - or by double-clicking an address. That window saves by itself, so it
    /// opens only while no spec edit is staged, and the page is read again after it saved.
    /// </summary>
    public partial class ucSpecSetting
    {
        private const int RCP_TO = 1;

        private const string COL_RCP_TYPE = "RECIPIENT_TYPE";
        private const string COL_RCP_ID = "RECIPIENT_ID";
        private const string COL_RCP_ADDRESS = "EMAIL_ADDRESS";

        private SettingEmailControllers _emailCon;

        /// <summary>Members of every group looked at, as read; emptied whenever the page reads the database again.</summary>
        private readonly Dictionary<int, DataTable> _members = new Dictionary<int, DataTable>();

        /// <summary>Group names (info_group_name), shown in the Recipient Group list and the panel title.</summary>
        private readonly Dictionary<int, string> _groupNames = new Dictionary<int, string>();

        /// <summary>Group on the panel; -1 = a message instead of a group.</summary>
        private int _shownGroupId = -1;

        private void WireRecipientEvents()
        {
            btnRcpAction.Click += delegate { OpenManageRecipients(""); };
            dtgRecipients.CellDoubleClick += dtgRecipients_CellDoubleClick;
            dtgRecipients.CellFormatting += dtgRecipients_CellFormatting;
            dtgRecipients.DataBindingComplete += delegate { FormatRecipientGrid(); };
        }

        private void SetupRecipients()
        {
            _emailCon = new SettingEmailControllers();

            SettingUi.StyleGrid(dtgRecipients, 30, 9F, 32);

            // the one way to edit recipients; always there, next to the title
            btnRcpAction.ButtonText = "Manage Recipients";
            btnRcpAction.Text = btnRcpAction.ButtonText;
            btnRcpAction.SetBounds(pnRecipients.ClientSize.Width - 12 - 170, 6, 170, 28);
            btnRcpAction.Visible = true;
            lbRcpTitle.Width = btnRcpAction.Left - 8 - lbRcpTitle.Left;
            toolTipAlert.SetToolTip(btnRcpAction, "เพิ่ม / ลบ / สลับ TO-CC ผู้รับ: ทีละคนหลาย group หรือทีละ group หลายคน, ตั้งชื่อ group, สร้าง group ใหม่");

            // the grid takes the room of the old Add row; one line under it says how to edit
            Label hint = new Label();
            hint.AutoSize = false;
            hint.Text = "ดูอย่างเดียว - กด Manage Recipients เพื่อแก้ หรือดับเบิลคลิกอีเมล";
            hint.Font = lbRcpNote.Font;
            hint.ForeColor = lbRcpNote.ForeColor;
            hint.TextAlign = ContentAlignment.MiddleLeft;
            hint.SetBounds(dtgRecipients.Left, pnRecipients.ClientSize.Height - 30, dtgRecipients.Width, 22);
            pnRecipients.Controls.Add(hint);

            dtgRecipients.Height = hint.Top - 6 - dtgRecipients.Top;
            lbRcpEmpty.Height = dtgRecipients.Height;

            ShowRecipientsMessage("Recipients", "");
        }

        // =====================================================================
        // which group to show
        // =====================================================================
        /// <summary>Put the group that matters for what is ticked / picked on the panel, or say why there is none.</summary>
        private void RefreshRecipients()
        {
            if (_emailCon == null) return;

            string customer = SelectedCustomerName();
            if (_specs == null || customer == "")
            {
                ShowRecipientsMessage("Recipients", "ยังไม่ได้เลือกลูกค้า");
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

                if (groups.Count > 1)
                {
                    ShowRecipientsMessage("Recipients",
                        "spec ที่ติ๊กใช้ " + groups.Count + " group: " + string.Join(", ", groups.Select(g => "#" + g)) +
                        Environment.NewLine + Environment.NewLine + "เลือก Recipient Group เดียวเพื่อดูผู้รับ");
                    return;
                }

                if (groups.Count == 0 || FindId(_groups, groups[0]) == null)
                {
                    ShowRecipientsMessage("Recipients", groups.Count == 0
                        ? "spec ที่ติ๊กยังไม่มี Group"
                        : "ไม่พบ Group #" + groups[0] + " ในฐานข้อมูล");
                    return;
                }

                ShowGroup(groups[0], "ใช้กับ " + ticked.Count + " spec ที่ติ๊ก");
                return;
            }

            int notOnIt = ticked.Count(r => ToInt(r[COL_GROUP]) != groupId);
            ShowGroup(groupId, notOnIt > 0
                ? "เลือกให้ " + notOnIt + " spec ที่ติ๊ก กด Apply เพื่อย้ายมา group นี้"
                : "ใช้กับ " + ticked.Count + " spec ที่ติ๊ก");
        }

        /// <summary>Nothing ticked: the group most Email specs of the customer use.</summary>
        private void ShowCustomerGroup(string customer)
        {
            List<DataRow> email = _specs.Rows.Cast<DataRow>().Where(r => ToInt(r[COL_SEND]) == SEND_EMAIL).ToList();

            if (email.Count == 0)
            {
                ShowRecipientsMessage("Recipients", customer + " ไม่มี spec ที่ส่ง Email");
                return;
            }

            List<IGrouping<int, DataRow>> byGroup = email
                .GroupBy(r => ToInt(r[COL_GROUP]))
                .OrderByDescending(g => g.Count())
                .ToList();

            ShowGroup(byGroup[0].Key, byGroup.Count > 1
                ? "spec Email ของ " + customer + " ใช้ " + byGroup.Count + " group นี่คือ group ของ " + byGroup[0].Count() + " spec ติ๊ก spec เพื่อดู group อื่น"
                : "ใช้กับ " + email.Count + " spec Email ของ " + customer + " ติ๊ก spec เพื่อเปลี่ยน group");
        }

        // =====================================================================
        // panel content
        // =====================================================================
        private void ShowGroup(int groupId, string contextNote)
        {
            DataTable members = MembersOf(groupId);
            if (members == null)
            {
                ShowRecipientsMessage("Recipients", "อ่าน Group #" + groupId + " ไม่ได้" + Environment.NewLine + _emailCon.lastError);
                return;
            }

            string name = NameOf(groupId);

            _shownGroupId = groupId;
            lbRcpTitle.Text = "Group #" + groupId + (name == "" ? "" : "   " + name);
            toolTipAlert.SetToolTip(lbRcpTitle, lbRcpTitle.Text);

            // #0 also mails other programs, said every time it is on screen
            bool internalGroup = groupId == GROUP_INTERNAL;
            lbRcpNote.ForeColor = internalGroup ? SettingUi.ALERT_WARN_TEXT : SettingUi.ALERT_NONE_TEXT;
            lbRcpNote.Text = internalGroup ? "#0 ใช้ร่วมกัน (ลูกค้าอื่น, uITLA) แก้ผู้รับมีผลกับทุกที่ที่ใช้ #0" : contextNote;
            toolTipAlert.SetToolTip(lbRcpNote, lbRcpNote.Text);

            lbRcpEmpty.Visible = members.Rows.Count == 0;
            lbRcpEmpty.Text = "Group #" + groupId + " ยังไม่มีผู้รับ";
            dtgRecipients.Visible = members.Rows.Count > 0;

            // TO first, then CC - reads like the mail header
            members.DefaultView.Sort = COL_RCP_ID + " ASC, " + COL_RCP_ADDRESS + " ASC";
            if (dtgRecipients.DataSource != members) dtgRecipients.DataSource = members;
            FormatRecipientGrid();
        }

        private void ShowRecipientsMessage(string title, string message)
        {
            _shownGroupId = -1;
            lbRcpTitle.Text = title;
            lbRcpNote.Text = "";

            dtgRecipients.DataSource = null;
            dtgRecipients.Visible = false;
            lbRcpEmpty.Text = message;
            lbRcpEmpty.Visible = true;
        }

        private void FormatRecipientGrid()
        {
            if (dtgRecipients.Columns.Count == 0) return;

            foreach (string hidden in new[] { "EMAIL_ID", COL_RCP_ID, "INUSE" })
            {
                if (dtgRecipients.Columns.Contains(hidden)) dtgRecipients.Columns[hidden].Visible = false;
            }

            SettingUi.SetColumn(dtgRecipients, COL_RCP_ADDRESS, "EMAIL ADDRESS", 300);
            SettingUi.SetColumn(dtgRecipients, COL_RCP_TYPE, "TYPE", 55);

            if (dtgRecipients.Columns.Contains(COL_RCP_TYPE))
            {
                dtgRecipients.Columns[COL_RCP_TYPE].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            dtgRecipients.ClearSelection();
        }

        /// <summary>TO blue / CC grey.</summary>
        private void dtgRecipients_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dtgRecipients.Columns[e.ColumnIndex].Name != COL_RCP_TYPE) return;

            Color fore = Convert.ToString(e.Value) == "TO" ? SettingUi.TYPE_TO_TEXT : SettingUi.TYPE_CC_TEXT;
            e.CellStyle.Font = _gridBold;
            e.CellStyle.ForeColor = fore;
            e.CellStyle.SelectionForeColor = fore;
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void dtgRecipients_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataRowView item = dtgRecipients.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (item != null) OpenManageRecipients(Convert.ToString(item[COL_RCP_ADDRESS]));
        }

        // =====================================================================
        // Manage Recipients
        // =====================================================================
        /// <summary>address = the one to open at once ("" = the user picks).</summary>
        private void OpenManageRecipients(string address)
        {
            if (_emailCon == null) return;

            // the window writes on its own and the page is read again after it: staged spec edits would be lost
            int pending = PendingCount();
            if (pending > 0)
            {
                frmError.ShowError(this, "มีการแก้ spec ที่ยังไม่ได้ Save " + pending + " รายการ" + Environment.NewLine +
                    "กด Save หรือ Cancel ก่อน แล้วค่อยเปิด Manage Recipients");
                return;
            }

            int saved = EmailGroupsDialog.Open(this, AllProducts(), address, _shownGroupId);
            if (saved > 0)
            {
                ReloadAfterSave("บันทึกลง " + MailSafety.ConfigDatabase + " แล้ว: Manage Recipients " + saved + " รายการ");
            }
        }

        // =====================================================================
        // data helpers
        // =====================================================================
        /// <summary>Members of a group, read from the database the first time.</summary>
        private DataTable MembersOf(int groupId)
        {
            DataTable members;
            if (_members.TryGetValue(groupId, out members)) return members;

            members = Busy(() => _emailCon.searchGroupMembers(new SettingEmailProperty { groupId = groupId }));
            if (!string.IsNullOrEmpty(_emailCon.lastError)) return null;

            _members[groupId] = members;
            return members;
        }

        /// <summary>Forget what was read (Save / Manage Recipients / another customer): the next look reads again.</summary>
        private void ClearRecipientCache()
        {
            _members.Clear();
            _shownGroupId = -1;
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

        /// <summary>Read every name once per list load; a failed read (a database without info_group_name) = no names.</summary>
        private void LoadGroupNames()
        {
            _groupNames.Clear();

            DataTable names = Busy(() => _specCon.searchGroupNames());
            foreach (DataRow row in names.Rows)
            {
                _groupNames[ToInt(row["GROUP_ID"])] = Convert.ToString(row["GROUP_NAME"]).Trim();
            }
        }

        private string NameOf(int groupId)
        {
            string name;
            return _groupNames.TryGetValue(groupId, out name) ? name : "";
        }

        /// <summary>"#12   QA Coherent  (3 อีเมล)" - number, name if any, how many addresses.</summary>
        private string GroupListText(int groupId, int memberCount)
        {
            string name = NameOf(groupId);
            return "#" + groupId + "   " + (name == "" ? "" : name + "  ") + "(" + memberCount + " อีเมล)";
        }
    }
}
