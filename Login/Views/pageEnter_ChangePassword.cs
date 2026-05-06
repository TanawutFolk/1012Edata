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
    public partial class pageEnter_ChangePassword : UserControl
    {
        frmChangePass_Main _frmMain;
        LoginControllors _controllors = new LoginControllors();

        public pageEnter_ChangePassword(frmChangePass_Main obj)
        {
            InitializeComponent();
            _frmMain = obj;
        }

        private void pageEnter_ChangePassword_Load(object sender, EventArgs e)
        {
            lblName.Text = _frmMain._empDetial.name;
            this.lblName.Text = _frmMain._empDetial.name + " " + _frmMain._empDetial.sername;
            this.lblName.Left = (this.panel1.Width / 2) - (lblName.Width / 2);
        }

        private void bttNext_Click(object sender, EventArgs e)
        {
            //Check empty
            if (this.txtPass.Text.Trim() == "" || this.txtConf.Text.Trim() == "")
            { return;}

            //Check Digit
            if (this.txtPass.Text.Trim().Length < 8 || this.txtConf.Text.Trim().Length < 8)
            { MessageBox.Show(" Use 8 characters or more.", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop); return;}

            //Check Pass = Confirm
            if (this.txtPass.Text.Trim() == this.txtConf.Text.Trim())
            {
                //Send OTP to E-mail
                if (_controllors.SendOTP(_frmMain._empDetial))
                {
                    frmEnter_OTP _frmOTP = new frmEnter_OTP(_frmMain._empDetial);
                    _frmOTP.ShowDialog();
                    if (_frmOTP.DialogResult == DialogResult.OK)
                    {
                        _frmMain._empDetial.pass = this.txtPass.Text.Trim();
                        if (_controllors.UpdatePassLoginFitelGroup(_frmMain._empDetial) == true)
                        {
                            frmSuccess _succ = new frmSuccess(1);
                            _succ.ShowDialog();

                            _frmMain.DialogResult = DialogResult.OK;
                            _frmMain.Close();
                        }
                    }
                }
            }
        }

        private void bttBack_Click(object sender, EventArgs e)
        {
            //Clear panel Body.
            _frmMain.panBody.Controls.Clear();

            //Load page search.
            pageBefore_ChangPassword _pageBefore_ChangPassword = new pageBefore_ChangPassword(_frmMain);
            _frmMain.panBody.Controls.Add(_pageBefore_ChangPassword);
            _pageBefore_ChangPassword.Dock = DockStyle.Fill;
            _pageBefore_ChangPassword.BringToFront();
        }

        private void txtPass_TextChanged(object sender, EventArgs e)
        {
            this.lblLimit_Pass.Text = this.txtPass.Text.Length + "/20";
        }

        private void txtConf_TextChanged(object sender, EventArgs e)
        {
            this.lblLimit_Conf.Text = this.txtConf.Text.Length + "/20";
        }
    }
}
