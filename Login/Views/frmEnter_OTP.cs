using Login.Property;
using Login.Controllors;

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
    public partial class frmEnter_OTP : Form
    {
        LoginControllors _controllors = new LoginControllors();
        EmpProperty emp;

        public frmEnter_OTP(EmpProperty empIN)
        {
            InitializeComponent();
            emp = empIN;
            this.DialogResult = DialogResult.None;
        }

        private void bttConfirm_Click(object sender, EventArgs e)
        {
            //OTP 6 Digit
            string d1 = this.txtOTP_D1.Text;
            string d2 = this.txtOTP_D2.Text;
            string d3 = this.txtOTP_D3.Text;
            string d4 = this.txtOTP_D4.Text;
            string d5 = this.txtOTP_D5.Text;
            string d6 = this.txtOTP_D6.Text;

            string OTP = d1 + d2 + d3 + d4 + d5 + d6;

            if (OTP.Length != 6)
            {
                MessageBox.Show("Please enter the complete OTP number.", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                ClearInputAll();
                return;
            }

            if (_controllors.EnterOTP(emp, OTP))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            ClearInputAll();
        }

        private void ClearInputAll()
        {
            this.txtOTP_D1.Text = "";
            this.txtOTP_D2.Text = "";
            this.txtOTP_D3.Text = "";
            this.txtOTP_D4.Text = "";
            this.txtOTP_D5.Text = "";
            this.txtOTP_D6.Text = "";
        }

        private void picClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblForgotPass_Click(object sender, EventArgs e)
        {
            if (_controllors.SendOTP(emp))
            {
                frmSuccess _succ = new frmSuccess(1);
                _succ.ShowDialog();
            }
        }

        private void bttBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtOTP_D1_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) || (e.KeyCode == Keys.Back))
            {
                return;
            }
            else
            {
                this.txtOTP_D2.Focus();
            }
        }

        private void txtOTP_D2_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) || (e.KeyCode == Keys.Back))
            {
                return;
            }
            else
            {
                this.txtOTP_D3.Focus();
            }
        }

        private void txtOTP_D3_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) || (e.KeyCode == Keys.Back))
            {
                return;
            }
            else
            {
                this.txtOTP_D4.Focus();
            }
        }

        private void txtOTP_D4_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) || (e.KeyCode == Keys.Back))
            {
                return;
            }
            else
            {
                this.txtOTP_D5.Focus();
            }
        }

        private void txtOTP_D5_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) || (e.KeyCode == Keys.Back))
            {
                return;
            }
            else
            {
                this.txtOTP_D6.Focus();
            }
        }


    }
}
