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
    public partial class frmSuccess : Form
    {
        int _fixTimeClose;
        int _sec = 0;

        public frmSuccess(int _delaySec)
        {
            InitializeComponent();
            _fixTimeClose = _delaySec;
        }

        private void timerSuccess_Tick(object sender, EventArgs e)
        {
            _sec += 1;
            if (_sec == _fixTimeClose)
            {
                this.timerSuccess.Enabled = false;
                this.Close();
            }
        }

        private void frmSuccess_Load(object sender, EventArgs e)
        {
            this.timerSuccess.Enabled = true;
        }
    }
}
