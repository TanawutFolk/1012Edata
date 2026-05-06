namespace EdataManagement.Views
{
    partial class ftpUserControl
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lb_pathServer = new System.Windows.Forms.Label();
            this.tb_pathServer = new System.Windows.Forms.TextBox();
            this.tb_searchFile = new System.Windows.Forms.TextBox();
            this.lb_SerialShippingCount = new System.Windows.Forms.Label();
            this.bt_sendFTP = new System.Windows.Forms.Button();
            this.dataGridViewCustomer = new System.Windows.Forms.DataGridView();
            this.dataGridViewSerialShipping = new System.Windows.Forms.DataGridView();
            this.bdFileCustomer = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSerialShipping)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdFileCustomer)).BeginInit();
            this.SuspendLayout();
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(356, 0);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(151, 24);
            this.label7.TabIndex = 42;
            this.label7.Text = "Customer Server";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(27, 0);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(106, 24);
            this.label6.TabIndex = 41;
            this.label6.Text = "FFT Server";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(511, 4);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 40;
            this.label5.Text = "Search File :";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // lb_pathServer
            // 
            this.lb_pathServer.AutoSize = true;
            this.lb_pathServer.BackColor = System.Drawing.Color.Transparent;
            this.lb_pathServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_pathServer.Location = new System.Drawing.Point(511, 29);
            this.lb_pathServer.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lb_pathServer.Name = "lb_pathServer";
            this.lb_pathServer.Size = new System.Drawing.Size(69, 13);
            this.lb_pathServer.TabIndex = 39;
            this.lb_pathServer.Text = "Path Server :";
            this.lb_pathServer.Click += new System.EventHandler(this.lb_pathServer_Click);
            // 
            // tb_pathServer
            // 
            this.tb_pathServer.Location = new System.Drawing.Point(584, 26);
            this.tb_pathServer.Margin = new System.Windows.Forms.Padding(2);
            this.tb_pathServer.Name = "tb_pathServer";
            this.tb_pathServer.ReadOnly = true;
            this.tb_pathServer.Size = new System.Drawing.Size(302, 20);
            this.tb_pathServer.TabIndex = 38;
            this.tb_pathServer.TextChanged += new System.EventHandler(this.tb_pathServer_TextChanged);
            // 
            // tb_searchFile
            // 
            this.tb_searchFile.Enabled = false;
            this.tb_searchFile.Location = new System.Drawing.Point(584, 2);
            this.tb_searchFile.Margin = new System.Windows.Forms.Padding(2);
            this.tb_searchFile.Name = "tb_searchFile";
            this.tb_searchFile.Size = new System.Drawing.Size(302, 20);
            this.tb_searchFile.TabIndex = 37;
            this.tb_searchFile.TextChanged += new System.EventHandler(this.tb_searchFile_TextChanged);
            // 
            // lb_SerialShippingCount
            // 
            this.lb_SerialShippingCount.AutoSize = true;
            this.lb_SerialShippingCount.Location = new System.Drawing.Point(28, 24);
            this.lb_SerialShippingCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lb_SerialShippingCount.Name = "lb_SerialShippingCount";
            this.lb_SerialShippingCount.Size = new System.Drawing.Size(46, 13);
            this.lb_SerialShippingCount.TabIndex = 36;
            this.lb_SerialShippingCount.Text = "จำนวน :";
            // 
            // bt_sendFTP
            // 
            this.bt_sendFTP.Enabled = false;
            this.bt_sendFTP.Location = new System.Drawing.Point(276, 103);
            this.bt_sendFTP.Margin = new System.Windows.Forms.Padding(2);
            this.bt_sendFTP.Name = "bt_sendFTP";
            this.bt_sendFTP.Size = new System.Drawing.Size(69, 68);
            this.bt_sendFTP.TabIndex = 35;
            this.bt_sendFTP.Text = "send";
            this.bt_sendFTP.UseVisualStyleBackColor = true;
            this.bt_sendFTP.Click += new System.EventHandler(this.bt_sendFTP_Click);
            // 
            // dataGridViewCustomer
            // 
            this.dataGridViewCustomer.AllowUserToAddRows = false;
            this.dataGridViewCustomer.AllowUserToDeleteRows = false;
            this.dataGridViewCustomer.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewCustomer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewCustomer.ColumnHeadersVisible = false;
            this.dataGridViewCustomer.Location = new System.Drawing.Point(360, 50);
            this.dataGridViewCustomer.Margin = new System.Windows.Forms.Padding(2);
            this.dataGridViewCustomer.MultiSelect = false;
            this.dataGridViewCustomer.Name = "dataGridViewCustomer";
            this.dataGridViewCustomer.ReadOnly = true;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Transparent;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewCustomer.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewCustomer.RowHeadersVisible = false;
            this.dataGridViewCustomer.RowHeadersWidth = 51;
            this.dataGridViewCustomer.RowTemplate.Height = 24;
            this.dataGridViewCustomer.Size = new System.Drawing.Size(650, 181);
            this.dataGridViewCustomer.TabIndex = 34;
            this.dataGridViewCustomer.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewCustomer_CellClick);
            this.dataGridViewCustomer.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewCustomer_CellContentClick);
            this.dataGridViewCustomer.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewCustomer_CellDoubleClick);
            // 
            // dataGridViewSerialShipping
            // 
            this.dataGridViewSerialShipping.AllowUserToAddRows = false;
            this.dataGridViewSerialShipping.AllowUserToDeleteRows = false;
            this.dataGridViewSerialShipping.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewSerialShipping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSerialShipping.ColumnHeadersVisible = false;
            this.dataGridViewSerialShipping.Location = new System.Drawing.Point(31, 50);
            this.dataGridViewSerialShipping.Margin = new System.Windows.Forms.Padding(2);
            this.dataGridViewSerialShipping.MultiSelect = false;
            this.dataGridViewSerialShipping.Name = "dataGridViewSerialShipping";
            this.dataGridViewSerialShipping.ReadOnly = true;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Transparent;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewSerialShipping.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewSerialShipping.RowHeadersVisible = false;
            this.dataGridViewSerialShipping.RowHeadersWidth = 51;
            this.dataGridViewSerialShipping.RowTemplate.Height = 24;
            this.dataGridViewSerialShipping.Size = new System.Drawing.Size(231, 181);
            this.dataGridViewSerialShipping.TabIndex = 33;
            this.dataGridViewSerialShipping.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewSerialShipping_CellClick);
            this.dataGridViewSerialShipping.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewSerialShipping_CellContentClick);
            this.dataGridViewSerialShipping.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewSerialShipping_CellDoubleClick);
            // 
            // ftpUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lb_pathServer);
            this.Controls.Add(this.tb_pathServer);
            this.Controls.Add(this.tb_searchFile);
            this.Controls.Add(this.lb_SerialShippingCount);
            this.Controls.Add(this.bt_sendFTP);
            this.Controls.Add(this.dataGridViewCustomer);
            this.Controls.Add(this.dataGridViewSerialShipping);
            this.Name = "ftpUserControl";
            this.Size = new System.Drawing.Size(1263, 264);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSerialShipping)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdFileCustomer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lb_pathServer;
        private System.Windows.Forms.TextBox tb_pathServer;
        private System.Windows.Forms.TextBox tb_searchFile;
        private System.Windows.Forms.Label lb_SerialShippingCount;
        private System.Windows.Forms.Button bt_sendFTP;
        private System.Windows.Forms.DataGridView dataGridViewCustomer;
        private System.Windows.Forms.DataGridView dataGridViewSerialShipping;
        private System.Windows.Forms.BindingSource bdFileCustomer;
    }
}
