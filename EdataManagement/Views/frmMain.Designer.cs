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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewShipping)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bdShipping)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.Location = new System.Drawing.Point(1329, 153);
            this.richTextBoxLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.ReadOnly = true;
            this.richTextBoxLog.Size = new System.Drawing.Size(368, 266);
            this.richTextBoxLog.TabIndex = 0;
            this.richTextBoxLog.Text = "";
            // 
            // shipCalendar
            // 
            this.shipCalendar.Location = new System.Drawing.Point(12, 220);
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
            this.dataGridViewShipping.Location = new System.Drawing.Point(451, 220);
            this.dataGridViewShipping.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGridViewShipping.MultiSelect = false;
            this.dataGridViewShipping.Name = "dataGridViewShipping";
            this.dataGridViewShipping.ReadOnly = true;
            this.dataGridViewShipping.RowHeadersVisible = false;
            this.dataGridViewShipping.RowHeadersWidth = 51;
            this.dataGridViewShipping.RowTemplate.Height = 24;
            this.dataGridViewShipping.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewShipping.Size = new System.Drawing.Size(867, 199);
            this.dataGridViewShipping.TabIndex = 5;
            this.dataGridViewShipping.SelectionChanged += new System.EventHandler(this.myDataGridView_SelectionChanged);
            // 
            // bt_Confirm
            // 
            this.bt_Confirm.Location = new System.Drawing.Point(331, 290);
            this.bt_Confirm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_Confirm.Name = "bt_Confirm";
            this.bt_Confirm.Size = new System.Drawing.Size(92, 59);
            this.bt_Confirm.TabIndex = 12;
            this.bt_Confirm.Text = "Confirm Date";
            this.bt_Confirm.UseVisualStyleBackColor = true;
            this.bt_Confirm.Click += new System.EventHandler(this.bt_Confirm_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label4);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1687, 75);
            this.panel1.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(700, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(411, 29);
            this.label4.TabIndex = 2;
            this.label4.Text = "1012 Send E-data 48-98 Direct V3.6.0";
            // 
            // lb_logMsg
            // 
            this.lb_logMsg.AutoSize = true;
            this.lb_logMsg.BackColor = System.Drawing.Color.Transparent;
            this.lb_logMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_logMsg.Location = new System.Drawing.Point(1423, 110);
            this.lb_logMsg.Name = "lb_logMsg";
            this.lb_logMsg.Size = new System.Drawing.Size(159, 29);
            this.lb_logMsg.TabIndex = 0;
            this.lb_logMsg.Text = "Log Message";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(445, 174);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(176, 29);
            this.label2.TabIndex = 1;
            this.label2.Text = "Please select...";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.label3);
            this.panel4.Location = new System.Drawing.Point(12, 96);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(303, 60);
            this.panel4.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F);
            this.label3.Location = new System.Drawing.Point(3, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(269, 29);
            this.label3.TabIndex = 1;
            this.label3.Text = "Please select product....";
            // 
            // lb_totalOrder
            // 
            this.lb_totalOrder.AutoSize = true;
            this.lb_totalOrder.Location = new System.Drawing.Point(1256, 153);
            this.lb_totalOrder.Name = "lb_totalOrder";
            this.lb_totalOrder.Size = new System.Drawing.Size(44, 16);
            this.lb_totalOrder.TabIndex = 18;
            this.lb_totalOrder.Text = "จำนวน :";
            // 
            // cb_product
            // 
            this.cb_product.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_product.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_product.FormattingEnabled = true;
            this.cb_product.ItemHeight = 40;
            this.cb_product.Items.AddRange(new object[] {
            "1480"});
            this.cb_product.Location = new System.Drawing.Point(8, 161);
            this.cb_product.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cb_product.Name = "cb_product";
            this.cb_product.Size = new System.Drawing.Size(305, 48);
            this.cb_product.TabIndex = 22;
            this.cb_product.SelectedIndexChanged += new System.EventHandler(this.cb_product_SelectedIndexChanged);
            // 
            // bt_selectOrder
            // 
            this.bt_selectOrder.Enabled = false;
            this.bt_selectOrder.Location = new System.Drawing.Point(1071, 150);
            this.bt_selectOrder.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_selectOrder.Name = "bt_selectOrder";
            this.bt_selectOrder.Size = new System.Drawing.Size(85, 53);
            this.bt_selectOrder.TabIndex = 24;
            this.bt_selectOrder.Text = "Select";
            this.bt_selectOrder.UseVisualStyleBackColor = true;
            this.bt_selectOrder.Click += new System.EventHandler(this.bt_selectOrder_Click);
            // 
            // bt_clearAll
            // 
            this.bt_clearAll.Location = new System.Drawing.Point(1164, 150);
            this.bt_clearAll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.bt_clearAll.Name = "bt_clearAll";
            this.bt_clearAll.Size = new System.Drawing.Size(85, 53);
            this.bt_clearAll.TabIndex = 30;
            this.bt_clearAll.Text = "Clear";
            this.bt_clearAll.UseVisualStyleBackColor = true;
            this.bt_clearAll.Click += new System.EventHandler(this.bt_clearAll_Click);
            // 
            // tb_path_shipping
            // 
            this.tb_path_shipping.Location = new System.Drawing.Point(633, 106);
            this.tb_path_shipping.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_path_shipping.Name = "tb_path_shipping";
            this.tb_path_shipping.ReadOnly = true;
            this.tb_path_shipping.Size = new System.Drawing.Size(521, 22);
            this.tb_path_shipping.TabIndex = 33;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label8.Location = new System.Drawing.Point(447, 110);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(104, 17);
            this.label8.TabIndex = 34;
            this.label8.Text = "Path Shipping :";
            // 
            // panelDelivery
            // 
            this.panelDelivery.Location = new System.Drawing.Point(15, 431);
            this.panelDelivery.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelDelivery.Name = "panelDelivery";
            this.panelDelivery.Size = new System.Drawing.Size(1684, 287);
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
            this.cb_customer.Location = new System.Drawing.Point(633, 153);
            this.cb_customer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cb_customer.Name = "cb_customer";
            this.cb_customer.Size = new System.Drawing.Size(408, 48);
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
            this.multiSelectCheckBox.Location = new System.Drawing.Point(1164, 97);
            this.multiSelectCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.multiSelectCheckBox.Name = "multiSelectCheckBox";
            this.multiSelectCheckBox.Size = new System.Drawing.Size(160, 35);
            this.multiSelectCheckBox.TabIndex = 37;
            this.multiSelectCheckBox.Text = "ALL Order";
            this.multiSelectCheckBox.UseVisualStyleBackColor = true;
            this.multiSelectCheckBox.CheckedChanged += new System.EventHandler(this.multiSelectCheckBox_CheckedChanged);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1776, 972);
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
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
    }
}

