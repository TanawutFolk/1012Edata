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
            this.btnSave = new System.Windows.Forms.Button();
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
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(102, 229);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(240, 27);
            this.btnSave.TabIndex = 26;
            this.btnSave.Text = "Ok";
            this.btnSave.UseCompatibleTextRendering = true;
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // frmError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 279);
            this.Controls.Add(this.picErrorModal);
            this.Controls.Add(this.lbErrorModal);
            this.Controls.Add(this.btnSave);
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
        private System.Windows.Forms.Button btnSave;
    }
}