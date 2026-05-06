using Login.Property;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Login
{
    public partial class frmLogin_Main : Form
    {
        public LoginProperty _resultLogin = new LoginProperty();

        public frmLogin_Main()
        {
            InitializeComponent();
        }

        private void frmLogin_Main_Load(object sender, EventArgs e)
        {
            //Clear panel Body.
            this.panBody.Controls.Clear();

            //Load page search.
            pageEnter_User _pageEnter_User = new pageEnter_User(this);
            this.panBody.Controls.Add(_pageEnter_User);
            _pageEnter_User.Dock = DockStyle.Fill;
            _pageEnter_User.BringToFront();
        }

        private void panBody_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
