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
    /// Yes / no modal used instead of MessageBox.
    ///
    ///     if (frmConfirm.Ask(this, "Remove this address?", "Remove", "Cancel")) { ... }
    /// </summary>
    public partial class frmConfirm : Form
    {
        private static readonly Color BORDER_COLOR = Color.FromArgb(209, 213, 219);

        public frmConfirm()
        {
            InitializeComponent();
        }

        /// <summary>Ask with the default button captions (OK / Cancel).</summary>
        public static bool Ask(IWin32Window owner, string message)
        {
            return Ask(owner, message, "OK", "Cancel");
        }

        /// <summary>Returns true when the left (confirm) button was pressed.</summary>
        public static bool Ask(IWin32Window owner, string message, string okText, string cancelText)
        {
            using (frmConfirm dialog = new frmConfirm())
            {
                dialog.Setup(message, okText, cancelText);
                return dialog.ShowDialog(owner) == DialogResult.Yes;
            }
        }

        private void Setup(string message, string okText, string cancelText)
        {
            StartPosition = FormStartPosition.CenterParent;

            btnSave.Text = okText;
            btnSave.DialogResult = DialogResult.Yes;
            btnCancle.Text = cancelText;
            btnCancle.DialogResult = DialogResult.No;

            AcceptButton = btnSave;
            CancelButton = btnCancle;   // Esc closes the borderless form

            // the label is AutoSize in the designer, so a long message would run off
            // the form - give it a fixed box that wraps and grow the form if needed
            lbConfirmModal.AutoSize = false;
            lbConfirmModal.TextAlign = ContentAlignment.MiddleCenter;
            lbConfirmModal.Bounds = new Rectangle(24, 155, ClientSize.Width - 48, 64);
            lbConfirmModal.Text = message ?? "";

            GrowForText(this, lbConfirmModal, new Control[] { btnSave, btnCancle });

            Paint += Modal_Paint;
        }

        /// <summary>Make room when the message needs more than one or two lines.</summary>
        internal static void GrowForText(Form form, Label label, Control[] below)
        {
            Size needed = TextRenderer.MeasureText(
                label.Text,
                label.Font,
                new Size(label.Width, 0),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);

            int extra = needed.Height - label.Height;

            if (extra <= 0) return;

            label.Height += extra;

            foreach (Control control in below)
            {
                control.Top += extra;
            }

            form.Height += extra;
        }

        /// <summary>A borderless form needs its own outline, otherwise it bleeds into the screen.</summary>
        internal static void PaintBorder(Form form, PaintEventArgs e)
        {
            using (Pen pen = new Pen(BORDER_COLOR))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, form.ClientSize.Width - 1, form.ClientSize.Height - 1);
            }
        }

        private void Modal_Paint(object sender, PaintEventArgs e)
        {
            PaintBorder(this, e);
        }
    }
}
