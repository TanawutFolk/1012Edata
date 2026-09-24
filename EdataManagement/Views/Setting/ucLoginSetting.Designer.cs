namespace EdataManagement.Views.Setting
{
    partial class ucLoginSetting
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbPermissionTitle = new System.Windows.Forms.Label();
            this.lbPermissionInfo = new System.Windows.Forms.Label();
            this.pnLoginCard = new System.Windows.Forms.Panel();
            this.btnAddUser = new Bunifu.Framework.UI.BunifuFlatButton();
            this.cbRole = new System.Windows.Forms.ComboBox();
            this.lbRole = new System.Windows.Forms.Label();
            this.txtAddEmpCode = new Bunifu.Framework.UI.BunifuMetroTextbox();
            this.lbAddEmpCode = new System.Windows.Forms.Label();
            this.dtgPermission = new Bunifu.Framework.UI.BunifuCustomDataGrid();
            this.pnLoginCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtgPermission)).BeginInit();
            this.SuspendLayout();
            //
            // lbPermissionTitle
            //
            this.lbPermissionTitle.AutoSize = true;
            this.lbPermissionTitle.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPermissionTitle.Location = new System.Drawing.Point(19, 10);
            this.lbPermissionTitle.Name = "lbPermissionTitle";
            this.lbPermissionTitle.Size = new System.Drawing.Size(245, 21);
            this.lbPermissionTitle.TabIndex = 0;
            this.lbPermissionTitle.Text = "Users allowed to open Setting";
            //
            // lbPermissionInfo
            //
            this.lbPermissionInfo.AutoSize = true;
            this.lbPermissionInfo.Font = new System.Drawing.Font("Century Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPermissionInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbPermissionInfo.Location = new System.Drawing.Point(20, 38);
            this.lbPermissionInfo.Name = "lbPermissionInfo";
            this.lbPermissionInfo.Size = new System.Drawing.Size(0, 16);
            this.lbPermissionInfo.TabIndex = 1;
            //
            // pnLoginCard
            //
            this.pnLoginCard.BackColor = System.Drawing.Color.White;
            this.pnLoginCard.Controls.Add(this.btnAddUser);
            this.pnLoginCard.Controls.Add(this.cbRole);
            this.pnLoginCard.Controls.Add(this.lbRole);
            this.pnLoginCard.Controls.Add(this.txtAddEmpCode);
            this.pnLoginCard.Controls.Add(this.lbAddEmpCode);
            this.pnLoginCard.Controls.Add(this.dtgPermission);
            this.pnLoginCard.Location = new System.Drawing.Point(16, 67);
            this.pnLoginCard.Name = "pnLoginCard";
            this.pnLoginCard.Size = new System.Drawing.Size(1059, 437);
            this.pnLoginCard.TabIndex = 2;
            //
            // btnAddUser
            //
            this.btnAddUser.Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAddUser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAddUser.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddUser.BorderRadius = 0;
            this.btnAddUser.ButtonText = "Add User";
            this.btnAddUser.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddUser.DisabledColor = System.Drawing.Color.Gray;
            this.btnAddUser.Iconcolor = System.Drawing.Color.Transparent;
            this.btnAddUser.Iconimage = null;
            this.btnAddUser.Iconimage_right = null;
            this.btnAddUser.Iconimage_right_Selected = null;
            this.btnAddUser.Iconimage_Selected = null;
            this.btnAddUser.IconMarginLeft = 0;
            this.btnAddUser.IconMarginRight = 0;
            this.btnAddUser.IconRightVisible = true;
            this.btnAddUser.IconRightZoom = 0D;
            this.btnAddUser.IconVisible = true;
            this.btnAddUser.IconZoom = 90D;
            this.btnAddUser.IsTab = false;
            this.btnAddUser.Location = new System.Drawing.Point(907, 390);
            this.btnAddUser.Name = "btnAddUser";
            this.btnAddUser.Normalcolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAddUser.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAddUser.OnHoverTextColor = System.Drawing.Color.White;
            this.btnAddUser.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnAddUser.selected = false;
            this.btnAddUser.Size = new System.Drawing.Size(132, 37);
            this.btnAddUser.TabIndex = 5;
            this.btnAddUser.Text = "Add User";
            this.btnAddUser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnAddUser.Textcolor = System.Drawing.Color.White;
            this.btnAddUser.TextFont = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //
            // cbRole
            //
            this.cbRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRole.Font = new System.Drawing.Font("Century Gothic", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbRole.FormattingEnabled = true;
            this.cbRole.Location = new System.Drawing.Point(611, 393);
            this.cbRole.Name = "cbRole";
            this.cbRole.Size = new System.Drawing.Size(178, 32);
            this.cbRole.TabIndex = 4;
            //
            // lbRole
            //
            this.lbRole.AutoSize = true;
            this.lbRole.Font = new System.Drawing.Font("Century Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbRole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbRole.Location = new System.Drawing.Point(608, 373);
            this.lbRole.Name = "lbRole";
            this.lbRole.Size = new System.Drawing.Size(33, 16);
            this.lbRole.TabIndex = 6;
            this.lbRole.Text = "Role";
            //
            // txtAddEmpCode
            //
            this.txtAddEmpCode.BorderColorFocused = System.Drawing.Color.Blue;
            this.txtAddEmpCode.BorderColorIdle = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtAddEmpCode.BorderColorMouseHover = System.Drawing.Color.Blue;
            this.txtAddEmpCode.BorderThickness = 3;
            this.txtAddEmpCode.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtAddEmpCode.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAddEmpCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtAddEmpCode.isPassword = false;
            this.txtAddEmpCode.Location = new System.Drawing.Point(21, 386);
            this.txtAddEmpCode.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.txtAddEmpCode.Name = "txtAddEmpCode";
            this.txtAddEmpCode.Size = new System.Drawing.Size(582, 41);
            this.txtAddEmpCode.TabIndex = 3;
            this.txtAddEmpCode.Text = "";
            this.txtAddEmpCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            //
            // lbAddEmpCode
            //
            this.lbAddEmpCode.AutoSize = true;
            this.lbAddEmpCode.Font = new System.Drawing.Font("Century Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAddEmpCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbAddEmpCode.Location = new System.Drawing.Point(18, 366);
            this.lbAddEmpCode.Name = "lbAddEmpCode";
            this.lbAddEmpCode.Size = new System.Drawing.Size(200, 16);
            this.lbAddEmpCode.TabIndex = 7;
            this.lbAddEmpCode.Text = "Employee code (e.g. S00616)";
            //
            // dtgPermission
            //
            this.dtgPermission.BackgroundColor = System.Drawing.Color.White;
            this.dtgPermission.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dtgPermission.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dtgPermission.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtgPermission.DoubleBuffered = true;
            this.dtgPermission.EnableHeadersVisualStyles = false;
            this.dtgPermission.HeaderBgColor = System.Drawing.Color.SeaGreen;
            this.dtgPermission.HeaderForeColor = System.Drawing.Color.White;
            this.dtgPermission.Location = new System.Drawing.Point(21, 15);
            this.dtgPermission.Name = "dtgPermission";
            this.dtgPermission.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dtgPermission.Size = new System.Drawing.Size(1018, 340);
            this.dtgPermission.TabIndex = 2;
            //
            // ucLoginSetting
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnLoginCard);
            this.Controls.Add(this.lbPermissionInfo);
            this.Controls.Add(this.lbPermissionTitle);
            this.Name = "ucLoginSetting";
            this.Size = new System.Drawing.Size(1090, 515);
            this.pnLoginCard.ResumeLayout(false);
            this.pnLoginCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtgPermission)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbPermissionTitle;
        private System.Windows.Forms.Label lbPermissionInfo;
        private System.Windows.Forms.Panel pnLoginCard;
        private Bunifu.Framework.UI.BunifuCustomDataGrid dtgPermission;
        private Bunifu.Framework.UI.BunifuMetroTextbox txtAddEmpCode;
        private System.Windows.Forms.Label lbAddEmpCode;
        private System.Windows.Forms.ComboBox cbRole;
        private System.Windows.Forms.Label lbRole;
        private Bunifu.Framework.UI.BunifuFlatButton btnAddUser;
    }
}
