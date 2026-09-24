using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdataManagement.Views
{
    /// <summary>
    /// Single button modal for errors and blocking warnings, used instead of MessageBox.
    ///
    ///     frmError.ShowError(this, "Employee code S12345 was not found.");
    /// </summary>
    public partial class frmError : Form
    {
        public frmError()
        {
            InitializeComponent();
        }

        public static void ShowError(IWin32Window owner, string message)
        {
            using (frmError dialog = new frmError())
            {
                dialog.Setup(message);
                dialog.ShowDialog(owner);
            }
        }

        private void Setup(string message)
        {
            StartPosition = FormStartPosition.CenterParent;

            btnSave.DialogResult = DialogResult.OK;
            AcceptButton = btnSave;
            CancelButton = btnSave;     // Esc closes it too

            // same reason as frmConfirm: the designer label is AutoSize
            lbErrorModal.AutoSize = false;
            lbErrorModal.TextAlign = ContentAlignment.MiddleCenter;
            lbErrorModal.Bounds = new Rectangle(24, 150, ClientSize.Width - 48, 64);
            lbErrorModal.Text = message ?? "";

            frmConfirm.GrowForText(this, lbErrorModal, new Control[] { btnSave });

            Paint += Modal_Paint;
        }

        private void Modal_Paint(object sender, PaintEventArgs e)
        {
            frmConfirm.PaintBorder(this, e);
        }
    }
}
