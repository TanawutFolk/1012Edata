namespace EdataManagement
{
    partial class frmMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.richTextBoxLog = new System.Windows.Forms.RichTextBox();
            this.shipCalendar = new System.Windows.Forms.MonthCalendar();
            this.dataGridViewShipping = new System.Windows.Forms.DataGridView();
            this.bt_Confirm = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.lb_logMsg = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.lb_totalOrder = new System.Windows.Forms.Label();
            this.cb_product = new System.Windows.Forms.ComboBox();
            this.bt_selectOrder = new System.Windows.Forms.Button();
            this.bt_clearAll = new System.Windows.Forms.Button();
            this.tb_path_shipping = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.panelDelivery = new System.Windows.Forms.Panel();
            this.bdShipping = new System.Windows.Forms.BindingSource(this.components);
            this.cb_customer = new System.Windows.Forms.ComboBox();
            this.multiSelectCheckBox = new System.Windows.Forms.CheckBox();
            this.picSetting = new System.Windows.Forms.PictureBox();
            this.lbSetting = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewShipping)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bdShipping)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSetting)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.Location = new System.Drawing.Point(997, 124);
            this.richTextBoxLog.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.ReadOnly = true;
            this.richTextBoxLog.Size = new System.Drawing.Size(277, 217);
            this.richTextBoxLog.TabIndex = 0;
            this.richTextBoxLog.Text = "";
            // 
            // shipCalendar
            // 
            this.shipCalendar.Location = new System.Drawing.Point(9, 179);
            this.shipCalendar.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.shipCalendar.Name = "shipCalendar";
            this.shipCalendar.ShowTodayCircle = false;
            this.shipCalendar.TabIndex = 4;
            this.shipCalendar.TitleBackColor = System.Drawing.Color.Plum;
            this.shipCalendar.TitleForeColor = System.Drawing.Color.DeepPink;
            // 
            // dataGridViewShipping
            // 
            this.dataGridViewShipping.AllowUserToAddRows = false;
            this.dataGridViewShipping.AllowUserToDeleteRows = false;
            this.dataGridViewShipping.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewShipping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewShipping.Location = new System.Drawing.Point(338, 179);
            this.dataGridViewShipping.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dataGridViewShipping.MultiSelect = false;
            this.dataGridViewShipping.Name = "dataGridViewShipping";
            this.dataGridViewShipping.ReadOnly = true;
            this.dataGridViewShipping.RowHeadersVisible = false;
            this.dataGridViewShipping.RowHeadersWidth = 51;
            this.dataGridViewShipping.RowTemplate.Height = 24;
            this.dataGridViewShipping.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewShipping.Size = new System.Drawing.Size(650, 162);
            this.dataGridViewShipping.TabIndex = 5;
            this.dataGridViewShipping.SelectionChanged += new System.EventHandler(this.myDataGridView_SelectionChanged);
            // 
            // bt_Confirm
            // 
            this.bt_Confirm.Location = new System.Drawing.Point(248, 236);
            this.bt_Confirm.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.bt_Confirm.Name = "bt_Confirm";
            this.bt_Confirm.Size = new System.Drawing.Size(69, 48);
            this.bt_Confirm.TabIndex = 12;
            this.bt_Confirm.Text = "Confirm Date";
            this.bt_Confirm.UseVisualStyleBackColor = true;
            this.bt_Confirm.Click += new System.EventHandler(this.bt_Confirm_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbSetting);
            this.panel1.Controls.Add(this.picSetting);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Location = new System.Drawing.Point(9, 10);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1265, 61);
            this.panel1.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(525, 15);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(321, 24);
            this.label4.TabIndex = 2;
            this.label4.Text = "1012 Send E-data 48-98 Direct V3.6.0";
            // 
            // lb_logMsg
            // 
            this.lb_logMsg.AutoSize = true;
            this.lb_logMsg.BackColor = System.Drawing.Color.Transparent;
            this.lb_logMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_logMsg.Location = new System.Drawing.Point(1067, 89);
            this.lb_logMsg.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lb_logMsg.Name = "lb_logMsg";
            this.lb_logMsg.Size = new System.Drawing.Size(124, 24);
            this.lb_logMsg.TabIndex = 0;
            this.lb_logMsg.Text = "Log Message";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(334, 141);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(136, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "Please select...";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.label3);
            this.panel4.Location = new System.Drawing.Point(9, 78);
            this.panel4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(227, 49);
            this.panel4.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F);
            this.label3.Location = new System.Drawing.Point(2, 11);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(210, 24);
            this.label3.TabIndex = 1;
            this.label3.Text = "Please select product....";
            // 
            // lb_totalOrder
            // 
            this.lb_totalOrder.AutoSize = true;
            this.lb_totalOrder.Location = new System.Drawing.Point(942, 124);
            this.lb_totalOrder.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lb_totalOrder.Name = "lb_totalOrder";
            this.lb_totalOrder.Size = new System.Drawing.Size(46, 13);
            this.lb_totalOrder.TabIndex = 18;
            this.lb_totalOrder.Text = "จำนวน :";
            // 
            // cb_product
            // 
            this.cb_product.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_product.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_product.FormattingEnabled = true;
            this.cb_product.ItemHeight = 33;
            this.cb_product.Items.AddRange(new object[] {
            "1480"});
            this.cb_product.Location = new System.Drawing.Point(6, 131);
            this.cb_product.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cb_product.Name = "cb_product";
            this.cb_product.Size = new System.Drawing.Size(230, 41);
            this.cb_product.TabIndex = 22;
            this.cb_product.SelectedIndexChanged += new System.EventHandler(this.cb_product_SelectedIndexChanged);
            // 
            // bt_selectOrder
            // 
            this.bt_selectOrder.Enabled = false;
            this.bt_selectOrder.Location = new System.Drawing.Point(803, 122);
            this.bt_selectOrder.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.bt_selectOrder.Name = "bt_selectOrder";
            this.bt_selectOrder.Size = new System.Drawing.Size(64, 43);
            this.bt_selectOrder.TabIndex = 24;
            this.bt_selectOrder.Text = "Select";
            this.bt_selectOrder.UseVisualStyleBackColor = true;
            this.bt_selectOrder.Click += new System.EventHandler(this.bt_selectOrder_Click);
            // 
            // bt_clearAll
            // 
            this.bt_clearAll.Location = new System.Drawing.Point(873, 122);
            this.bt_clearAll.Name = "bt_clearAll";
            this.bt_clearAll.Size = new System.Drawing.Size(64, 43);
            this.bt_clearAll.TabIndex = 30;
            this.bt_clearAll.Text = "Clear";
            this.bt_clearAll.UseVisualStyleBackColor = true;
            this.bt_clearAll.Click += new System.EventHandler(this.bt_clearAll_Click);
            // 
            // tb_path_shipping
            // 
            this.tb_path_shipping.Location = new System.Drawing.Point(475, 86);
            this.tb_path_shipping.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tb_path_shipping.Name = "tb_path_shipping";
            this.tb_path_shipping.ReadOnly = true;
            this.tb_path_shipping.Size = new System.Drawing.Size(392, 20);
            this.tb_path_shipping.TabIndex = 33;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label8.Location = new System.Drawing.Point(335, 89);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(79, 13);
            this.label8.TabIndex = 34;
            this.label8.Text = "Path Shipping :";
            // 
            // panelDelivery
            // 
            this.panelDelivery.Location = new System.Drawing.Point(11, 350);
            this.panelDelivery.Name = "panelDelivery";
            this.panelDelivery.Size = new System.Drawing.Size(1263, 233);
            this.panelDelivery.TabIndex = 35;
            // 
            // bdShipping
            // 
            this.bdShipping.CurrentChanged += new System.EventHandler(this.bdShipping_CurrentChanged);
            // 
            // cb_customer
            // 
            this.cb_customer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_customer.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_customer.FormattingEnabled = true;
            this.cb_customer.Location = new System.Drawing.Point(475, 124);
            this.cb_customer.Name = "cb_customer";
            this.cb_customer.Size = new System.Drawing.Size(307, 41);
            this.cb_customer.TabIndex = 36;
            this.cb_customer.SelectedIndexChanged += new System.EventHandler(this.cb_customer_SelectedIndexChanged);
            // 
            // multiSelectCheckBox
            // 
            this.multiSelectCheckBox.AutoSize = true;
            this.multiSelectCheckBox.Checked = true;
            this.multiSelectCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.multiSelectCheckBox.Enabled = false;
            this.multiSelectCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.multiSelectCheckBox.Location = new System.Drawing.Point(873, 79);
            this.multiSelectCheckBox.Name = "multiSelectCheckBox";
            this.multiSelectCheckBox.Size = new System.Drawing.Size(129, 29);
            this.multiSelectCheckBox.TabIndex = 37;
            this.multiSelectCheckBox.Text = "ALL Order";
            this.multiSelectCheckBox.UseVisualStyleBackColor = true;
            this.multiSelectCheckBox.CheckedChanged += new System.EventHandler(this.multiSelectCheckBox_CheckedChanged);
            // 
            // picSetting
            // 
            this.picSetting.ErrorImage = null;
            this.picSetting.Image = ((System.Drawing.Image)(resources.GetObject("picSetting.Image")));
            this.picSetting.Location = new System.Drawing.Point(1212, 7);
            this.picSetting.Name = "picSetting";
            this.picSetting.Size = new System.Drawing.Size(35, 35);
            this.picSetting.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSetting.TabIndex = 3;
            this.picSetting.TabStop = false;
            // 
            // lbSetting
            // 
            this.lbSetting.AutoSize = true;
            this.lbSetting.Location = new System.Drawing.Point(1210, 45);
            this.lbSetting.Name = "lbSetting";
            this.lbSetting.Size = new System.Drawing.Size(40, 13);
            this.lbSetting.TabIndex = 4;
            this.lbSetting.Text = "Setting";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1332, 790);
            this.Controls.Add(this.multiSelectCheckBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cb_customer);
            this.Controls.Add(this.panelDelivery);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tb_path_shipping);
            this.Controls.Add(this.bt_clearAll);
            this.Controls.Add(this.bt_selectOrder);
            this.Controls.Add(this.lb_logMsg);
            this.Controls.Add(this.cb_product);
            this.Controls.Add(this.lb_totalOrder);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.bt_Confirm);
            this.Controls.Add(this.dataGridViewShipping);
            this.Controls.Add(this.shipCalendar);
            this.Controls.Add(this.richTextBoxLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "  ";
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewShipping)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bdShipping)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSetting)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxLog;
        private System.Windows.Forms.MonthCalendar shipCalendar;
        private System.Windows.Forms.DataGridView dataGridViewShipping;
        private System.Windows.Forms.Button bt_Confirm;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lb_logMsg;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lb_totalOrder;
        private System.Windows.Forms.ComboBox cb_product;
        private System.Windows.Forms.BindingSource bdShipping;
        private System.Windows.Forms.Button bt_selectOrder;
        private System.Windows.Forms.Button bt_clearAll;
        private System.Windows.Forms.TextBox tb_path_shipping;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panelDelivery;
        private System.Windows.Forms.ComboBox cb_customer;
        private System.Windows.Forms.CheckBox multiSelectCheckBox;
        private System.Windows.Forms.PictureBox picSetting;
        private System.Windows.Forms.Label lbSetting;
    }
}

