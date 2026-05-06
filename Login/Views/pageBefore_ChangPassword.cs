using Login.Controllors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Login
{
    public partial class pageBefore_ChangPassword : UserControl
    {
        LoginControllors _controllors = new LoginControllors();
        frmChangePass_Main _frmMain;
        public pageBefore_ChangPassword(frmChangePass_Main obj)
        {
            InitializeComponent();
            _frmMain = obj;
        }

        private void bttNext_Click(object sender, EventArgs e)
        {
            if (this.txtPass.Text.Trim() == "") { return; }
            _frmMain._empDetial.pass = this.txtPass.Text.Trim();

            if (_controllors.LoginFitelGroup(_frmMain._empDetial) == true)
            {
                //Clear panel Body.
                _frmMain.panBody.Controls.Clear();

                //Load page search.
                pageEnter_ChangePassword _pageEnter_ChangePassword = new pageEnter_ChangePassword(_frmMain);
                _frmMain.panBody.Controls.Add(_pageEnter_ChangePassword);
                _pageEnter_ChangePassword.Dock = DockStyle.Fill;
                _pageEnter_ChangePassword.BringToFront();
            }
            else
            {
                MessageBox.Show(" Wrong password. Try again or click Forgot password to reset it.", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void pageBefore_ChangPassword_Load(object sender, EventArgs e)
        {
            lblName.Text = _frmMain._empDetial.name;
            this.lblName.Text = _frmMain._empDetial.name + " " + _frmMain._empDetial.sername;
            this.lblName.Left = (this.panel1.Width / 2) - (lblName.Width / 2);
        }

        private void txtPass_TextChanged(object sender, EventArgs e)
        {
            this.lblLimit_Pass.Text = this.txtPass.Text.Length + "/20";

        }
    }
}
