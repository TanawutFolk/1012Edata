namespace EdataManagement.Views
{
    partial class emailUserControl
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
            this.label6 = new System.Windows.Forms.Label();
            this.lb_SerialShippingCount = new System.Windows.Forms.Label();
            this.dataGridViewDocs = new System.Windows.Forms.DataGridView();
            this.bt_sendEmail = new System.Windows.Forms.Button();
            this.bdEmail = new System.Windows.Forms.BindingSource(this.components);
            this.bdSerialList = new System.Windows.Forms.BindingSource(this.components);
            this.bdWafer = new System.Windows.Forms.BindingSource(this.components);
            this.bt_ShowEmail = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDocs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdEmail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdSerialList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdWafer)).BeginInit();
            this.SuspendLayout();
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
            this.label6.TabIndex = 46;
            this.label6.Text = "FFT Server";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // lb_SerialShippingCount
            // 
            this.lb_SerialShippingCount.AutoSize = true;
            this.lb_SerialShippingCount.Location = new System.Drawing.Point(28, 24);
            this.lb_SerialShippingCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lb_SerialShippingCount.Name = "lb_SerialShippingCount";
            this.lb_SerialShippingCount.Size = new System.Drawing.Size(46, 13);
            this.lb_SerialShippingCount.TabIndex = 47;
            this.lb_SerialShippingCount.Text = "จำนวน :";
            // 
            // dataGridViewDocs
            // 
            this.dataGridViewDocs.AllowUserToAddRows = false;
            this.dataGridViewDocs.AllowUserToDeleteRows = false;
            this.dataGridViewDocs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewDocs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDocs.ColumnHeadersVisible = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.NullValue = null;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewDocs.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewDocs.Location = new System.Drawing.Point(31, 39);
            this.dataGridViewDocs.Margin = new System.Windows.Forms.Padding(2);
            this.dataGridViewDocs.MultiSelect = false;
            this.dataGridViewDocs.Name = "dataGridViewDocs";
            this.dataGridViewDocs.ReadOnly = true;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Transparent;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDocs.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewDocs.RowHeadersVisible = false;
            this.dataGridViewDocs.RowHeadersWidth = 51;
            this.dataGridViewDocs.RowTemplate.Height = 24;
            this.dataGridViewDocs.Size = new System.Drawing.Size(568, 191);
            this.dataGridViewDocs.TabIndex = 48;
            this.dataGridViewDocs.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewDocs_CellContentClick);
            // 
            // bt_sendEmail
            // 
            this.bt_sendEmail.Location = new System.Drawing.Point(626, 90);
            this.bt_sendEmail.Margin = new System.Windows.Forms.Padding(2);
            this.bt_sendEmail.Name = "bt_sendEmail";
            this.bt_sendEmail.Size = new System.Drawing.Size(69, 68);
            this.bt_sendEmail.TabIndex = 49;
            this.bt_sendEmail.Text = "send";
            this.bt_sendEmail.UseVisualStyleBackColor = true;
            this.bt_sendEmail.Click += new System.EventHandler(this.bt_sendEmail_Click);
            // 
            // bt_ShowEmail
            // 
            this.bt_ShowEmail.Location = new System.Drawing.Point(155, 6);
            this.bt_ShowEmail.Margin = new System.Windows.Forms.Padding(2);
            this.bt_ShowEmail.Name = "bt_ShowEmail";
            this.bt_ShowEmail.Size = new System.Drawing.Size(72, 29);
            this.bt_ShowEmail.TabIndex = 50;
            this.bt_ShowEmail.Text = "Show Email";
            this.bt_ShowEmail.UseVisualStyleBackColor = true;
            this.bt_ShowEmail.Visible = false;
            this.bt_ShowEmail.Click += new System.EventHandler(this.bt_ShowEmail_Click);
            // 
            // emailUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bt_ShowEmail);
            this.Controls.Add(this.bt_sendEmail);
            this.Controls.Add(this.dataGridViewDocs);
            this.Controls.Add(this.lb_SerialShippingCount);
            this.Controls.Add(this.label6);
            this.Name = "emailUserControl";
            this.Size = new System.Drawing.Size(1263, 252);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDocs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdEmail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdSerialList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdWafer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lb_SerialShippingCount;
        private System.Windows.Forms.DataGridView dataGridViewDocs;
        private System.Windows.Forms.Button bt_sendEmail;
        private System.Windows.Forms.BindingSource bdEmail;
        private System.Windows.Forms.BindingSource bdSerialList;
        private System.Windows.Forms.BindingSource bdWafer;
        private System.Windows.Forms.Button bt_ShowEmail;
    }
}
