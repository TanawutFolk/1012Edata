namespace EdataManagement.Views
{
    partial class frmError
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmError));
            this.picErrorModal = new System.Windows.Forms.PictureBox();
            this.lbErrorModal = new System.Windows.Forms.Label();
            this.btnSavee = new Bunifu.Framework.UI.BunifuFlatButton();
            ((System.ComponentModel.ISupportInitialize)(this.picErrorModal)).BeginInit();
            this.SuspendLayout();
            // 
            // picErrorModal
            // 
            this.picErrorModal.ErrorImage = ((System.Drawing.Image)(resources.GetObject("picErrorModal.ErrorImage")));
            this.picErrorModal.Image = ((System.Drawing.Image)(resources.GetObject("picErrorModal.Image")));
            this.picErrorModal.Location = new System.Drawing.Point(163, 22);
            this.picErrorModal.Name = "picErrorModal";
            this.picErrorModal.Size = new System.Drawing.Size(115, 115);
            this.picErrorModal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picErrorModal.TabIndex = 28;
            this.picErrorModal.TabStop = false;
            // 
            // lbErrorModal
            // 
            this.lbErrorModal.AutoSize = true;
            this.lbErrorModal.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbErrorModal.Location = new System.Drawing.Point(98, 175);
            this.lbErrorModal.Name = "lbErrorModal";
            this.lbErrorModal.Size = new System.Drawing.Size(60, 24);
            this.lbErrorModal.TabIndex = 27;
            this.lbErrorModal.Text = "label1";
            // 
            // btnSavee
            // 
            this.btnSavee.Activecolor = System.Drawing.Color.Lime;
            this.btnSavee.BackColor = System.Drawing.Color.Red;
            this.btnSavee.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSavee.BorderRadius = 0;
            this.btnSavee.ButtonText = "OK";
            this.btnSavee.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSavee.DisabledColor = System.Drawing.Color.Gray;
            this.btnSavee.Iconcolor = System.Drawing.Color.Transparent;
            this.btnSavee.Iconimage = null;
            this.btnSavee.Iconimage_right = null;
            this.btnSavee.Iconimage_right_Selected = null;
            this.btnSavee.Iconimage_Selected = null;
            this.btnSavee.IconMarginLeft = 0;
            this.btnSavee.IconMarginRight = 0;
            this.btnSavee.IconRightVisible = true;
            this.btnSavee.IconRightZoom = 0D;
            this.btnSavee.IconVisible = true;
            this.btnSavee.IconZoom = 90D;
            this.btnSavee.IsTab = false;
            this.btnSavee.Location = new System.Drawing.Point(102, 230);
            this.btnSavee.Name = "btnSavee";
            this.btnSavee.Normalcolor = System.Drawing.Color.Red;
            this.btnSavee.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnSavee.OnHoverTextColor = System.Drawing.Color.White;
            this.btnSavee.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnSavee.selected = false;
            this.btnSavee.Size = new System.Drawing.Size(250, 37);
            this.btnSavee.TabIndex = 29;
            this.btnSavee.Text = "OK";
            this.btnSavee.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnSavee.Textcolor = System.Drawing.Color.White;
            this.btnSavee.TextFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // frmError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 279);
            this.Controls.Add(this.btnSavee);
            this.Controls.Add(this.picErrorModal);
            this.Controls.Add(this.lbErrorModal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Error";
            ((System.ComponentModel.ISupportInitialize)(this.picErrorModal)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picErrorModal;
        private System.Windows.Forms.Label lbErrorModal;
        private Bunifu.Framework.UI.BunifuFlatButton btnSavee;
    }
}