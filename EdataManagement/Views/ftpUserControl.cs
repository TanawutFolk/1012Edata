using EdataManagement.Property;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Controls;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EdataManagement.Views
{
    public partial class ftpUserControl : System.Windows.Forms.UserControl
    {
        private frmMain mf;

        public DataGridViewRow dataGridViewRowShipping;

        public string mainFolderPath;
        public RichTextBox rtbLog;
        // ประกาศ event
        public event Action<string> LogUpdated;

        public ftpUserControl(frmMain mf)
        {
            this.mf = mf;
            InitializeComponent();
        }

        

        public DataGridView GetdataGridViewCustomer()
        {

            return dataGridViewCustomer;
        }

        public DataGridView GetDataGridViewSerialShipping()
        {
            return dataGridViewSerialShipping;
        }

        public System.Windows.Forms.TextBox GetTextBoxSearchFile()
        {
            return tb_searchFile;
        }

        public System.Windows.Forms.TextBox GetTextBoxPathServer()
        {
            return tb_pathServer;
        }

        public System.Windows.Forms.Button GetButtonSendFTP()
        {
            return bt_sendFTP;
        }

        public System.Windows.Forms.Label GetLabelSerialShippingCount()
        {
            return lb_SerialShippingCount;
        }
        
  

        public bool ReadSftpFile(string host, int port, string username, string password, string remoteFolderPath)
        {
            try
            {
                //string custPartNo = tb_cusPartNo.Text;
                // Create ConnectionInfo with host, port, username, and password
                // ConnectionInfo connectionInfo = new ConnectionInfo();
                // Create SFTP client
                //SftpClient client = new SftpClient(host, port, username, password);
                using (SftpClient client = new SftpClient(host, port, username, password))
                {


                    // Connect to SFTP server
                    client.Connect();

                    string currentDirectory = client.WorkingDirectory;
                    //var fileEntries = client.ListDirectory(remoteFolderPath);

                    string fullPath = remoteFolderPath;
                    if (!remoteFolderPath.StartsWith("/"))
                    {
                        fullPath = $"{currentDirectory}/{remoteFolderPath}";
                    }

                    // Check if the folder exists
                    if (!client.Exists(fullPath))
                    {
                        client.CreateDirectory(fullPath);
                        //richTextBoxLog.AppendText($"Created folder: {remoteFolderPath}\n");

                    }
                    else
                    {
                        //richTextBoxLog.AppendText($"Folder already exists: {remoteFolderPath}\n");

                    }

                    var fileEntries = client.ListDirectory(fullPath);

                    // สร้าง DataTable เพื่อเก็บข้อมูลชื่อไฟล์
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("File");

                    // วนลูปเพื่อเพิ่มชื่อไฟล์ลงใน DataTable
                    foreach (var fileEntry in fileEntries)
                    {
                        if (!fileEntry.IsDirectory)
                        {
                            dataTable.Rows.Add(fileEntry.Name);
                        }
                    }

                    // กำหนด DataTable เป็น DataSource ของ DataGridView
                    dataGridViewCustomer.DataSource = dataTable;
                    dataGridViewCustomer.Refresh();

                    if (dataGridViewCustomer != null && dataGridViewCustomer.Rows.Count > 0)
                    {
                        tb_searchFile.Enabled = true;
                    }
                    else
                    {
                        tb_searchFile.Enabled = false;
                    }

                    bdFileCustomer.DataSource = dataTable;

                    //MessageBox.Show("File read successfully.");
                    client.Disconnect();

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading file: {ex.Message}");
                return false;
            }
        }

        public void setDataGridView(DataGridView dataGridView)
        {
            //dataGridViewInUsercontrol.
        }

        private void tb_searchFile_TextChanged(object sender, EventArgs e)
        {
            string searchText = tb_searchFile.Text.Trim().ToLower();
            DataTable dt = ((DataTable)bdFileCustomer.DataSource);



            if (!string.IsNullOrEmpty(searchText))
            {
                string filterExpression = string.Empty;
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.DataType == typeof(string))
                    {
                        if (!string.IsNullOrEmpty(filterExpression))
                        {
                            filterExpression += " OR ";
                        }
                        filterExpression += string.Format("{0} LIKE '%{1}%'", column.ColumnName, searchText);
                    }
                }

                DataRow[] filteredRows = dt.Select(filterExpression);
                if (filteredRows.Length > 0)
                {
                    DataTable filteredTable = filteredRows.CopyToDataTable();
                    dataGridViewCustomer.DataSource = filteredTable;

                    // กำหนดสีใหม่ให้กับ DataGridView ที่ตรงกับการค้นหา
                    foreach (DataGridViewRow row in dataGridViewCustomer.Rows)
                    {
                        //row.DefaultCellStyle.BackColor = Color.Green; // กำหนดสีเขียวให้กับแถวที่ตรงกับการค้นหา
                    }

                }
                else
                {
                    dataGridViewCustomer.DataSource = null;
                }
            }
            else
            {
                dataGridViewCustomer.DataSource = bdFileCustomer;
            }
        }

        //private DataTable originalDataTable; // ตัวแปรเก็บ DataTable ต้นฉบับ
        private Dictionary<int, Color> rowColors = new Dictionary<int, Color>(); // ตัวแปรเก็บสีของแถว

        // ใน ftpControl UserControl
        public void UpdateRowColors(Dictionary<int, Color> rowColors)
        {
            foreach (var rowColor in rowColors)
            {
                if (rowColor.Key >= 0 && rowColor.Key < dataGridViewSerialShipping.Rows.Count)
                {
                    dataGridViewSerialShipping.Rows[rowColor.Key].DefaultCellStyle.BackColor = rowColor.Value;
                }
            }
        }

        private void RestoreRowColors()
        {
            foreach (DataGridViewRow row in dataGridViewCustomer.Rows)
            {
                row.DefaultCellStyle.BackColor = rowColors[row.Index];
            }
        }

        private void bt_sendFTP_Click(object sender, EventArgs e)
        {
            //เปลี่ยนเป็นรับมาตั้งแต่ main ปุ่ม confirm เลย
            //DateTime selectedDate = shipCalendar.SelectionRange.Start;
            try
            {
                ShippingProperty dataItem = new ShippingProperty();
                //string mainFolderPath = @"\\office_SRV\C06_Shipping-Data\1480LDM\" + selectedDate.ToString("yyyy") + @"\" + selectedDate.ToString("yyMMdd") + "-Shipping Data";
                //string mainFolderPath = "";


                if (dataGridViewRowShipping.DataGridView.Rows.Count > 0)
                {
                    DataGridViewRow selectedRow = dataGridViewRowShipping.DataGridView.Rows[0];
                    string remoteFolderPath = tb_pathServer.Text;

                    dataItem.customerName = selectedRow.Cells["CustomerName"].Value?.ToString();
                    dataItem.fecOrderNo = selectedRow.Cells["manuFactureNo"].Value?.ToString();
                    string[] subfolders = Directory.GetDirectories(mainFolderPath, "*", SearchOption.AllDirectories);
                    //string targetFolder = null;



                    //เตรียม serial
                    List<string> targetFileNames = new List<string>();
                    foreach (DataGridViewRow row in dataGridViewSerialShipping.Rows)
                    {
                        string fileName = row.Cells["Serial"].Value?.ToString();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            targetFileNames.Add(fileName);
                        }
                    }

                    List<string> FindTarget = new List<string>();
                    foreach (string subfolder in subfolders)
                    {
                        string folderName = Path.GetFileName(subfolder);

                        //// หา sub-folder ที่ชื่อลูกค้า
                        if (folderName.Contains(dataItem.customerName))
                        {
                            //target คือ targetFileNames => serial.txt ทั้งหมดใน gridview dataGridViewSerialShipping
                            FindTarget = FindTargetFiles(subfolder, targetFileNames, dataItem); // หาใน subfolder ที่ชื่อ dataItem อะไรก็ตามที่อยู่ในนี้(serial.txt)
                            break;
                        }
                    }

                    if (FindTarget != null)
                    {
                        // อ่านชื่อไฟล์ .txt และแสดงใน Richtextbox
                        //string[] txtFiles = Directory.GetFiles(targetFolder, "*.txt");

                        foreach (string txtFile in FindTarget)
                        {
                            string fileName = Path.GetFileName(txtFile);
                            //richTextBoxLog.AppendText($"Found .txt file: {fileName}\n");
                            //richTextBoxLog.AppendText($"Found .txt file: {txtFile}\n");
                            //fileList.Add(txtFile);
                        }
                    }

                    string host = "204.128.141.121";
                    int port = 22;
                    string username = "furukawaftp";
                    string password = "0fs@inf1ner@.13";

                    List<string> uploadStatusList = UploadFiles(host, port, username, password, remoteFolderPath, FindTarget);

                    foreach (string status in uploadStatusList)
                    {
                        // Trigger the event and pass the status
                        LogUpdated?.Invoke(status);
                        //rtbLog.AppendText($"{status}\n");
                    }

                    
                    if (ReadSftpFile(host, port, username, password, remoteFolderPath) == false)
                    {
                        //return;
                        return;
                    }


                    int intSerialModule = 0;


                    foreach (DataGridViewRow row2 in dataGridViewSerialShipping.Rows)
                    {
                        string value2 = row2.Cells[0].Value.ToString();
                        bool found = false; // ตั้งค่าเริ่มต้นว่ายังไม่เจอค่าที่ตรงกันใน dataGridView1

                        foreach (DataGridViewRow row1 in dataGridViewCustomer.Rows)
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
                            LogUpdated?.Invoke($"ยังไม่เคยส่ง serial : {value2}\n");
                        }
                    }

                    if (dataGridViewSerialShipping.Rows.Count > 0)
                    {
                        // มีข้อมูลใน DataGridView
                        intSerialModule = dataGridViewSerialShipping.Rows.Count;

                    }
                    else
                    {
                        // ไม่มีข้อมูลใน DataGridView

                        intSerialModule = 0;
                    }

                    bt_sendFTP.Enabled = true;
                    //dataGridViewShipping.Enabled = true;


                    lb_SerialShippingCount.Text = "จำนวน : " + intSerialModule.ToString();

                    if (dataGridViewCustomer.SelectedCells.Count > 0)
                    {
                        dataGridViewCustomer.SelectedCells[0].OwningRow.Selected = false;
                    }

                    if (dataGridViewSerialShipping.SelectedCells.Count > 0)
                    {
                        dataGridViewSerialShipping.SelectedCells[0].OwningRow.Selected = false;
                    }

                    return;

                }

                //dataGridViewShipping_CellDoubleClick(dataGridViewShipping, new DataGridViewCellEventArgs(0, dataGridViewShipping.CurrentCell.RowIndex));


                //bt_sendFTP.Enabled = false;

                //this.Parent.Controls.Remove(this);
                //mf.clearDataForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }
            finally
            {
                //this.Parent.Controls.Remove(this);
                //mf.clearDataForm();
            }
            //dataGridViewShipping.Enabled = true;
            //bt_selectOrder.Enabled = true;

        }

        public List<string> FindTargetFiles(string mainFolderPath, List<string> targetFileNames, ShippingProperty dataItem)
        {
            List<string> foundFilePaths = new List<string>();

            // ค้นหา sub-folders ทั้งหมด
            string[] subfolders = Directory.GetDirectories(mainFolderPath, "*", SearchOption.AllDirectories);

            foreach (string subfolder in subfolders)
            {
                string folderName = Path.GetFileName(subfolder);

                //if (folderName.Contains(dataItem.fecOrderNo))
                //{
                foreach (string targetFileName in targetFileNames)
                {
                    string filePath = Path.Combine(subfolder, targetFileName + ".txt");

                    if (System.IO.File.Exists(filePath))
                    {
                        // หากเจอไฟล์ที่ต้องการใน folder นี้ กำหนด targetFolder และหยุด loop
                        foundFilePaths.Add(filePath);
                    }
                }
                //}
            }

            return foundFilePaths;
        }

        private List<string> UploadFiles(string host, int port, string username, string password, string remoteFolderPath, List<string> fileList)
        {
            List<string> uploadStatusList = new List<string>();

            using (SftpClient client = new SftpClient(host, port, username, password))
            {
                client.Connect();

                if (!client.Exists(remoteFolderPath))
                {
                    client.CreateDirectory(remoteFolderPath);
                }

                // ส่งทีละไฟล์
                foreach (string filePath in fileList)
                {
                    try
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Open))
                        {
                            string remoteFile = remoteFolderPath + "/" + Path.GetFileName(filePath);
                            client.UploadFile(fileStream, remoteFile);

                            uploadStatusList.Add($"Uploaded: {filePath} to {remoteFile}");
                        }
                    }
                    catch (Exception ex)
                    {
                        uploadStatusList.Add($"Failed to upload: {filePath}. Error: {ex.Message}");
                    }
                }

                client.Disconnect();
            }

            return uploadStatusList;
        }

        private void dataGridViewSerialShipping_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewSerialShipping.Rows[e.RowIndex].Selected = false;
            }
        }

        private void dataGridViewCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewCustomer.Rows[e.RowIndex].Selected = false;
            }
        }


        //เหตุการณ์ที่ click/double click dataGridViewSerialShipping and dataGridViewCustomer ให้ไม่เกิดการเลือก
        private void dataGridViewSerialShipping_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewSerialShipping.Rows[e.RowIndex].Selected = false;
            }
        }

        private void dataGridViewSerialShipping_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewSerialShipping.Rows[e.RowIndex].Selected = false;
            }
        }

        private void dataGridViewCustomer_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewCustomer.Rows[e.RowIndex].Selected = false;
            }
        }

        private void dataGridViewCustomer_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewCustomer.Rows[e.RowIndex].Selected = false;
            }
        }

        private void lb_pathServer_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void tb_pathServer_TextChanged(object sender, EventArgs e)
        {

        }

        //public string FindTargetFolder(string startFolder , List<string> targetFileNames, ShippingProperty dataItem)
        //{
        //    string targetFolder = null;

        //    // ตรวจสอบว่ามีไฟล์ .txt อยู่ใน folder นี้หรือไม่
        //    string[] txtFiles = Directory.GetFiles(startFolder, "*.txt");
        //    if (txtFiles.Length > 0)
        //    {
        //        return startFolder; // หยุดการทำงานและส่งค่ากลับ
        //    }

        //    // หา sub-folders
        //    string[] innerSubfolders = Directory.GetDirectories(startFolder);
        //    foreach (string innerSubfolder in innerSubfolders)
        //    {
        //        string innerFolderName = Path.GetFileName(innerSubfolder);
        //        if (innerFolderName.Contains(dataItem.fecOrderNo))
        //        {
        //            targetFolder = FindTargetFolder(innerSubfolder , targetFileNames,  dataItem); // เรียกตัวเอง
        //            if (targetFolder != null)
        //            {
        //                break; // หยุด loop เมื่อเจอ folder ที่ตรงตามเงื่อนไข
        //            }
        //        }
        //    }

        //    return targetFolder;
        //}

    }
}
