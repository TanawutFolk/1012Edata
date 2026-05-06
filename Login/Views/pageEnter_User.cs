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
    public partial class pageEnter_User : UserControl
    {
        LoginControllors _controllors = new LoginControllors();
        EmpProperty _empProperty;
        frmLogin_Main _frmMain;

        public pageEnter_User(frmLogin_Main obj)
        {
            InitializeComponent();
            _frmMain = obj;
        }

        private void bttNext_Click(object sender, EventArgs e)
        {
            if (this.txtEmpID.Text.Trim() == "") { return; }
            _empProperty = new EmpProperty{ code = this.txtEmpID.Text.Trim() };

            if (_controllors.CheckEmpCompanyExist(_empProperty) == true)
            {_empProperty = _controllors.GetEmpProperty(_empProperty); }
            else { this.txtEmpID.Text = ""; return;}

            _frmMain._resultLogin.empDetial = _empProperty;
            if (_controllors.CheckEmpFitelGroupExist(_empProperty) == true)
            {
                //Clear panel Body.
                _frmMain.panBody.Controls.Clear();

                //Load page search.
                pageEnter_Password _pageEnter_User = new pageEnter_Password(_frmMain);
                _frmMain.panBody.Controls.Add(_pageEnter_User);
                _pageEnter_User.Dock = DockStyle.Fill;
                _pageEnter_User.BringToFront();
            }
            else
            {
                //Clear panel Body.
                _frmMain.panBody.Controls.Clear();

                //Load page search.
                pageEnter_Create _pageEnter_User = new pageEnter_Create(_frmMain);
                _frmMain.panBody.Controls.Add(_pageEnter_User);
                _pageEnter_User.Dock = DockStyle.Fill;
                _pageEnter_User.BringToFront();
            }
        }

        private void txtEmpID_TextChanged(object sender, EventArgs e)
        {
            this.lblLimit_User.Text = this.txtEmpID.Text.Length + "/10";
        }

        private void pageEnter_User_Load(object sender, EventArgs e)
        {
            _frmMain._resultLogin.empDetial = new EmpProperty();
        }
    }
}
