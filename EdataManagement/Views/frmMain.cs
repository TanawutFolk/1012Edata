using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


//using EdataManagement.Models;
//using EdataManagement.Services;
using EdataManagement.Controllers;
using EdataManagement.Property;
using EdataManagement.Utilities;
using System.Configuration;
using Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;
using System.Windows.Controls;
using Control = System.Windows.Forms.Control;
using System.Globalization;
using System.Threading;
using System.Data.Common;

namespace EdataManagement
{
    public partial class frmMain : Form
    {

        EmailCls emailCls = new EmailCls();
        EmailProperty emailProp = new EmailProperty();

        private Dictionary<int, Color> rowColors = new Dictionary<int, Color>(); // ตัวแปรเก็บสีของแถว

        // Setting shortcut (pnClickToSetting + picSetting + lbSetting)
        // this file has both System.Windows.Controls and System.Windows.Forms in scope
        private readonly System.Windows.Forms.ToolTip toolTipSetting = new System.Windows.Forms.ToolTip();
        public DataGridView mainDataGridView => dataGridViewShipping;

        public frmMain()
        {
            // กำหนด CultureInfo ให้กับทั้งโปรเจกต์ 2024-06-07
            CultureInfo culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            //string pathExe = @"C:\xampp\xampp_start.exe";
            //emailCls.runAs(pathExe);
            InitializeComponent();

        }

        public void AppendToRichTextBoxLog(string text)
        {
            richTextBoxLog.AppendText(text + Environment.NewLine);
        }

        private string GetDbProductForRun(string productName, string dbProduct)
        {
            // Temporary test override. Remove this when 1480 should use info_db_name.DATABASE_NAME again.
            //if (productName == "1480")
            //{
            //    return "ddb_test";
            //}

            return dbProduct;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

            SettingProperty sp = new SettingProperty();
            SettingControllers scon = new SettingControllers();

            string strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder
            {
                ConnectionString = strConnection
            };
            string databaseName = builder["Database"].ToString();

            // any database but qa_oc: mail is saved as a draft and SFTP is skipped (Utilities.MailSafety)
            if (Utilities.MailSafety.IsTestMode)
            {
                string testGroups = string.Join(", ", Utilities.MailSafety.TestMailGroupIds);
                label4.Text = label4.Text + "   TEST MODE (" + databaseName + ") - " +
                              (testGroups.Length > 0 ? "mail only to test groups " + testGroups : "no mail") + ", no FTP";
                label4.ForeColor = Color.FromArgb(185, 28, 28);
                this.Text = "[TEST MODE - " + databaseName + "] " + this.Text;
            }

            this.BackColor = Color.FromArgb(255, 249, 220, 222);

            MainControllers mainControllers = new MainControllers();
            DataTable product = mainControllers.getProduct();

            cb_product.DataSource = product;
            cb_product.DisplayMember = "PRODUCT_NAME";
            cb_product.ValueMember = "PRODUCT_ID";


            foreach (DataRow dtRow in product.Rows)
            {
                sp.productName = dtRow["PRODUCT_NAME"].ToString();
                sp.dbProduct = GetDbProductForRun(sp.productName, dtRow["DATABASE_NAME"].ToString());

                if (scon.updateLastData(sp) == true)
                {

                }

            }

            cb_product.SelectedIndexChanged += cb_product_SelectedIndexChanged;
            //cb_product.SelectedIndex = 0;

            WireSettingShortcut();

            UpdatePathText();

        }

        private void cb_product_SelectedIndexChanged(object sender, EventArgs e)
        {
            // เมื่อเลือกรายการใน ComboBox แก้ไข LabelName
            UpdatePathText();
            clearDataForm();
        }

