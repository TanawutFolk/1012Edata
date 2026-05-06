using Login.Property;
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
    public partial class pageEnter_Password : UserControl
    {
        LoginControllors _controllors = new LoginControllors();
        frmLogin_Main _frmMain;
       
        public pageEnter_Password(frmLogin_Main obj)
        {
            InitializeComponent();
            _frmMain = obj;
        }

        private void pageEnter_Password_Load(object sender, EventArgs e)
        {
            _frmMain._resultLogin.empDetial.pass = "";

            this.lblName.Text = _frmMain._resultLogin.empDetial.name + " " + _frmMain._resultLogin.empDetial.sername;
            this.lblName.Left = (this.panel1.Width / 2) - (lblName.Width / 2);

            this.txtPass.Focus();
        }

        private void lblForgotPass_Click(object sender, EventArgs e)
        {
            //Clear panel Body.
            _frmMain.panBody.Controls.Clear();

            //Load page search.
            pageEnter_Forgot _pageEnter_Forgot = new pageEnter_Forgot(_frmMain);
            _frmMain.panBody.Controls.Add(_pageEnter_Forgot);
            _pageEnter_Forgot.Dock = DockStyle.Fill;
            _pageEnter_Forgot.BringToFront();
        }

        private void txtPass_TextChanged(object sender, EventArgs e)
        {
            this.lblLimit.Text = this.txtPass.Text.Length + "/20";
        }

        private void bttNext_Click(object sender, EventArgs e)
        {
            if (this.txtPass.Text.Trim() == "") { return; }
            _frmMain._resultLogin.empDetial.pass = this.txtPass.Text.Trim();

            if (_controllors.LoginFitelGroup(_frmMain._resultLogin.empDetial) == true)
            {
                _frmMain._resultLogin.loginStatus = true;
                _frmMain.Close();
            }
            else
            {
                MessageBox.Show(" Wrong password. Try again or click Forgot password to reset it.", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void bttBack_Click(object sender, EventArgs e)
        {
            //Clear panel Body.
            _frmMain.panBody.Controls.Clear();

            //Load page search.
            pageEnter_User _pageEnter_User = new pageEnter_User(_frmMain);
            _frmMain.panBody.Controls.Add(_pageEnter_User);
            _pageEnter_User.Dock = DockStyle.Fill;
            _pageEnter_User.BringToFront();
        }

        private void lblChangePass_Click(object sender, EventArgs e)
        {

        }
    }
}
