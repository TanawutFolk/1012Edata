namespace EdataManagement.Views.Setting
{
    partial class frmSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSetting));
            this.label4 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnSettingLogin = new Bunifu.Framework.UI.BunifuFlatButton();
            this.btnSettingEmail = new Bunifu.Framework.UI.BunifuFlatButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbChangeForbtn = new System.Windows.Forms.Label();
            this.picChoose = new System.Windows.Forms.PictureBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnCancel = new Bunifu.Framework.UI.BunifuFlatButton();
            this.btnSave = new Bunifu.Framework.UI.BunifuFlatButton();
            this.panelUCSetting = new System.Windows.Forms.Panel();
            this.cbDatabase = new System.Windows.Forms.ComboBox();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picChoose)).BeginInit();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(859, 17);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(221, 24);
            this.label4.TabIndex = 3;
            this.label4.Text = "1012 Send E-data Setting";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.cbDatabase);
            this.panel2.Controls.Add(this.btnSettingLogin);
            this.panel2.Controls.Add(this.btnSettingEmail);
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Controls.Add(this.lbChangeForbtn);
            this.panel2.Controls.Add(this.picChoose);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1096, 110);
            this.panel2.TabIndex = 10;
            // 
            // btnSettingLogin
            // 
            this.btnSettingLogin.Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSettingLogin.BackColor = System.Drawing.Color.White;
            this.btnSettingLogin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSettingLogin.BorderRadius = 0;
            this.btnSettingLogin.ButtonText = "Login Permission";
            this.btnSettingLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettingLogin.DisabledColor = System.Drawing.Color.Gray;
            this.btnSettingLogin.Iconcolor = System.Drawing.Color.Transparent;
            this.btnSettingLogin.Iconimage = ((System.Drawing.Image)(resources.GetObject("btnSettingLogin.Iconimage")));
            this.btnSettingLogin.Iconimage_right = null;
            this.btnSettingLogin.Iconimage_right_Selected = null;
            this.btnSettingLogin.Iconimage_Selected = null;
            this.btnSettingLogin.IconMarginLeft = 0;
            this.btnSettingLogin.IconMarginRight = 0;
            this.btnSettingLogin.IconRightVisible = true;
            this.btnSettingLogin.IconRightZoom = 0D;
            this.btnSettingLogin.IconVisible = true;
            this.btnSettingLogin.IconZoom = 65D;
            this.btnSettingLogin.IsTab = false;
            this.btnSettingLogin.Location = new System.Drawing.Point(179, 60);
            this.btnSettingLogin.Name = "btnSettingLogin";
            this.btnSettingLogin.Normalcolor = System.Drawing.Color.White;
            this.btnSettingLogin.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSettingLogin.OnHoverTextColor = System.Drawing.Color.White;
            this.btnSettingLogin.selected = false;
            this.btnSettingLogin.Size = new System.Drawing.Size(172, 48);
            this.btnSettingLogin.TabIndex = 14;
            this.btnSettingLogin.Text = "Login Permission";
            this.btnSettingLogin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSettingLogin.Textcolor = System.Drawing.Color.Black;
            this.btnSettingLogin.TextFont = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettingLogin.Click += new System.EventHandler(this.btnSettingLogin_Click);
            // 
            // btnSettingEmail
            // 
            this.btnSettingEmail.Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSettingEmail.BackColor = System.Drawing.Color.White;
            this.btnSettingEmail.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSettingEmail.BorderRadius = 0;
            this.btnSettingEmail.ButtonText = "Email Sending";
            this.btnSettingEmail.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettingEmail.DisabledColor = System.Drawing.Color.Gray;
            this.btnSettingEmail.Iconcolor = System.Drawing.Color.Transparent;
            this.btnSettingEmail.Iconimage = ((System.Drawing.Image)(resources.GetObject("btnSettingEmail.Iconimage")));
            this.btnSettingEmail.Iconimage_right = null;
            this.btnSettingEmail.Iconimage_right_Selected = null;
            this.btnSettingEmail.Iconimage_Selected = null;
            this.btnSettingEmail.IconMarginLeft = 0;
            this.btnSettingEmail.IconMarginRight = 0;
            this.btnSettingEmail.IconRightVisible = true;
            this.btnSettingEmail.IconRightZoom = 0D;
            this.btnSettingEmail.IconVisible = true;
            this.btnSettingEmail.IconZoom = 90D;
            this.btnSettingEmail.IsTab = false;
            this.btnSettingEmail.Location = new System.Drawing.Point(5, 60);
            this.btnSettingEmail.Name = "btnSettingEmail";
            this.btnSettingEmail.Normalcolor = System.Drawing.Color.White;
            this.btnSettingEmail.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSettingEmail.OnHoverTextColor = System.Drawing.Color.White;
            this.btnSettingEmail.selected = false;
            this.btnSettingEmail.Size = new System.Drawing.Size(168, 48);
            this.btnSettingEmail.TabIndex = 13;
            this.btnSettingEmail.Text = "Email Sending";
            this.btnSettingEmail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSettingEmail.Textcolor = System.Drawing.Color.Black;
            this.btnSettingEmail.TextFont = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettingEmail.Click += new System.EventHandler(this.btnSettingEmail_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightGray;
            this.panel1.Location = new System.Drawing.Point(2, 55);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1093, 2);
            this.panel1.TabIndex = 6;
            // 
            // lbChangeForbtn
            // 
            this.lbChangeForbtn.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbChangeForbtn.AutoSize = true;
            this.lbChangeForbtn.BackColor = System.Drawing.Color.Transparent;
            this.lbChangeForbtn.Font = new System.Drawing.Font("Century Gothic", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbChangeForbtn.Location = new System.Drawing.Point(51, 17);
            this.lbChangeForbtn.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbChangeForbtn.Name = "lbChangeForbtn";
            this.lbChangeForbtn.Size = new System.Drawing.Size(230, 25);
            this.lbChangeForbtn.TabIndex = 5;
            this.lbChangeForbtn.Text = "Email Sending Setting";
            // 
            // picChoose
            // 
            this.picChoose.Location = new System.Drawing.Point(12, 12);
            this.picChoose.Name = "picChoose";
            this.picChoose.Size = new System.Drawing.Size(34, 33);
            this.picChoose.TabIndex = 4;
            this.picChoose.TabStop = false;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.Controls.Add(this.btnCancel);
            this.panel4.Controls.Add(this.btnSave);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 635);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1096, 59);
            this.panel4.TabIndex = 17;
            // 
            // btnCancel
            // 
            this.btnCancel.Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.BorderRadius = 0;
            this.btnCancel.ButtonText = "Cancel";
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DisabledColor = System.Drawing.Color.Gray;
            this.btnCancel.Iconcolor = System.Drawing.Color.Transparent;
            this.btnCancel.Iconimage = null;
            this.btnCancel.Iconimage_right = null;
            this.btnCancel.Iconimage_right_Selected = null;
            this.btnCancel.Iconimage_Selected = null;
            this.btnCancel.IconMarginLeft = 0;
            this.btnCancel.IconMarginRight = 0;
            this.btnCancel.IconRightVisible = true;
            this.btnCancel.IconRightZoom = 0D;
            this.btnCancel.IconVisible = true;
            this.btnCancel.IconZoom = 90D;
            this.btnCancel.IsTab = false;
            this.btnCancel.Location = new System.Drawing.Point(794, 14);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Normalcolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnCancel.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnCancel.OnHoverTextColor = System.Drawing.Color.White;
            this.btnCancel.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnCancel.selected = false;
            this.btnCancel.Size = new System.Drawing.Size(132, 37);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnCancel.Textcolor = System.Drawing.Color.White;
            this.btnCancel.TextFont = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.BorderRadius = 0;
            this.btnSave.ButtonText = "Save";
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.DisabledColor = System.Drawing.Color.Gray;
            this.btnSave.Iconcolor = System.Drawing.Color.Transparent;
            this.btnSave.Iconimage = null;
            this.btnSave.Iconimage_right = null;
            this.btnSave.Iconimage_right_Selected = null;
            this.btnSave.Iconimage_Selected = null;
            this.btnSave.IconMarginLeft = 0;
            this.btnSave.IconMarginRight = 0;
            this.btnSave.IconRightVisible = true;
            this.btnSave.IconRightZoom = 0D;
            this.btnSave.IconVisible = true;
            this.btnSave.IconZoom = 90D;
            this.btnSave.IsTab = false;
            this.btnSave.Location = new System.Drawing.Point(948, 14);
            this.btnSave.Name = "btnSave";
            this.btnSave.Normalcolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnSave.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnSave.OnHoverTextColor = System.Drawing.Color.White;
            this.btnSave.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnSave.selected = false;
            this.btnSave.Size = new System.Drawing.Size(132, 37);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Save";
            this.btnSave.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnSave.Textcolor = System.Drawing.Color.White;
            this.btnSave.TextFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // panelUCSetting
            // 
            this.panelUCSetting.Location = new System.Drawing.Point(5, 116);
            this.panelUCSetting.Name = "panelUCSetting";
            this.panelUCSetting.Size = new System.Drawing.Size(1090, 515);
            this.panelUCSetting.TabIndex = 6;
            // 
            // cbDatabase
            // 
            this.cbDatabase.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbDatabase.FormattingEnabled = true;
            this.cbDatabase.Location = new System.Drawing.Point(678, 14);
            this.cbDatabase.Name = "cbDatabase";
            this.cbDatabase.Size = new System.Drawing.Size(176, 30);
            this.cbDatabase.TabIndex = 24;
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(240)))), ((int)(((byte)(243)))));
            this.ClientSize = new System.Drawing.Size(1096, 694);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panelUCSetting);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSetting";
            this.Text = "Setting - 1012 Send E-data";
            this.Load += new System.EventHandler(this.frmSetting_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picChoose)).EndInit();
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox picChoose;
        private System.Windows.Forms.Label lbChangeForbtn;
        private System.Windows.Forms.Panel panel1;
        private Bunifu.Framework.UI.BunifuFlatButton btnSettingEmail;
        private Bunifu.Framework.UI.BunifuFlatButton btnSettingLogin;
        private System.Windows.Forms.Panel panel4;
        private Bunifu.Framework.UI.BunifuFlatButton btnCancel;
        private Bunifu.Framework.UI.BunifuFlatButton btnSave;
        private System.Windows.Forms.Panel panelUCSetting;
        private System.Windows.Forms.ComboBox cbDatabase;
    }
}