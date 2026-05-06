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
    public partial class pageEnter_Create : UserControl
    {
        frmLogin_Main _frmMain;
        
        public pageEnter_Create(frmLogin_Main obj)
        {
            InitializeComponent();
            _frmMain = obj;
        }

        private void pageEnter_Create_Load(object sender, EventArgs e)
        {
            this.lblName.Text = _frmMain._resultLogin.empDetial.name + " " + _frmMain._resultLogin.empDetial.sername;
            this.lblName.Left = (this.panel1.Width / 2) - (lblName.Width / 2);

        }

        private void bttNext_Click(object sender, EventArgs e)
        {
            DateTime _selectConf = this.dtpSelectConf.Value;
            DateTime _startWork = Convert.ToDateTime(_frmMain._resultLogin.empDetial.startwork);

            if (_selectConf.Date == _startWork.Date)
            {
                //Clear panel Body.
                _frmMain.panBody.Controls.Clear();

                //Load page search.
                pageEnter_ConfirmPass _pageEnter_User = new pageEnter_ConfirmPass(_frmMain);
                _frmMain.panBody.Controls.Add(_pageEnter_User);
                _pageEnter_User.Dock = DockStyle.Fill;
                _pageEnter_User.BringToFront();
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
            pageEnter_User _pageEnter_User = new pageEnter_User(_frmMain);
            _frmMain.panBody.Controls.Add(_pageEnter_User);
            _pageEnter_User.Dock = DockStyle.Fill;
            _pageEnter_User.BringToFront();
        }
    }
}
