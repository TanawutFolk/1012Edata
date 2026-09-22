namespace EdataManagement.Views
{
    partial class frmConfirm
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
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancle = new System.Windows.Forms.Button();
            this.lbConfirmModal = new System.Windows.Forms.Label();
            this.picConfirmModal = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picConfirmModal)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(30, 235);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(240, 27);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Save";
            this.btnSave.UseCompatibleTextRendering = true;
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // btnCancle
            // 
            this.btnCancle.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancle.Location = new System.Drawing.Point(290, 235);
            this.btnCancle.Name = "btnCancle";
            this.btnCancle.Size = new System.Drawing.Size(128, 27);
            this.btnCancle.TabIndex = 21;
            this.btnCancle.Text = "Cancle";
            this.btnCancle.UseCompatibleTextRendering = true;
            this.btnCancle.UseVisualStyleBackColor = true;
            // 
            // lbConfirmModal
            // 
            this.lbConfirmModal.AutoSize = true;
            this.lbConfirmModal.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbConfirmModal.Location = new System.Drawing.Point(56, 178);
            this.lbConfirmModal.Name = "lbConfirmModal";
            this.lbConfirmModal.Size = new System.Drawing.Size(60, 24);
            this.lbConfirmModal.TabIndex = 23;
            this.lbConfirmModal.Text = "label1";
            // 
            // picConfirmModal
            // 
            this.picConfirmModal.Location = new System.Drawing.Point(171, 26);
            this.picConfirmModal.Name = "picConfirmModal";
            this.picConfirmModal.Size = new System.Drawing.Size(115, 115);
            this.picConfirmModal.TabIndex = 24;
            this.picConfirmModal.TabStop = false;
            // 
            // frmConfirm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 279);
            this.Controls.Add(this.picConfirmModal);
            this.Controls.Add(this.lbConfirmModal);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimizeBox = false;
            this.Name = "frmConfirm";
            this.Text = "frmConfirm";
            ((System.ComponentModel.ISupportInitialize)(this.picConfirmModal)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancle;
        private System.Windows.Forms.Label lbConfirmModal;
        private System.Windows.Forms.PictureBox picConfirmModal;
    }
}