        private void UpdatePathText()
        {
            // ดึงข้อมูลจาก ComboBox
            DataRowView selectedRow = (DataRowView)cb_product.SelectedItem;

            // ตรวจสอบว่ามีข้อมูลหรือไม่
            if (selectedRow != null)
            {
                // ดึงค่าที่ต้องการจาก DataRowView แสดง path shipping data
                string selectedPathName = selectedRow["PATH_SHIPPING"].ToString();

                // แสดงผลลัพธ์ใน LabelName
                if (selectedPathName.EndsWith("\\"))
                {
                    // ถ้าลงท้ายด้วย backslash ไม่ต้องทำอะไร
                }
                else
                {
                    //เติม backslash
                    selectedPathName += "\\";
                }

                tb_path_shipping.Text = selectedPathName;
            }
            else
            {
                // หากรายการว่าง, ก็ให้ LabelName เป็นข้อความว่า "No Name Selected"
                tb_path_shipping.Text = "No Name Selected";
            }

            cb_customer.SelectedIndexChanged -= cb_customer_SelectedIndexChanged;
            cb_customer.DataSource = null;

        }

        private void bt_Confirm_Click(object sender, EventArgs e)
        {
            //cb_customer จะเกิดการ active SelectedIndexChanged ที่ datasource จะวิ่งไปที่ selectedIndexChanged จึงทำการ -= หยุดใช้งานชั่วคราว => += เพื่อเริ่มต่อ
            cb_customer.SelectedIndexChanged -= cb_customer_SelectedIndexChanged;

            clearDataForm();

            DateTime selectedDate = shipCalendar.SelectionRange.Start;

            ShippingProperty dataItem = new ShippingProperty();
            ShippingControllers _controllerShip = new ShippingControllers();

            // ดึงข้อมูลจาก ComboBox cb_Product
            DataRowView selectedRowProduct = (DataRowView)cb_product.SelectedItem;

            //dataItem.conn = ConfigurationManager.ConnectionStrings["ConnectionStandAlone"].ConnectionString;
            //dataItem.connProdName = selectedRowProduct["CONNECTION_PRODUCTION_NAME"].ToString();


            dataItem.shipdate = selectedDate.ToString("yyyy-MM-dd");
            dataItem.productName = selectedRowProduct["PRODUCT_NAME"].ToString();
            dataItem.dbProduct = GetDbProductForRun(dataItem.productName, selectedRowProduct["DATABASE_NAME"].ToString());

            DataTable customer = _controllerShip.searchCustomerShipping(dataItem);


            if (customer == null || customer.Rows.Count == 0)
            {
                // กรณี DataTable เป็น null หรือไม่มีข้อมูล
                cb_customer.DataSource = null;
                MessageBox.Show("ไม่พบ ship date ของวันที่ " + dataItem.shipdate, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // เอาแต่ customer มาก่อน แล้วค่อยไปกวาด
            cb_customer.DataSource = customer;
            cb_customer.DisplayMember = "customerName";
            cb_customer.ValueMember = "customerName";

            cb_customer.SelectedIndexChanged += cb_customer_SelectedIndexChanged;


        }


        public void clearDataForm()
        {
            //multiSelectCheckBox.Checked = true;
            //multiSelectCheckBox_CheckedChanged(multiSelectCheckBox, EventArgs.Empty);

            dataGridViewShipping.Enabled = true;

            dataGridViewShipping.DataSource = null;

            dataGridViewShipping.Rows.Clear();

            bt_selectOrder.Enabled = false;

            lb_totalOrder.Text = "จำนวน :";

            panelDelivery.Controls.Clear();



        }

        private void bt_selectOrder_Click(object sender, EventArgs e)
        {
            panelDelivery.Controls.Clear();


        }

        private void order_email_click(object sender, EventArgs e)
        {
            if (dataGridViewShipping.SelectedRows.Count == 0)
            {
                MessageBox.Show("เลือกส่งอย่างน้อย 1 test report ถ้าเลือกหมด กรุณา click All order", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dataGridViewShipping.SelectedRows.Count == 0)
            {
                return;
            }
            panelDelivery.Controls.Clear();


            EmailProperty emailProp = new EmailProperty();

            emailProp.isValid = true;

            dataGridViewShipping.SelectAll();

            emailProp.shipDate = shipCalendar.SelectionRange.Start;
            emailProp.dgvShip = dataGridViewShipping;
            emailProp.selectedRowProduct = (DataRowView)cb_product.SelectedItem;
            emailProp.selectedRowCustomer = (DataRowView)cb_customer.SelectedItem;
            emailProp = emailCls.prepareFileInfoEmail(emailProp);

            if (emailProp.isValid == false)
            {
                // ตรวจสอบว่า emailProp.strLog ไม่ใช่ null ก่อนเพิ่มลง richTextBoxLog
                if (!string.IsNullOrEmpty(emailProp.strLog))
                {
                    richTextBoxLog.AppendText(emailProp.strLog);
                }
                else
                {
                    //richTextBoxLog.AppendText("เกิดข้อผิดพลาด: ไม่มีข้อความบันทึก (Log)\n");
                }
                return;
            }

            var emailControl = new Views.emailUserControl(this);

            // Accessing dataGridViewDocs from emailUserControl
            DataGridView dataGridViewDocs = emailControl.GetDataGridViewDocs();

            // Set the dataSource of dataGridViewDocs to fileList
            dataGridViewDocs.DataSource = emailProp.dgvFile.DataSource;

            // ตรวจสอบว่า dataGridViewDocs มีคอลัมน์ "folder" หรือไม่
            if (dataGridViewDocs.Columns.Contains("folder"))
            {
                // กำหนด Visible เป็น false สำหรับคอลัมน์ "folder"
                dataGridViewDocs.Columns["folder"].Visible = false;
            }

            BindingSource bdInfo = emailControl.bdInfoEmail();
            bdInfo.DataSource = emailProp.dgvInfoEmail;

            BindingSource bdSL = emailControl.GetDataGridViewSL();
            if (emailProp.dgslFile != null)
            {
                bdSL.DataSource = emailProp.dgslFile;
            }
            else
            {
                //MessageBox.Show("The data source is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            BindingSource bdWF = emailControl.GetDataGridViewWF();
            if (emailProp.dgwfFile != null)
            {
                bdWF.DataSource = emailProp.dgwfFile;
            }
            else
            {
                //MessageBox.Show("The data source is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



            panelDelivery.Controls.Add(emailControl);

            System.Windows.Forms.Label label = emailControl.countOnLabel();
            int countRow = 0;
            if (dataGridViewDocs.Rows.Count > 0)
            {
                // มีข้อมูลใน DataGridView
                countRow = dataGridViewDocs.Rows.Count;
                //richTextBoxLog.AppendText(emailProp.strLog);

            }
            else
            {
                // ไม่มีข้อมูลใน DataGridView
                //richTextBoxLog.AppendText(emailProp.strLog);
                countRow = 0;
                //label.Text = "จำนวน : " + countRow.ToString();

            }

            label.Text = "จำนวน : " + countRow.ToString();
            //bt_selectOrder.Click -= order_multiselect_click;
            return;
        }


        private void bt_clearAll_Click(object sender, EventArgs e)
        {
            clearDataForm();
        }

        // pnClickToSetting holds the gear icon + the "Setting" label.
        // A click on a child control does not bubble up to the panel, so the icon,
        // the label and the panel itself all get the same handler.
        private void WireSettingShortcut()
        {
            Control[] parts = new Control[] { pnClickToSetting, picSetting, lbSetting };

            foreach (Control part in parts)
            {
                part.Cursor = Cursors.Hand;
                part.Click += pnClickToSetting_Click;
            }

            toolTipSetting.SetToolTip(pnClickToSetting, "Open Setting (login required)");
            toolTipSetting.SetToolTip(picSetting, "Open Setting (login required)");
            toolTipSetting.SetToolTip(lbSetting, "Open Setting (login required)");
        }

        // Setting is behind a login: employee code must be on the whitelist
        // (table user_setting_edata) before the screen opens.
        private void pnClickToSetting_Click(object sender, EventArgs e)
        {
            if (Views.frmLogin.CurrentUser == null)
            {
                using (Views.frmLogin login = new Views.frmLogin())
                {
                    if (login.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                }
            }

            using (Views.Setting.frmSetting setting = new Views.Setting.frmSetting())
            {
                setting.ShowDialog(this);
            }
        }


        private void dataGridViewShipping_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //create  0 dont send
            //create 2 ftp/sftp
            //create 3 email attach file(s)

            // โค้ดในการประมวลผลของคุณ

            if (dataGridViewShipping.SelectedRows.Count == 0)
            {
                return;
            }

            panelDelivery.Controls.Clear();

            EmailCls emailCls = new EmailCls();
            ExcelCls excelCls = new ExcelCls();

            DataRowView selectedRowProduct = (DataRowView)cb_product.SelectedItem;
            ShippingProperty shipProp = new ShippingProperty();
            ShippingControllers _shipCon = new ShippingControllers();

            EmailControllers _emailCon = new EmailControllers();
            EmailProperty emailProp = new EmailProperty();
            EmailProperty.replaceWord rpWord = new EmailProperty.replaceWord();

            SettingProperty setProp = new SettingProperty();

            if (e.RowIndex >= 0 && e.RowIndex < dataGridViewShipping.Rows.Count)
            {
                // ดึงข้อมูลจาก ComboBox cb_Product
                //panelDelivery.Controls.Clear();
                //dataItem.conn = selectedRowProduct["CONNECTION_QA_NAME"].ToString();

                //change alias ให้เป็น key_id ให้หมด โดยที่ไม่ว่าจะเป็น CusCondition_ID , ProductionInstructionNo
                shipProp.key = (dataGridViewShipping["KEY_ID", e.RowIndex].Value).ToString();
                shipProp.productName = selectedRowProduct["PRODUCT_NAME"].ToString();
                shipProp.dbProduct = GetDbProductForRun(shipProp.productName, selectedRowProduct["DATABASE_NAME"].ToString());

                string strDelv = _shipCon.getSendMethods(shipProp);


                if (strDelv == "2")
                {
                    int intSerialModule = 0;
                    //ftpUserControl

                    var ftpControl = new Views.ftpUserControl(this);
                    panelDelivery.Controls.Add(ftpControl);

                    shipProp.fecOrderNo = dataGridViewShipping["manuFactureNo", e.RowIndex].Value?.ToString();
                    //dataItem.shipdate = dataGridViewShipping["Ship_Date", e.RowIndex].Value?.ToString();
                    shipProp.testReportNo = dataGridViewShipping["TestReportNo", e.RowIndex].Value?.ToString();
                    shipProp.customerPN = dataGridViewShipping["customerPN", e.RowIndex].Value?.ToString();

                    string remoteFolderPath = "";

                    DataGridView dgvSerialShipping = ftpControl.GetDataGridViewSerialShipping();
                    DataGridView dgvCustomer = ftpControl.GetdataGridViewCustomer();

                    System.Windows.Forms.TextBox tb_pathServer = ftpControl.GetTextBoxPathServer();
                    System.Windows.Forms.Button bt_sendFTP = ftpControl.GetButtonSendFTP();
                    System.Windows.Forms.Label lb_SerialShippingCount = ftpControl.GetLabelSerialShippingCount();

                    ftpControl.dataGridViewRowShipping = dataGridViewShipping.Rows[e.RowIndex];
                    ftpControl.mainFolderPath = tb_path_shipping.Text + shipCalendar.SelectionRange.Start.ToString("yyMMdd") + "-Shipping Data" + "\\"; ;
                    
                    //เตรียมส่ง log status ไปยัง formmain
                    ftpControl.LogUpdated += AppendToRichTextBoxLog;


                    if (shipProp.customerPN.Contains(" "))
                    {
                        string[] custSplit = shipProp.customerPN.Split(' ');
                        remoteFolderPath = custSplit[0];
                    }
                    else
                    {
                        remoteFolderPath = shipProp.customerPN;
                    }
                    // ทำสิ่งที่ต้องการกับค่า alphaValue ที่ได้รับ
                    string host = "204.128.141.121";
                    int port = 22;
                    string username = "furukawaftp";
                    string password = "0fs@inf1ner@.13";


                    //for test
                    //remoteFolderPath = "fftTest";

                    //for real
                    tb_pathServer.Text = remoteFolderPath;


                    //

                    // remote folder path check 
                    // if folder exists (else create folder)
                    if (ftpControl.ReadSftpFile(host, port, username, password, remoteFolderPath) == false)
                    {
                        //return;
                        return;
                    }


                    DataTable dt = _shipCon.SearchSerialShipping(shipProp);
                    dgvSerialShipping.DataSource = dt;
                    dgvSerialShipping.Refresh();



                    foreach (DataGridViewRow row2 in dgvSerialShipping.Rows)
                    {
                        string value2 = row2.Cells[0].Value.ToString();
                        bool found = false; // ตั้งค่าเริ่มต้นว่ายังไม่เจอค่าที่ตรงกันใน dataGridView1

                        foreach (DataGridViewRow row1 in dgvCustomer.Rows)
                        {
                            string value1 = row1.Cells[0].Value.ToString();

                            if (value1.Contains(value2))
                            {
                                row1.DefaultCellStyle.BackColor = Color.Green;  // เปลี่ยนสีพื้นหลังของแถวที่ตรงกันเป็นสีเขียว
                                row2.DefaultCellStyle.BackColor = Color.Green;  // เปลี่ยนสีพื้นหลังของแถวที่ตรงกันใน dgvSerialShipping เป็นสีเขียว

                                found = true; // เมื่อเจอค่าที่ตรงกันใน dataGridView1 กำหนดให้ found เป็น true
                                break; // ออกจากลูปในกรณีที่เจอค่าที่ตรงกันแล้ว
                            }
                        }

                        if (!found)
                        {
                            row2.DefaultCellStyle.BackColor = Color.Red;  // เปลี่ยนสีพื้นหลังของแถวใน dgvSerialShipping เป็นสีแดงหากไม่เจอค่าที่ตรงกัน
                            richTextBoxLog.AppendText($"ยังไม่เคยส่ง serial : {value2}\n");
                        }
                    }

                    if (dgvSerialShipping.Rows.Count > 0)
                    {
                        // มีข้อมูลใน DataGridView
                        intSerialModule = dgvSerialShipping.Rows.Count;

                    }
                    else
                    {
                        // ไม่มีข้อมูลใน DataGridView

                        intSerialModule = 0;
                    }

                    bt_sendFTP.Enabled = true;
                    //dataGridViewShipping.Enabled = true;


                    lb_SerialShippingCount.Text = "จำนวน : " + intSerialModule.ToString();

                    if (dgvCustomer.SelectedCells.Count > 0)
                    {
                        dgvCustomer.SelectedCells[0].OwningRow.Selected = false;
                    }

                    if (dgvSerialShipping.SelectedCells.Count > 0)
                    {
                        dgvSerialShipping.SelectedCells[0].OwningRow.Selected = false;
                    }

                    return;
                }

                //else if (strDelv == "3")
                //{
                //    //key, spec ID ไปหา subject

                //    emailProp.isValid = true;
                //    dataGridViewShipping.Enabled = false;

                //    emailProp.dgvShip = dataGridViewShipping;
                //    emailProp.selectedRowProduct = (DataRowView)cb_product.SelectedItem;
                //    emailProp.selectedRowCustomer = (DataRowView)cb_customer.SelectedItem;

                //    emailProp.productName = shipProp.productName;
                //    emailProp.key = shipProp.key;
                //    emailProp.shipDate = shipCalendar.SelectionRange.Start;

                //    //Subject replace
                //    rpWord.productName = emailProp.productName;
                //    rpWord.customerPN = (dataGridViewShipping["customerPN", e.RowIndex].Value).ToString();

                //    rpWord.shipDate = shipCalendar.SelectionRange.Start;

                //    rpWord.customerName = (dataGridViewShipping["customerName", e.RowIndex].Value).ToString();
                //    rpWord.manuFactureNo = (dataGridViewShipping["manuFactureNo", e.RowIndex].Value).ToString();
                //    rpWord.testReportNo = (dataGridViewShipping["TestReportNo", e.RowIndex].Value).ToString();

                //    rpWord.formatDate = _emailCon.allFormatDate();



                //    rpWord.pathPO = selectedRowProduct["PATH_PO_ALL"].ToString();
                //    rpWord.productNameInEmail = selectedRowProduct["NAME_IN_EMAIL"].ToString();
                //    rpWord.productID = selectedRowProduct["PRODUCT_ID"].ToString();



                //    emailProp.pathShipping = tb_path_shipping.Text + shipCalendar.SelectionRange.Start.ToString("yyMMdd") + "-Shipping Data" + "\\";// + shipCalendar.SelectionRange.Start.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")";


                //    DataGridView fileList = new DataGridView();
                //    emailProp = emailCls.prepareFileInfoEmailSelection(emailProp);


                //    var emailControl = new Views.emailUserControl(this);
                //    panelDelivery.Controls.Add(emailControl);

                //    // Accessing dataGridViewDocs from emailUserControl
                //    DataGridView dataGridViewDocs = emailControl.GetDataGridViewDocs();

                //    // Set the dataSource of dataGridViewDocs to fileList
                //    dataGridViewDocs.DataSource = emailProp.dgvFile.DataSource;

                //    // ตรวจสอบว่า dataGridViewDocs มีคอลัมน์ "folder" หรือไม่
                //    if (dataGridViewDocs.Columns.Contains("folder"))
                //    {
                //        // กำหนด Visible เป็น false สำหรับคอลัมน์ "folder"
                //        dataGridViewDocs.Columns["folder"].Visible = false;
                //    }



                //}
                
                else
                {

                }
                
            }
        }



        private void order_ftp_click(object sender, EventArgs e)
        {
            dataGridViewShipping_CellDoubleClick(dataGridViewShipping, new DataGridViewCellEventArgs(0, dataGridViewShipping.CurrentCell.RowIndex));
        }

        private void cb_customer_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            DateTime selectedDate = shipCalendar.SelectionRange.Start;
            panelDelivery.Controls.Clear();

            ShippingProperty shipProp = new ShippingProperty();
            ShippingControllers _controllerShip = new ShippingControllers();

            EmailControllers _emailCon = new EmailControllers();
            EmailProperty emailProp = new EmailProperty();
            EmailProperty.replaceWord rpWord = new EmailProperty.replaceWord();

            // ดึงข้อมูลจาก ComboBox cb_Product
            emailProp.selectedRowProduct = (DataRowView)cb_product.SelectedItem;
            shipProp.conn = ConfigurationManager.ConnectionStrings["ConnectionStandAlone"].ConnectionString;

            shipProp.shipdate = selectedDate.ToString("yyyy-MM-dd");
            shipProp.productName = emailProp.selectedRowProduct["PRODUCT_NAME"].ToString();
            shipProp.dbProduct = GetDbProductForRun(shipProp.productName, emailProp.selectedRowProduct["DATABASE_NAME"].ToString());
            shipProp.productNameInEmail = emailProp.selectedRowProduct["NAME_IN_EMAIL"].ToString();


            DataRowView selectedRowCustomer = (DataRowView)cb_customer.SelectedItem;
            shipProp.customerName = selectedRowCustomer["customerName"].ToString();
            shipProp.sendMethodsID = selectedRowCustomer["SEND_METHODS_ID"].ToString();
            emailProp.fileFormatsId = selectedRowCustomer["FILE_FORMATS_ID"].ToString();

            //key, spec ID ไปหา subject
            emailProp.productName = shipProp.productName;
            emailProp.shipDate = shipCalendar.SelectionRange.Start;
               
            emailProp.pathShipping = tb_path_shipping.Text + shipCalendar.SelectionRange.Start.ToString("yyMMdd") + "-Shipping Data" + "\\";
            emailProp.folderCustomer = emailProp.pathShipping + shipCalendar.SelectionRange.Start.ToString("yyMMdd") +"-" + shipProp.customerName + "(" + shipProp.productNameInEmail + ")\\";
                
            DataTable ship = _controllerShip.SearchDataShipping(shipProp);

            //var emailControl = new Views.emailUserControl();
            //panelDelivery.Controls.Add(emailControl);

            switch (shipProp.sendMethodsID)
            {
                case "1": // Nothing
                
                    //hisilicon เปิด excel 
                    //ciena arrow เปิด app duc

                    return;

                case "2": //ftp ,sftp
                    dataGridViewShipping.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dataGridViewShipping.MultiSelect = false;
                    bt_selectOrder.Visible = true;
                    bt_selectOrder.Enabled = true;
                    multiSelectCheckBox.Enabled = false;

                    dataGridViewShipping.ReadOnly = true;
                    dataGridViewShipping.SelectionChanged -= myDataGridView_SelectionChanged;
                    dataGridViewShipping.CellDoubleClick += dataGridViewShipping_CellDoubleClick;

                    // เชื่อมโยงอีเวนต์ Click ของปุ่ม bt_selectOrder กับ order_ftp_click
                    bt_selectOrder.Click -= order_ftp_click;
                    bt_selectOrder.Click -= order_email_click;
                    bt_selectOrder.Click += order_ftp_click;

                    dataGridViewShipping.DataSource = ship;
                    return;

                case "3": // email
                    multiSelectCheckBox.Enabled = false;
                    dataGridViewShipping.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dataGridViewShipping.MultiSelect = true;
                    bt_selectOrder.Enabled = true;
                    dataGridViewShipping.ReadOnly = true;

                    dataGridViewShipping.Enabled = true;
                    dataGridViewShipping.CellDoubleClick -= dataGridViewShipping_CellDoubleClick;
                    
                    
                    bt_selectOrder.Click -= order_ftp_click;

                    if (multiSelectCheckBox.Checked == true)
                    {
                        bt_selectOrder.Click -= order_email_click;
                        bt_selectOrder.Click -= order_multiselect_click;
                        bt_selectOrder.Click += order_email_click;

                        dataGridViewShipping.DataSource = ship;
                        dataGridViewShipping.SelectionChanged += myDataGridView_SelectionChanged;
                        dataGridViewShipping.SelectAll();
                        emailProp.dtShip = ship;


                    }
                    else
                    {
                        bt_selectOrder.Click -= order_multiselect_click;
                        bt_selectOrder.Click -= order_email_click;
                        bt_selectOrder.Click += order_multiselect_click;

                        dataGridViewShipping.DataSource = ship;
                        dataGridViewShipping.SelectionChanged -= myDataGridView_SelectionChanged;
                        emailProp.dtShip = ship;
                    }

                    foreach (DataGridViewColumn column in dataGridViewShipping.Columns)
                    {
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }

                    //dataGridViewShipping.DataSource = ship;
                    //multiSelectCheckBox.Enabled = true;
                    //multiSelectCheckBox.Checked = true;
                    //dataGridViewShipping.SelectAll();
                    //emailProp.dtShip = ship;


                    return;

                default:

                    break;
            }
        }



        private void myDataGridView_SelectionChanged(Object sender, EventArgs e)
        {
            // เลือกทั้งหมด
            dataGridViewShipping.SelectionChanged -= myDataGridView_SelectionChanged; // ป้องกัน loop ที่ไม่มีที่สิ้นสุด
            dataGridViewShipping.SelectAll();
            dataGridViewShipping.SelectionChanged += myDataGridView_SelectionChanged;
        }

        private void myDataGridView_MultiSelectionChanged(Object sender, EventArgs e)
        {
            // Check if all rows are selected
            if (dataGridViewShipping.SelectedRows.Count == dataGridViewShipping.RowCount)
            {
                // Set CheckBox to true if all rows are selected
                //multiSelectCheckBox.Checked = true;
            }
            
        }

        private void UnselectAllRows(object sender, EventArgs e)
        {


            foreach (DataGridViewRow row in dataGridViewShipping.Rows)
            {

                row.Selected = false;
            }
        }

        private void bdShipping_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void multiSelectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
           // System.Windows.Forms.CheckBox checkBox = sender as System.Windows.Forms.CheckBox;
            if (multiSelectCheckBox != null)
            {
                if (multiSelectCheckBox.Checked == true)
                {
                    // เปิดโหมดเลือกทั้งหมด
                    dataGridViewShipping.MultiSelect = true;
                    //dataGridViewShipping.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                    dataGridViewShipping.SelectionChanged -= myDataGridView_SelectionChanged;
                    dataGridViewShipping.SelectionChanged += myDataGridView_SelectionChanged;
                    dataGridViewShipping.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                    dataGridViewShipping.SelectAll();
                    panelDelivery.Controls.Clear();

                    bt_selectOrder.Click -= order_email_click;
                    bt_selectOrder.Click -= order_multiselect_click;
                    bt_selectOrder.Click -= order_ftp_click;
                    bt_selectOrder.Click += order_email_click;
                    

                }
                else
                {
                    // เปิดโหมดการเลือกหลายรายการ
                    dataGridViewShipping.MultiSelect = true;
                    //dataGridViewShipping.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dataGridViewShipping.SelectionChanged -= myDataGridView_SelectionChanged;
                    dataGridViewShipping.SelectionChanged -= myDataGridView_MultiSelectionChanged;
                    dataGridViewShipping.SelectionChanged += myDataGridView_MultiSelectionChanged;

                    bt_selectOrder.Enabled = true;
                    dataGridViewShipping.ReadOnly = true;
                    dataGridViewShipping.Enabled = true;
                    dataGridViewShipping.CellDoubleClick -= dataGridViewShipping_CellDoubleClick;
                    dataGridViewShipping.SelectionChanged -= myDataGridView_SelectionChanged;
                    dataGridViewShipping.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                    panelDelivery.Controls.Clear();
                    dataGridViewShipping.ClearSelection();
                    //dataGridViewShipping.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                    bt_selectOrder.Click -= order_multiselect_click;
                    bt_selectOrder.Click -= order_ftp_click;
                    bt_selectOrder.Click -= order_email_click;
                    bt_selectOrder.Click += order_multiselect_click;


                }
            }
        }


        private void order_multiselect_click(Object sender, EventArgs e)
        {

            panelDelivery.Controls.Clear();
            //เงื่อนไขคือ ถ้าอ่านแล้วว่า datagrtid เลือกหมด จะให้เลือกใช้ 

            if (dataGridViewShipping.SelectedRows.Count == 0)
            {
                MessageBox.Show("เลือกส่งอย่างน้อย 1 test report ถ้าเลือกหมด กรุณา click All order", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dataGridViewShipping.SelectedRows.Count == 0)
            {
                return;
            }

            if ((dataGridViewShipping.SelectedRows.Count == dataGridViewShipping.Rows.Count) & (multiSelectCheckBox.Checked == false))
            {
                MessageBox.Show("กรุณาเลือกเป็นบาง order ถ้าเลือกหมด กรุณา click All order", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }


            EmailProperty emailProp = new EmailProperty();

            emailProp.isValid = true;

            emailProp.shipDate = shipCalendar.SelectionRange.Start;
            emailProp.dgvShip = dataGridViewShipping;
            emailProp.selectedRowProduct = (DataRowView)cb_product.SelectedItem;
            emailProp.selectedRowCustomer = (DataRowView)cb_customer.SelectedItem;
            emailProp = emailCls.prepareFileInfoEmailSelection(emailProp);

            if (emailProp.isValid == false)
            {
                return;
            }


            var emailControl = new Views.emailUserControl(this);

            // Accessing dataGridViewDocs from emailUserControl
            DataGridView dataGridViewDocs = emailControl.GetDataGridViewDocs();

            // Set the dataSource of dataGridViewDocs to fileList
            dataGridViewDocs.DataSource = emailProp.dgvFile.DataSource;

            // ตรวจสอบว่า dataGridViewDocs มีคอลัมน์ "folder" หรือไม่
            if (dataGridViewDocs.Columns.Contains("folder"))
            {
                // กำหนด Visible เป็น false สำหรับคอลัมน์ "folder"
                dataGridViewDocs.Columns["folder"].Visible = false;
            }

            BindingSource bdInfo = emailControl.bdInfoEmail();
            bdInfo.DataSource = emailProp.dgvInfoEmail;

            BindingSource bdSL = emailControl.GetDataGridViewSL();
            if (emailProp.dgslFile != null)
            {
                bdSL.DataSource = emailProp.dgslFile;
            }
            else
            {
                //MessageBox.Show("The data source is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            BindingSource bdWF = emailControl.GetDataGridViewWF();
            if (emailProp.dgwfFile != null)
            {
                bdWF.DataSource = emailProp.dgwfFile;
            }
            else
            {
                //MessageBox.Show("The data source is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



            panelDelivery.Controls.Add(emailControl);

            System.Windows.Forms.Label label = emailControl.countOnLabel();
            int countRow = 0;
            if (dataGridViewDocs.Rows.Count > 0)
            {
                // มีข้อมูลใน DataGridView
                countRow = dataGridViewDocs.Rows.Count;
                //richTextBoxLog.AppendText(emailProp.strLog);

            }
            else
            {
                // ไม่มีข้อมูลใน DataGridView
                //richTextBoxLog.AppendText(emailProp.strLog);
                countRow = 0;
                //label.Text = "จำนวน : " + countRow.ToString();

            }

            label.Text = "จำนวน : " + countRow.ToString();
            //bt_selectOrder.Click -= order_email_click;
            return;
        }

    }
}

