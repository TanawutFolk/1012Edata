using EdataManagement.Controllers;
using EdataManagement.Property;

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace EdataManagement.Views.Setting
{
    /// <summary>
    /// "Login Permission" page (ADMIN only) - who may open Setting, table user_setting_edata.
    /// Every change here is written straight away (one row at a time), so the footer Save has nothing to do.
    /// </summary>
    public partial class ucLoginSetting : UserControl, ISettingPage
    {
        private const string COL_TOGGLE = "colToggle";

        // created on first show, not in the constructor, so the designer can open this control
        private SettingUserControllers _userCon;
        private bool _initialized;

        public event EventHandler SaveButtonTextChanged;

        public ucLoginSetting()
        {
            InitializeComponent();
            SettingUi.StyleButton(btnAddUser);

            btnAddUser.Click += btnAddUser_Click;
            dtgPermission.CellContentClick += dtgPermission_CellContentClick;
            dtgPermission.CellFormatting += dtgPermission_CellFormatting;
        }

        // =====================================================================
        // ISettingPage
        // =====================================================================
        public string PageTitle
        {
            get { return "Login Permission Setting"; }
        }

        public string SaveButtonText
        {
            get { return "Save"; }
        }

        /// <summary>Nothing is ever staged on this page.</summary>
        public bool HasPendingChanges
        {
            get { return false; }
        }

        public void PageShown()
        {
            if (!_initialized)
            {
                _initialized = true;
                _userCon = new SettingUserControllers();
                SetupPermissionGrid();
            }

            // someone may have been added from another PC meanwhile
            LoadPermissionUsers();
        }

        public void SavePage()
        {
            lbPermissionInfo.Text = "Changes on this page are saved as soon as you make them.   |   " + CountText();
        }

        public bool CancelPage()
        {
            return false;
        }

        public bool ConfirmLeavePage()
        {
            return true;
        }

        // keep the compiler quiet: the event is part of the page contract but this page never raises it
        protected virtual void OnSaveButtonTextChanged()
        {
            EventHandler handler = SaveButtonTextChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        // =====================================================================
        // grid
        // =====================================================================
        private void SetupPermissionGrid()
        {
            cbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cbRole.Items.Clear();
            cbRole.Items.Add(SettingUserProperty.ROLE_USER);
            cbRole.Items.Add(SettingUserProperty.ROLE_ADMIN);
            cbRole.SelectedIndex = 0;

            SettingUi.StyleGrid(dtgPermission);
        }

        private int _activeCount;
        private int _adminCount;

        private string CountText()
        {
            return _activeCount + " active user(s), " + _adminCount + " admin(s)";
        }

        private void LoadPermissionUsers()
        {
            DataTable users = _userCon.searchAllUsers();

            if (!string.IsNullOrEmpty(_userCon.lastError))
            {
                frmError.ShowError(this, _userCon.lastError);
                return;
            }

            dtgPermission.DataSource = users;
            FormatPermissionGrid();

            _activeCount = 0;
            _adminCount = 0;
            foreach (DataRow row in users.Rows)
            {
                if (Convert.ToInt32(row["INUSE"]) != 1) continue;
                _activeCount++;
                if (row["ROLE"].ToString().ToUpper() == SettingUserProperty.ROLE_ADMIN) _adminCount++;
            }

            lbPermissionInfo.Text = CountText()
                + "   |   signed in as " + (frmLogin.CurrentUser == null ? "-" : frmLogin.CurrentUser.empCode + " (" + frmLogin.CurrentUser.role + ")")
                + "   |   table user_setting_edata";
        }

        private void FormatPermissionGrid()
        {
            if (dtgPermission.Columns.Count == 0) return;

            if (dtgPermission.Columns.Contains("INUSE")) dtgPermission.Columns["INUSE"].Visible = false;

            SettingUi.SetColumn(dtgPermission, "EMPCODE", "EMP CODE", 90);
            SettingUi.SetColumn(dtgPermission, "EMP_NAME", "NAME", 200);
            SettingUi.SetColumn(dtgPermission, "EMP_DEPT", "DEPT", 110);
            SettingUi.SetColumn(dtgPermission, "ROLE", "ROLE  (click to switch)", 110);
            SettingUi.SetColumn(dtgPermission, "STATUS", "STATUS", 90);
            SettingUi.SetColumn(dtgPermission, "EMP_STATUS", "EMPLOYEE", 90);
            SettingUi.SetColumn(dtgPermission, "REMARK", "REMARK", 120);

            if (!dtgPermission.Columns.Contains(COL_TOGGLE))
            {
                // caption is set per row in CellFormatting (Revoke / Restore)
                dtgPermission.Columns.Add(SettingUi.MakeButtonColumn(COL_TOGGLE, "", 90, SettingUi.REMOVE_TEXT));
            }
        }

        private void dtgPermission_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dtgPermission.Rows.Count) return;

            DataRowView view = dtgPermission.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (view == null) return;

            bool active = Convert.ToInt32(view.Row["INUSE"]) == 1;
            string column = dtgPermission.Columns[e.ColumnIndex].Name;

            if (column == COL_TOGGLE)
            {
                e.Value = active ? "Revoke" : "Restore";
                e.CellStyle.ForeColor = active ? SettingUi.REMOVE_TEXT : SettingUi.TYPE_TO_TEXT;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                e.FormattingApplied = true;
                return;
            }

            if (!active)
            {
                e.CellStyle.ForeColor = SettingUi.TYPE_CC_TEXT;
                e.CellStyle.SelectionForeColor = SettingUi.TYPE_CC_TEXT;
            }

            if (column == "ROLE")
            {
                e.CellStyle.Font = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Bold);
                if (active)
                {
                    e.CellStyle.ForeColor = Convert.ToString(e.Value) == SettingUserProperty.ROLE_ADMIN
                        ? SettingUi.ALERT_WARN_TEXT
                        : SettingUi.TYPE_TO_TEXT;
                    e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                }
            }

            if (column == "EMP_STATUS" && Convert.ToString(e.Value) != "ACTIVE")
            {
                e.CellStyle.ForeColor = SettingUi.REMOVE_TEXT;
                e.CellStyle.SelectionForeColor = SettingUi.REMOVE_TEXT;
            }
        }

        private void dtgPermission_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!SettingUi.IsAdmin()) return;

            DataRowView view = dtgPermission.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (view == null) return;

            string column = dtgPermission.Columns[e.ColumnIndex].Name;

            SettingUserProperty prop = new SettingUserProperty
            {
                empCode = view.Row["EMPCODE"].ToString(),
                role = view.Row["ROLE"].ToString().ToUpper(),
                inuse = Convert.ToInt32(view.Row["INUSE"]),
                createUser = SettingUi.CurrentUserCode()
            };

            if (column == COL_TOGGLE)
            {
                bool active = prop.inuse == 1;

                if (active && !EnsureAnotherAdminExists(prop, "revoke")) return;

                if (!frmConfirm.Ask(this,
                    (active ? "Revoke access for " : "Restore access for ") + prop.empCode + " ?",
                    active ? "Revoke" : "Restore", "Cancel")) return;

                prop.inuse = active ? 0 : 1;

                if (!_userCon.updateUserInuse(prop))
                {
                    frmError.ShowError(this, _userCon.lastError);
                    return;
                }

                LoadPermissionUsers();
                return;
            }

            if (column == "ROLE")
            {
                if (prop.inuse != 1)
                {
                    frmError.ShowError(this, "Restore this user first, then change the role.");
                    return;
                }

                bool wasAdmin = prop.role == SettingUserProperty.ROLE_ADMIN;

                if (wasAdmin && !EnsureAnotherAdminExists(prop, "change the role of")) return;

                prop.role = wasAdmin ? SettingUserProperty.ROLE_USER : SettingUserProperty.ROLE_ADMIN;

                if (!frmConfirm.Ask(this,
                    prop.empCode + " : " + (wasAdmin ? "ADMIN -> USER" : "USER -> ADMIN") + " ?",
                    "Change role", "Cancel")) return;

                if (!_userCon.updateUserRole(prop))
                {
                    frmError.ShowError(this, _userCon.lastError);
                    return;
                }

                LoadPermissionUsers();
            }
        }

        // =====================================================================
        // add user
        // =====================================================================
        private void btnAddUser_Click(object sender, EventArgs e)
        {
            if (_userCon == null) return;

            if (!SettingUi.IsAdmin())
            {
                frmError.ShowError(this, "Only an ADMIN can add a user.");
                return;
            }

            string empCode = (txtAddEmpCode.Text ?? "").Trim();

            if (string.IsNullOrEmpty(empCode))
            {
                frmError.ShowError(this, "Please type an employee code.");
                txtAddEmpCode.Focus();
                return;
            }

            // employee must exist, so a typo does not create a dead permission row
            SettingUserProperty employee = _userCon.searchEmployee(new SettingUserProperty { empCode = empCode });

            if (!string.IsNullOrEmpty(_userCon.lastError))
            {
                frmError.ShowError(this, _userCon.lastError);
                return;
            }

            if (employee == null)
            {
                frmError.ShowError(this, "Employee code " + empCode + " was not found in mfg.info_employees.");
                return;
            }

            if (!employee.isActiveEmployee)
            {
                if (!frmConfirm.Ask(this,
                    employee.fullName + " is marked as RESIGNED." + Environment.NewLine + "Add anyway?",
                    "Add anyway", "Cancel")) return;
            }

            string role = cbRole.SelectedItem == null
                ? SettingUserProperty.ROLE_USER
                : cbRole.SelectedItem.ToString();

            if (!frmConfirm.Ask(this,
                "Give " + employee.fullName + " (" + employee.empCode + ", " + employee.empDept + ")" +
                Environment.NewLine + "access to Setting as " + role + " ?",
                "Add user", "Cancel")) return;

            SettingUserProperty prop = new SettingUserProperty
            {
                empCode = employee.empCode,
                role = role,
                remark = employee.empDept,
                createUser = SettingUi.CurrentUserCode()
            };

            if (!_userCon.insertUser(prop))
            {
                frmError.ShowError(this, _userCon.lastError);
                return;
            }

            txtAddEmpCode.Text = "";
            LoadPermissionUsers();
        }

        /// <summary>Block the change when it would remove the last active ADMIN.</summary>
        private bool EnsureAnotherAdminExists(SettingUserProperty prop, string action)
        {
            int others = _userCon.countActiveAdmins(prop);

            if (others > 0) return true;

            frmError.ShowError(this,
                "Cannot " + action + " the last active ADMIN." + Environment.NewLine +
                "Add another ADMIN first, otherwise nobody can open this page again.");

            return false;
        }
    }
}
