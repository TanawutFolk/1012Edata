using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace EdataManagement.Views
{
    public partial class frmEmail : Form
    {
        public void SetDataSource(object dataSource)
        {
            dataGridViewEmail.DataSource = dataSource;
        }

        public frmEmail()
        {
            InitializeComponent();

        }

        private void bt_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
