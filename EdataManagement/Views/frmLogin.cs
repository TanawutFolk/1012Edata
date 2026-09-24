using EdataManagement.Controllers;
using EdataManagement.Property;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdataManagement.Views
{
    /// <summary>
    /// Gate in front of the Setting screen (same idea as the 1st Lot / RawMat login).
    ///
    /// Two steps:
    ///   1) EMPCODE + portal password against the Security Center API
    ///      -> TURNED OFF for now (see btnLogin_Click), the Password box is ignored.
    ///   2) EMPCODE must exist in mfg.info_employees, must not be resigned,
    ///      and must be listed in user_setting_edata with INUSE = 1 -> gives the ROLE.
    ///
    /// On success DialogResult is OK and CurrentUser holds the signed in person.
    /// </summary>
    public partial class frmLogin : Form
    {
        private readonly SettingUserControllers _userCon = new SettingUserControllers();

        // kept for step 1 - used once the portal check is switched back on
        private readonly SecurityCenterControllers _securityCon = new SecurityCenterControllers();

        /// <summary>Person signed in for this run of the application.</summary>
        public static SettingUserProperty CurrentUser { get; private set; }

        public static void SignOut()
        {
            CurrentUser = null;
        }

        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string empCode = (txtEmpCode.Text ?? "").Trim();

            if (string.IsNullOrEmpty(empCode))
            {
                ShowStatus("Please type your employee code.");
                txtEmpCode.Focus();
                return;
            }

            // ---------------------------------------------------------------
            // 1) portal password - OFF while testing
            //    The Password box stays on screen but is not read, so any value
            //    (or none) passes this step.
            //
            //    To switch it on:
            //      - uncomment the block below
            //      - delete the fake SignInResultProperty line
            //      - require the password in the empty check above
            //
            //string password = txtPassword.Text;
            //SignInResultProperty signIn;
            //btnLogin.Enabled = false;
            //Cursor = Cursors.WaitCursor;
            //try
            //{
            //    signIn = _securityCon.SignIn(empCode, password);
            //}
            //finally
            //{
            //    Cursor = Cursors.Default;
            //    btnLogin.Enabled = true;
            //}
            // ---------------------------------------------------------------
            SignInResultProperty signIn = new SignInResultProperty() { Status = true };

            if (signIn == null || signIn.Status == false)
            {
                string detail = (signIn == null || string.IsNullOrWhiteSpace(signIn.Message))
                    ? ""
                    : Environment.NewLine + "(" + signIn.Message + ")";

                ShowStatus("Employee code or password is not correct." + detail);
                txtPassword.Clear();
                txtPassword.Focus();
                return;
            }

            // ---------------------------------------------------------------
            // 2) employee + whitelist
            // ---------------------------------------------------------------
            SettingUserProperty user = _userCon.searchUserForLogin(
                new SettingUserProperty { empCode = empCode });

            if (!string.IsNullOrEmpty(_userCon.lastError))
            {
                ShowStatus("Cannot reach the database." + Environment.NewLine + _userCon.lastError);
                return;
            }

            if (user == null)
            {
                ShowStatus("Employee code " + empCode + " was not found in the employee database.");
                txtEmpCode.SelectAll();
                txtEmpCode.Focus();
                return;
            }

            if (!user.isActiveEmployee)
            {
                ShowStatus("This employee code is marked as resigned." + Environment.NewLine +
                           "Please contact the system administrator.");
                return;
            }

            if (string.IsNullOrEmpty(user.role))
            {
                ShowStatus(user.fullName + " (" + user.empCode + ")" + Environment.NewLine +
                           "is not allowed to open Setting." + Environment.NewLine +
                           "Ask an administrator to add you on the Login Permission tab.");
                return;
            }

            CurrentUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ShowStatus(string message)
        {
            lbLoginStatus.Text = message;
        }
    }
}
