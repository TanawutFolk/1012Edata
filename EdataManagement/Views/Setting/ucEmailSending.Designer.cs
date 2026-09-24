namespace EdataManagement.Views.Setting
{
    partial class ucEmailSending
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucEmailSending));
            this.cbProduct = new System.Windows.Forms.ComboBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.pnAlert = new System.Windows.Forms.Panel();
            this.btnAlertAction = new Bunifu.Framework.UI.BunifuFlatButton();
            this.lbAlertText = new System.Windows.Forms.Label();
            this.lbAlertIcon = new System.Windows.Forms.Label();
            this.lbForGroupID = new System.Windows.Forms.Label();
            this.btnAddToGroup = new Bunifu.Framework.UI.BunifuFlatButton();
            this.cbToOrCC = new System.Windows.Forms.ComboBox();
            this.txtAddEmail = new Bunifu.Framework.UI.BunifuMetroTextbox();
            this.dtgShowEmailAll = new Bunifu.Framework.UI.BunifuCustomDataGrid();
            this.lbChangeForReceiver = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbCustomer = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAllGroupDetails = new Bunifu.Framework.UI.BunifuFlatButton();
            this.label2 = new System.Windows.Forms.Label();
            this.cbEdataType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel3.SuspendLayout();
            this.pnAlert.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtgShowEmailAll)).BeginInit();
            this.SuspendLayout();
            // 
            // cbProduct
            // 
            this.cbProduct.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbProduct.FormattingEnabled = true;
            this.cbProduct.Location = new System.Drawing.Point(16, 31);
            this.cbProduct.Name = "cbProduct";
            this.cbProduct.Size = new System.Drawing.Size(178, 30);
            this.cbProduct.TabIndex = 17;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Controls.Add(this.pnAlert);
            this.panel3.Controls.Add(this.lbForGroupID);
            this.panel3.Controls.Add(this.btnAddToGroup);
            this.panel3.Controls.Add(this.cbToOrCC);
            this.panel3.Controls.Add(this.txtAddEmail);
            this.panel3.Controls.Add(this.dtgShowEmailAll);
            this.panel3.Controls.Add(this.lbChangeForReceiver);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Location = new System.Drawing.Point(16, 67);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1059, 437);
            this.panel3.TabIndex = 22;
            // 
            // pnAlert
            // 
            this.pnAlert.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(243)))), ((int)(((byte)(199)))));
            this.pnAlert.Controls.Add(this.btnAlertAction);
            this.pnAlert.Controls.Add(this.lbAlertText);
            this.pnAlert.Controls.Add(this.lbAlertIcon);
            this.pnAlert.Location = new System.Drawing.Point(21, 46);
            this.pnAlert.Name = "pnAlert";
            this.pnAlert.Size = new System.Drawing.Size(1018, 44);
            this.pnAlert.TabIndex = 23;
            // 
            // btnAlertAction
            // 
            this.btnAlertAction.Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAlertAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAlertAction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAlertAction.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAlertAction.BorderRadius = 0;
            this.btnAlertAction.ButtonText = "Split To New Group";
            this.btnAlertAction.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAlertAction.DisabledColor = System.Drawing.Color.Gray;
            this.btnAlertAction.Iconcolor = System.Drawing.Color.Transparent;
            this.btnAlertAction.Iconimage = null;
            this.btnAlertAction.Iconimage_right = null;
            this.btnAlertAction.Iconimage_right_Selected = null;
            this.btnAlertAction.Iconimage_Selected = null;
            this.btnAlertAction.IconMarginLeft = 0;
            this.btnAlertAction.IconMarginRight = 0;
            this.btnAlertAction.IconRightVisible = true;
            this.btnAlertAction.IconRightZoom = 0D;
            this.btnAlertAction.IconVisible = true;
            this.btnAlertAction.IconZoom = 90D;
            this.btnAlertAction.IsTab = false;
            this.btnAlertAction.Location = new System.Drawing.Point(806, 6);
            this.btnAlertAction.Name = "btnAlertAction";
            this.btnAlertAction.Normalcolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAlertAction.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAlertAction.OnHoverTextColor = System.Drawing.Color.White;
            this.btnAlertAction.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnAlertAction.selected = false;
            this.btnAlertAction.Size = new System.Drawing.Size(200, 32);
            this.btnAlertAction.TabIndex = 2;
            this.btnAlertAction.Text = "Split To New Group";
            this.btnAlertAction.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnAlertAction.Textcolor = System.Drawing.Color.White;
            this.btnAlertAction.TextFont = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlertAction.Visible = false;
            // 
            // lbAlertText
            // 
            this.lbAlertText.AutoEllipsis = true;
            this.lbAlertText.BackColor = System.Drawing.Color.Transparent;
            this.lbAlertText.Font = new System.Drawing.Font("Century Gothic", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAlertText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(64)))), ((int)(((byte)(14)))));
            this.lbAlertText.Location = new System.Drawing.Point(40, 6);
            this.lbAlertText.Name = "lbAlertText";
            this.lbAlertText.Size = new System.Drawing.Size(754, 32);
            this.lbAlertText.TabIndex = 1;
            this.lbAlertText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbAlertIcon
            // 
            this.lbAlertIcon.AutoSize = true;
            this.lbAlertIcon.BackColor = System.Drawing.Color.Transparent;
            this.lbAlertIcon.Font = new System.Drawing.Font("Century Gothic", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAlertIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(64)))), ((int)(((byte)(14)))));
            this.lbAlertIcon.Location = new System.Drawing.Point(12, 9);
            this.lbAlertIcon.Name = "lbAlertIcon";
            this.lbAlertIcon.Size = new System.Drawing.Size(15, 23);
            this.lbAlertIcon.TabIndex = 0;
            this.lbAlertIcon.Text = "!";
            // 
            // lbForGroupID
            // 
            this.lbForGroupID.AutoSize = true;
            this.lbForGroupID.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbForGroupID.Location = new System.Drawing.Point(955, 17);
            this.lbForGroupID.Name = "lbForGroupID";
            this.lbForGroupID.Size = new System.Drawing.Size(81, 21);
            this.lbForGroupID.TabIndex = 22;
            this.lbForGroupID.Text = "Group ID";
            // 
            // btnAddToGroup
            // 
            this.btnAddToGroup.Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAddToGroup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAddToGroup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddToGroup.BorderRadius = 0;
            this.btnAddToGroup.ButtonText = "Add To Group";
            this.btnAddToGroup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddToGroup.DisabledColor = System.Drawing.Color.Gray;
            this.btnAddToGroup.Iconcolor = System.Drawing.Color.Transparent;
            this.btnAddToGroup.Iconimage = null;
            this.btnAddToGroup.Iconimage_right = null;
            this.btnAddToGroup.Iconimage_right_Selected = null;
            this.btnAddToGroup.Iconimage_Selected = null;
            this.btnAddToGroup.IconMarginLeft = 0;
            this.btnAddToGroup.IconMarginRight = 0;
            this.btnAddToGroup.IconRightVisible = true;
            this.btnAddToGroup.IconRightZoom = 0D;
            this.btnAddToGroup.IconVisible = true;
            this.btnAddToGroup.IconZoom = 90D;
            this.btnAddToGroup.IsTab = false;
            this.btnAddToGroup.Location = new System.Drawing.Point(907, 390);
            this.btnAddToGroup.Name = "btnAddToGroup";
            this.btnAddToGroup.Normalcolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAddToGroup.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAddToGroup.OnHoverTextColor = System.Drawing.Color.White;
            this.btnAddToGroup.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnAddToGroup.selected = false;
            this.btnAddToGroup.Size = new System.Drawing.Size(132, 37);
            this.btnAddToGroup.TabIndex = 21;
            this.btnAddToGroup.Text = "Add To Group";
            this.btnAddToGroup.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnAddToGroup.Textcolor = System.Drawing.Color.White;
            this.btnAddToGroup.TextFont = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // cbToOrCC
            // 
            this.cbToOrCC.Font = new System.Drawing.Font("Century Gothic", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbToOrCC.FormattingEnabled = true;
            this.cbToOrCC.Location = new System.Drawing.Point(611, 393);
            this.cbToOrCC.Name = "cbToOrCC";
            this.cbToOrCC.Size = new System.Drawing.Size(178, 32);
            this.cbToOrCC.TabIndex = 17;
            // 
            // txtAddEmail
            // 
            this.txtAddEmail.BorderColorFocused = System.Drawing.Color.Blue;
            this.txtAddEmail.BorderColorIdle = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtAddEmail.BorderColorMouseHover = System.Drawing.Color.Blue;
            this.txtAddEmail.BorderThickness = 3;
            this.txtAddEmail.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtAddEmail.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAddEmail.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtAddEmail.isPassword = false;
            this.txtAddEmail.Location = new System.Drawing.Point(21, 386);
            this.txtAddEmail.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.txtAddEmail.Name = "txtAddEmail";
            this.txtAddEmail.Size = new System.Drawing.Size(582, 41);
            this.txtAddEmail.TabIndex = 20;
            this.txtAddEmail.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            // 
            // dtgShowEmailAll
            // 
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.dtgShowEmailAll.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dtgShowEmailAll.BackgroundColor = System.Drawing.Color.White;
            this.dtgShowEmailAll.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dtgShowEmailAll.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.SeaGreen;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Century Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dtgShowEmailAll.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dtgShowEmailAll.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtgShowEmailAll.DoubleBuffered = true;
            this.dtgShowEmailAll.EnableHeadersVisualStyles = false;
            this.dtgShowEmailAll.HeaderBgColor = System.Drawing.Color.SeaGreen;
            this.dtgShowEmailAll.HeaderForeColor = System.Drawing.Color.White;
            this.dtgShowEmailAll.Location = new System.Drawing.Point(21, 98);
            this.dtgShowEmailAll.Name = "dtgShowEmailAll";
            this.dtgShowEmailAll.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dtgShowEmailAll.Size = new System.Drawing.Size(1018, 280);
            this.dtgShowEmailAll.TabIndex = 19;
            // 
            // lbChangeForReceiver
            // 
            this.lbChangeForReceiver.AutoSize = true;
            this.lbChangeForReceiver.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbChangeForReceiver.Location = new System.Drawing.Point(125, 15);
            this.lbChangeForReceiver.Name = "lbChangeForReceiver";
            this.lbChangeForReceiver.Size = new System.Drawing.Size(15, 21);
            this.lbChangeForReceiver.TabIndex = 18;
            this.lbChangeForReceiver.Text = "-";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(17, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 21);
            this.label3.TabIndex = 17;
            this.label3.Text = "Email recipients of";
            // 
            // cbCustomer
            // 
            this.cbCustomer.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbCustomer.FormattingEnabled = true;
            this.cbCustomer.Location = new System.Drawing.Point(223, 31);
            this.cbCustomer.Name = "cbCustomer";
            this.cbCustomer.Size = new System.Drawing.Size(320, 30);
            this.cbCustomer.TabIndex = 18;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(19, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 21);
            this.label1.TabIndex = 19;
            this.label1.Text = "Product";
            // 
            // btnAllGroupDetails
            // 
            this.btnAllGroupDetails.Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAllGroupDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAllGroupDetails.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAllGroupDetails.BorderRadius = 0;
            this.btnAllGroupDetails.ButtonText = "All Group Details";
            this.btnAllGroupDetails.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAllGroupDetails.DisabledColor = System.Drawing.Color.Gray;
            this.btnAllGroupDetails.Iconcolor = System.Drawing.Color.Transparent;
            this.btnAllGroupDetails.Iconimage = ((System.Drawing.Image)(resources.GetObject("btnAllGroupDetails.Iconimage")));
            this.btnAllGroupDetails.Iconimage_right = null;
            this.btnAllGroupDetails.Iconimage_right_Selected = null;
            this.btnAllGroupDetails.Iconimage_Selected = null;
            this.btnAllGroupDetails.IconMarginLeft = 0;
            this.btnAllGroupDetails.IconMarginRight = 0;
            this.btnAllGroupDetails.IconRightVisible = true;
            this.btnAllGroupDetails.IconRightZoom = 0D;
            this.btnAllGroupDetails.IconVisible = true;
            this.btnAllGroupDetails.IconZoom = 55D;
            this.btnAllGroupDetails.IsTab = false;
            this.btnAllGroupDetails.Location = new System.Drawing.Point(923, 23);
            this.btnAllGroupDetails.Name = "btnAllGroupDetails";
            this.btnAllGroupDetails.Normalcolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAllGroupDetails.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(90)))), ((int)(((byte)(229)))));
            this.btnAllGroupDetails.OnHoverTextColor = System.Drawing.Color.White;
            this.btnAllGroupDetails.selected = false;
            this.btnAllGroupDetails.Size = new System.Drawing.Size(152, 30);
            this.btnAllGroupDetails.TabIndex = 21;
            this.btnAllGroupDetails.Text = "All Group Details";
            this.btnAllGroupDetails.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAllGroupDetails.Textcolor = System.Drawing.Color.White;
            this.btnAllGroupDetails.TextFont = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(226, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 21);
            this.label2.TabIndex = 20;
            this.label2.Text = "Customer";
            // 
            // cbEdataType
            // 
            this.cbEdataType.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbEdataType.FormattingEnabled = true;
            this.cbEdataType.Location = new System.Drawing.Point(574, 31);
            this.cbEdataType.Name = "cbEdataType";
            this.cbEdataType.Size = new System.Drawing.Size(145, 30);
            this.cbEdataType.TabIndex = 23;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(577, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 21);
            this.label4.TabIndex = 24;
            this.label4.Text = "Edata-Type";
            // 
            // ucEmailSending
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbEdataType);
            this.Controls.Add(this.cbProduct);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.cbCustomer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnAllGroupDetails);
            this.Controls.Add(this.label2);
            this.Name = "ucEmailSending";
            this.Size = new System.Drawing.Size(1090, 515);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.pnAlert.ResumeLayout(false);
            this.pnAlert.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtgShowEmailAll)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbProduct;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel pnAlert;
        private Bunifu.Framework.UI.BunifuFlatButton btnAlertAction;
        private System.Windows.Forms.Label lbAlertText;
        private System.Windows.Forms.Label lbAlertIcon;
        private System.Windows.Forms.Label lbForGroupID;
        private Bunifu.Framework.UI.BunifuFlatButton btnAddToGroup;
        private System.Windows.Forms.ComboBox cbToOrCC;
        private Bunifu.Framework.UI.BunifuMetroTextbox txtAddEmail;
        private Bunifu.Framework.UI.BunifuCustomDataGrid dtgShowEmailAll;
        private System.Windows.Forms.Label lbChangeForReceiver;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbCustomer;
        private System.Windows.Forms.Label label1;
        private Bunifu.Framework.UI.BunifuFlatButton btnAllGroupDetails;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbEdataType;
        private System.Windows.Forms.Label label4;
    }
}
