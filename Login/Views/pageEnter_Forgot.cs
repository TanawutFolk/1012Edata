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
    public partial class pageEnter_Forgot : UserControl
    {
        LoginControllors _controllors = new LoginControllors();
        frmLogin_Main _frmMain;
        public pageEnter_Forgot(frmLogin_Main obj)
        {
            InitializeComponent();
            _frmMain = obj;
        }

        private void pageEnter_Forgot_Load(object sender, EventArgs e)
        {
            this.lblName.Text = _frmMain._resultLogin.empDetial.name + " " + _frmMain._resultLogin.empDetial.sername;
            this.lblName.Left = (this.panel1.Width / 2) - (lblName.Width / 2);

            this.lblEmail.Text = _frmMain._resultLogin.empDetial.email;
            this.lblEmail.Left = (this.panel1.Width / 2) - (lblEmail.Width / 2);
        }

        private void bttNext_Click(object sender, EventArgs e)
        {
            DateTime _selectConf = this.dtpSelectConf.Value;
            DateTime _startWork = Convert.ToDateTime(_frmMain._resultLogin.empDetial.startwork);

            //Check emp startwork.
            if (_selectConf.Date == _startWork.Date)
            {
                if (_controllors.SendOTP(_frmMain._resultLogin.empDetial))
                {
                    //Confirm OTP.
                    frmEnter_OTP _frmOTP = new frmEnter_OTP(_frmMain._resultLogin.empDetial);
                    _frmOTP.ShowDialog();
                    if (_frmOTP.DialogResult == DialogResult.OK)
                    {
                        //Set New Pass
                        string newPass = _controllors.GetUniqueKey(8);
                        _frmMain._resultLogin.empDetial.pass = newPass;
                        if (_controllors.UpdatePassLoginFitelGroup(_frmMain._resultLogin.empDetial))
                        {
                            //Sent new pass
                            if (_controllors.SendNewPassword(_frmMain._resultLogin.empDetial) == true)
                            {
                                frmSuccess _succ = new frmSuccess(1);
                                _succ.ShowDialog();

                                //Clear panel Body.
                                _frmMain.panBody.Controls.Clear();

                                //Load page search.
                                pageEnter_Password _pageEnter_Password = new pageEnter_Password(_frmMain);
                                _frmMain.panBody.Controls.Add(_pageEnter_Password);
                                _pageEnter_Password.Dock = DockStyle.Fill;
                                _pageEnter_Password.BringToFront();
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("You choose the wrong start date. Please choose again.", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void bttBack_Click(object sender, EventArgs e)
        {
            //Clear panel Body.
            _frmMain.panBody.Controls.Clear();

            //Load page search.
            pageEnter_Password _pageEnter_Password = new pageEnter_Password(_frmMain);
            _frmMain.panBody.Controls.Add(_pageEnter_Password);
            _pageEnter_Password.Dock = DockStyle.Fill;
            _pageEnter_Password.BringToFront();
        }
    }
}
