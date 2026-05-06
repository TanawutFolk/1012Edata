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
    public partial class frmChangePass_Main : Form
    {
        public EmpProperty _empDetial;
        public bool con;

        public frmChangePass_Main(EmpProperty empIN)
        {
            InitializeComponent();
            _empDetial = empIN;
            this.DialogResult = DialogResult.None;
        }

        private void frmChangePass_Main_Load(object sender, EventArgs e)
        {
            //Clear panel Body.
            this.panBody.Controls.Clear();

            //Load page search.
            pageBefore_ChangPassword _pageBefore_ChangPassword = new pageBefore_ChangPassword(this);
            this.panBody.Controls.Add(_pageBefore_ChangPassword);
            _pageBefore_ChangPassword.Dock = DockStyle.Fill;
            _pageBefore_ChangPassword.BringToFront();
        }


    }
}
