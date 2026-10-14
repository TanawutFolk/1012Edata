using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using EdataManagement.Property;
using SevenZipExtractor;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Security.Cryptography.Xml;
using EdataManagement.Controllers;
using System.ComponentModel;
using System.Windows.Controls;
using Renci.SshNet.Messages;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Net.WebRequestMethods;
using File = System.IO.File;


namespace EdataManagement.Utilities
{
    public class EmailCls
    {
       
        ExcelProperty excelProp = new ExcelProperty();
        ExcelCls excelCls = new ExcelCls();
        
        EmailControllers _emailCon = new EmailControllers();

        ShippingProperty shipProp = new ShippingProperty();
        ShippingControllers _shipCon = new ShippingControllers();

        FileCls fileCls = new FileCls();

        private const string Case10SpecialPOAllPath = @"\\192.168.2.197\Butterfly1480\10-48-ProductControl\07-Shipping_PO\PO from FEC\Shipping-PO-all.xlsm";

        // info_document ids of the E-data types that go out as their own mail
        private const int DOC_SERIAL_LIST = 2;
        private const int DOC_WAFER = 3;

        /// <summary>
        /// TO / CC of a Serial List or Wafer mail: key_group_email_document in the database first
        /// (editable in Setting, same on every PC); Setting\EmailSetting.ini only when the database
        /// has nothing for this product + customer + type yet.
        /// </summary>
        private List<string> GetDocumentRecipients(ShippingProperty ship, int documentId, string iniSuffix, string recipientType)
        {
            List<string> fromDatabase = _emailCon.getDocumentEmailAddresses(ship.productName, ship.customerName, documentId, recipientType);
            if (fromDatabase != null) return fromDatabase;

            return fileCls.GetEmailAddresses(ship.productName + ship.customerName + iniSuffix, recipientType);
        }

        private string GetCellValue(DataGridViewRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.DataGridView.Columns.Contains(columnName))
                {
                    return row.Cells[columnName].Value?.ToString()?.Trim() ?? "";
                }
            }

            return "";
        }

        private string GetPOAllValue(string productID, string pathPO, string variable, string input)
        {
            if (string.IsNullOrEmpty(productID) || string.IsNullOrEmpty(pathPO) ||
                string.IsNullOrEmpty(variable) || string.IsNullOrEmpty(input))
            {
                return "";
            }

            ExcelProperty poAllProp = new ExcelProperty();
            poAllProp.productID = productID;
            poAllProp.pathPOall = pathPO;
            poAllProp.VARIABLE = variable;
            poAllProp.input = input;

            return excelCls.ExcelValue(poAllProp)?.Trim() ?? "";
        }

        /// <summary>
        /// PO part of the case 10 file name yyMMdd-{PO}(short PN).xls, from Shipping-PO-all column V.
        /// The file name uses column V as it is: XTERA-INC "PO-0000032335", SMTC-CORP "SJ 70320745".
        /// </summary>
        private string GetCase10SpecialPO(EmailProperty.replaceWord rpWord, string testReportNo)
        {
            string specialPO = GetCase10ExcelValueByColumns(Case10SpecialPOAllPath, "N", "V", testReportNo);
            return NormalizeSpecialPO(specialPO);
        }

        private string GetCase10ExcelValueByColumns(string pathPOall, string keyColumn, string valueColumn, string input)
        {
            Microsoft.Office.Interop.Excel.Application xlApp = null;
            Microsoft.Office.Interop.Excel.Workbook xlWorkbook = null;
            Microsoft.Office.Interop.Excel.Worksheet xlWorksheet = null;
            Microsoft.Office.Interop.Excel.Range xlRange = null;

            try
            {
                if (string.IsNullOrEmpty(pathPOall) || string.IsNullOrEmpty(keyColumn) ||
                    string.IsNullOrEmpty(valueColumn) || string.IsNullOrEmpty(input))
                {
                    return "";
                }

                xlApp = new Microsoft.Office.Interop.Excel.Application();
                xlApp.Visible = false;
                xlApp.DisplayAlerts = false;

                xlWorkbook = xlApp.Workbooks.Open(pathPOall, Type.Missing, true);
                xlWorksheet = xlWorkbook.Sheets[1];
                xlRange = xlWorksheet.UsedRange;

                var keyColumnRange = xlRange.Columns[Case10ExcelColumnNameToIndex(keyColumn), Type.Missing];
                var keyCell = keyColumnRange.Find(input, Type.Missing, Microsoft.Office.Interop.Excel.XlFindLookIn.xlValues, Microsoft.Office.Interop.Excel.XlLookAt.xlWhole, Microsoft.Office.Interop.Excel.XlSearchOrder.xlByRows, Microsoft.Office.Interop.Excel.XlSearchDirection.xlNext, false, Type.Missing, Type.Missing);

                if (keyCell != null)
                {
                    int rowFound = keyCell.Row;
                    return (xlRange.Cells[rowFound, Case10ExcelColumnNameToIndex(valueColumn)] as Microsoft.Office.Interop.Excel.Range).Text?.ToString() ?? "";
                }

                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return "";
            }
            finally
            {
                if (xlRange != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(xlRange);
                if (xlWorksheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet);
                if (xlWorkbook != null)
                {
                    xlWorkbook.Close(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkbook);
                }
                if (xlApp != null)
                {
                    xlApp.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                }
            }
        }

        private int Case10ExcelColumnNameToIndex(string columnName)
        {
            int index = 0;

            foreach (char c in columnName)
            {
                index = index * 26 + c - 'A' + 1;
            }

            return index;
        }
        /// <summary>
        /// Column V as it is ("PO-0000032335", "SJ 70320745"). Only a bare PO number
        /// ("32335", "32335.0", "PO32335", "PO-32335") is rewritten to "PO-" + 10 digits,
        /// which is how those rows were named before.
        /// </summary>
        private string NormalizeSpecialPO(string specialPO)
        {
            specialPO = specialPO?.Trim() ?? "";

            if (specialPO.EndsWith(".0", StringComparison.OrdinalIgnoreCase))
            {
                specialPO = specialPO.Substring(0, specialPO.Length - 2);
            }

            System.Text.RegularExpressions.Match number =
                System.Text.RegularExpressions.Regex.Match(specialPO, @"^(?:PO\s*-?\s*)?(\d+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (number.Success)
            {
                string digits = number.Groups[1].Value;
                return "PO-" + (digits.Length < 10 ? digits.PadLeft(10, '0') : digits);
            }

            return specialPO;
        }
        public string strProcessSubject(EmailProperty.replaceWord itemData)
        {

            if (itemData.subject.IndexOf("ManufactureNo", StringComparison.OrdinalIgnoreCase) >= 0)
            // ตรวจสอบและแทนที่ "ManufactureNo" 
            {
                itemData.subject = ReplaceCaseInsensitive(itemData.subject, "ManufactureNo", itemData.manuFactureNo);
            }

            if (itemData.subject.IndexOf("FECOrderNumber", StringComparison.OrdinalIgnoreCase) >= 0)
            // ตรวจสอบและแทนที่ "FECOrderNumber" 
            {
                itemData.subject = ReplaceCaseInsensitive(itemData.subject, "FECOrderNumber", itemData.manuFactureNo);
            }

            if (itemData.subject.IndexOf("ProductName", StringComparison.OrdinalIgnoreCase) >= 0)
            // ตรวจสอบและแทนที่ "ProductName" 
            {
                itemData.subject = ReplaceCaseInsensitive(itemData.subject, "ProductName", itemData.productName);
            }

            if (itemData.subject.IndexOf("CustomerName", StringComparison.OrdinalIgnoreCase) >= 0)
            // ตรวจสอบและแทนที่ "CustomerName" ด้วยชื่อผู้ใช้
            {
                itemData.subject = ReplaceCaseInsensitive(itemData.subject, "CustomerName", itemData.customerName);
            }

            if (itemData.subject.IndexOf("CustomerPN", StringComparison.OrdinalIgnoreCase) >= 0)
            // ตรวจสอบและแทนที่ "CustomerPN" ด้วยชื่อผู้ใช้
            {
                itemData.subject = ReplaceCaseInsensitive(itemData.subject, "CustomerPN", itemData.customerPN);
            }

            // ตรวจสอบว่า subject มี "Shipdate" โดยไม่คำนึงถึงตัวพิมพ์ใหญ่หรือตัวพิมพ์เล็ก
            if (itemData.subject.IndexOf("HAWBNo", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                //รับดาต้ามาจาก email ที่จะ replace เผื่อมา action poall excel
                excelProp.productID = itemData.productID;
                excelProp.VARIABLE = "HAWBNo";
                excelProp.pathPOall = itemData.pathPO;
                
                //ใช้ test report ในการป้อน input เพื่อหา row เดียว แล้ว return HAWBNo
                excelProp.input = itemData.testReportNo;
                itemData.HAWBNo = excelCls.ExcelValue(excelProp);

                if (itemData.HAWBNo == "")
                {
                    itemData.subject = "";
                    MessageBox.Show("Have no data HAWBNo in Shipping PO All", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return itemData.subject;
                    
                }    

                itemData.subject = ReplaceCaseInsensitive(itemData.subject, "HAWBNo", itemData.HAWBNo);
                

            }

            if (itemData.subject.IndexOf("ShipDate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                itemData.subject = ProcessShipdate(itemData.subject, itemData.shipDate, itemData.formatDate);

            }

            if (itemData.subject.IndexOf("Qty", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                itemData.subject = ReplaceCaseInsensitive(itemData.subject, "Qty", itemData.Qty);
            }

            //if (itemData.subject.IndexOf("ThisCount", StringComparison.OrdinalIgnoreCase) >= 0)
            //{
            //    itemData.subject = ReplaceCaseInsensitive(itemData.subject, "ThisCount", itemData.thisCount);
            //}

            //if (itemData.subject.IndexOf("TotalCount", StringComparison.OrdinalIgnoreCase) >= 0)
            //{
            //    itemData.subject = ReplaceCaseInsensitive(itemData.subject, "TotalCount", itemData.totalCount);
            //}



            return itemData.subject;
        }

        public string strProcessBody(EmailProperty.replaceWord itemData)
        {

            if (itemData.body.IndexOf("ManufactureNo", StringComparison.OrdinalIgnoreCase) >= 0)
            // ตรวจสอบและแทนที่ "ManufactureNo" 
            {
                itemData.body = ReplaceCaseInsensitive(itemData.body, "ManufactureNo", itemData.manuFactureNo);
            }

            if (itemData.body.IndexOf("FECOrderNumber", StringComparison.OrdinalIgnoreCase) >= 0)
            // ตรวจสอบและแทนที่ "FECOrderNumber" 
            {
                itemData.body = ReplaceCaseInsensitive(itemData.body, "FECOrderNumber", itemData.manuFactureNo);
            }

            if (itemData.body.IndexOf("ProductName", StringComparison.OrdinalIgnoreCase) >= 0)
            // ตรวจสอบและแทนที่ "ProductName" 
            {
                itemData.body = ReplaceCaseInsensitive(itemData.body, "ProductName", itemData.productName);
            }

            if (itemData.body.IndexOf("CustomerName", StringComparison.OrdinalIgnoreCase) >= 0)
            // ตรวจสอบและแทนที่ "CustomerName" ด้วยชื่อผู้ใช้
            {
                itemData.body = ReplaceCaseInsensitive(itemData.body, "CustomerName", itemData.customerName);
            }

            if (itemData.body.IndexOf("CustomerPN", StringComparison.OrdinalIgnoreCase) >= 0)
            // ตรวจสอบและแทนที่ "CustomerPN" ด้วยชื่อผู้ใช้
            {
                itemData.body = ReplaceCaseInsensitive(itemData.body, "CustomerPN", itemData.customerPN);
            }

            if (itemData.body.IndexOf("ShipDate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                itemData.body = ProcessShipdate(itemData.body, itemData.shipDate, itemData.formatDate);

            }

            if (itemData.body.IndexOf("FolderName", StringComparison.OrdinalIgnoreCase) >= 0)
            // ตรวจสอบและแทนที่ "FolderName" ด้วยชื่อผู้ใช้
            {
                //210205-OPLINK(98)
                //string firstFolder = tb_path_shipping.Text + shipCalendar.SelectionRange.Start.ToString("yyMMdd") + "-Shipping Data" + "\\" + shipCalendar.SelectionRange.Start.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")";

                //วนดู 7B-xxxx(PartNoL7) เดียวกันว่ามีกี่ folder    foreach {a
                //   itemData.FolderName = a + "<BR>"
                //}    
                //ManufactureNo(PartNoL7)-xxx 
                
                

                //using (SevenZipExtractor.ArchiveFile archive = new SevenZipArchive.Open("archive.7z"))
                //{
                //    foreach (SevenZipEntry entry in extractor.Entries)
                //    {
                //        // แสดงชื่อไฟล์
                //        Console.WriteLine(entry.FileName);
                //    }
                //}

               // string firstFolder = itemData.folderCustomer; //
                //string folderpattern = @"\d{6}-7B-\w+";
                //Console.WriteLine("Matching folders:");
                
                //string[] matchingFolders = GetMatchingFolders(itemData.folderCustomer, folderpattern);

                //var filteredFolders = matchingFolders
                //    .Where(folder => System.Text.RegularExpressions.Regex.IsMatch(itemData.folderCustomer, folderpattern))
                //    .Select(folder => System.Text.RegularExpressions.Regex.Match(itemData.folderCustomer, folderpattern).Value);

                //var filteredFolders = matchingFolders.Where(folder => System.Text.RegularExpressions.Regex.IsMatch(folder, @"7B-\w+.*"));

                //Console.WriteLine("Matching folders:");

                //foreach (string folder in matchingFolders)
                //{

                //    if (System.Text.RegularExpressions.Regex.IsMatch(folder, folderpattern))
                //    {
                //        int bIndex = folder.IndexOf("7B-");

                //        // ถ้าเจอ "7B-"
                //        if (bIndex != -1)
                //        {
                //            string bSubstring = folder.Substring(bIndex);
                //            //Console.WriteLine(folder);
                //            itemData.folderName += bSubstring + "<BR>";
                //        }
                //    }
                //}


                //itemData.folderName;

                itemData.body = ReplaceCaseInsensitive(itemData.body, "FolderName", itemData.folderName);
            }

            return itemData.body;
        }

        private string FormatShipDate(DateTime shipDate, DataTable formats, string formatToCheck)
        {
            foreach (DataRow row in formats.Rows)
            {
                string format = row["DATE_FORMAT"].ToString();

                if (string.Equals(format, formatToCheck, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        return shipDate.ToString(format);
                    }
                    catch (FormatException)
                    {
                        // กรณีรูปแบบไม่ถูกต้อง
                        MessageBox.Show($"Invalid format: {format}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return "";
                    }
                }
            }

            return "";
        }

        private string ProcessShipdate(string strSubject, DateTime shipDate, DataTable formats)
        {
            // ตรวจสอบว่า strSubject มีคำว่า "Shipdate" หรือไม่
            if (strSubject.IndexOf("ShipDate", StringComparison.OrdinalIgnoreCase)>=0)
            {
                // ตรวจสอบว่า strSubject มีในรูปแบบที่ต้องการหรือไม่
                foreach (DataRow row in formats.Rows)
                {
                    string formatToCheck = row["DATE_FORMAT"].ToString(); // แทน "FormatColumn" ด้วยชื่อคอลัมน์ที่เก็บ format ใน DataTable ของคุณ

                    // ตรวจสอบว่า strSubject มีในรูปแบบที่ต้องการหรือไม่
                    //if (strSubject.Contains("ShipDate" + formatToCheck))
                    if (strSubject.IndexOf("ShipDate" + formatToCheck, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // แปลง birthDate ให้อยู่ในรูปแบบที่ตรงกันกับ formatToCheck
                        try
                        {
                            string actualShipDate = shipDate.ToString(formatToCheck).ToUpper();

                            // Replace ส่วนของ strSubject ที่ตรงกับ "Shipdate" + formatToCheck ด้วย actualBirthDate
                            //strSubject = strSubject.Replace("ShipDate" + formatToCheck, actualShipDate);
                            strSubject = Regex.Replace(strSubject, "ShipDate" + formatToCheck, actualShipDate, RegexOptions.IgnoreCase);
                            return strSubject;
                        }
                        catch (FormatException)
                        {
                            // กรณีรูปแบบไม่ถูกต้อง
                            MessageBox.Show($"Invalid format: {formatToCheck}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return strSubject;
                        }
                    }
                }

                // ถ้าไม่พบรูปแบบที่ตรงกัน
                //MessageBox.Show($"No matching format found in strSubject", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return strSubject;
            }
            else
            {
                // ถ้าไม่มีคำว่า "Shipdate" ใน strSubject
                MessageBox.Show("No Shipdate found in " + strSubject, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return strSubject;
            }
        }

        public static string ReplaceCaseInsensitive(string input, string search, string replacement)
        {
            return Regex.Replace(input, Regex.Escape(search), replacement.Replace("$", "$$"), RegexOptions.IgnoreCase);
        }

        //public DataGridView infoEmail()

        public void runAs(string pathExe)
        {

            string passwordFromConfig = ConfigurationManager.AppSettings["Password"];
            string usernameFromConfig = ConfigurationManager.AppSettings["Username"];
            string domainFromConfig = ConfigurationManager.AppSettings["Domain"];

            // สร้าง SecureString สำหรับเก็บรหัสผ่าน
            SecureString securePassword = new SecureString();
            foreach (char c in passwordFromConfig)
            {
                securePassword.AppendChar(c);
            }

            // สร้าง ProcessStartInfo
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = pathExe, // ใช้ runas command
                Verb = "runas",
                UserName = usernameFromConfig, // ระบุชื่อผู้ใช้ Administrator
                Domain = domainFromConfig, // ระบุ Domain (ไม่มีให้เว้นว่าง)
                Password = securePassword, // ใส่รหัสผ่านด้วย SecureString
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
                //Arguments = $"/user:{domainFromConfig}\\{usernameFromConfig} \"{pathExe}\""
                // ระบุตำแหน่งของ XAMPP 

            };

            // สร้าง Process
            Process process = new Process { StartInfo = psi };

            // เริ่มกระบวนการ
            try
            {
                process.Start();

                // รอให้กระบวนการทำงานเสร็จสมบูรณ์
                process.WaitForExit();

                // ตรวจสอบสถานะ MySQL
                CheckMysqlStatusMessageBox();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                // รอให้กระบวนการทำงานเสร็จสมบูรณ์
                //process.WaitForExit();
            }

            //CheckMysqlStatusMessageBox();
            //// อ่านผลลัพธ์จาก StandardOutput
            //string output = process.StandardOutput.ReadToEnd();
            //string error = process.StandardError.ReadToEnd();

            //// รอให้กระบวนการทำงานเสร็จสมบูรณ์
            //process.WaitForExit();

            //// แสดงผลลัพธ์หรือ error ตามที่ต้องการ
            //Console.WriteLine("Output: " + output);
            //Console.WriteLine("Error: " + error);
        }

        static void CheckMysqlStatusMessageBox()
        {
            // ค้นหา mysqld.exe ในกระบวนการทั้งหมด
            Process[] processes = Process.GetProcessesByName("mysqld");

            if (processes.Length > 0)
            {
                // มี mysqld.exe ทำงาน
                StringBuilder message = new StringBuilder();
                foreach (Process process in processes)
                {
                    message.AppendLine($"PID: {process.Id}");
                }

                MessageBox.Show($"MySQL Status: Running\n\n{message.ToString()}", "MySQL Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // ไม่มี mysqld.exe ทำงาน
                MessageBox.Show("MySQL Status: Not Running", "MySQL Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        static string[] GetMatchingFolders(string rootDirectory, string folderPattern)
        {
            try
            {
                string[] folders = Directory.GetDirectories(rootDirectory);
                Regex regex = new Regex(folderPattern);

                return Array.FindAll(folders, folder => regex.IsMatch(Path.GetFileName(folder)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new string[0];
            }
        }


        //public bool SendEmail(EmailProperty prop, EmailProperty.replaceWord itemData)
        //{
        //    //foreach ตาม //
        //    try
        //    {
        //        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        //        SmtpClient smtpClient = new SmtpClient
        //        {
        //            Host = prop.smtpHost,
        //            DeliveryMethod = SmtpDeliveryMethod.Network,
        //            UseDefaultCredentials = false,
        //            Credentials = new NetworkCredential(prop.emailSender, prop.passwordSender),
        //            TargetName = prop.targetName,
        //            Port = 587 ,
        //            EnableSsl = true


        //        };

        //        // สร้างอีเมล

        //        MailMessage mail = new MailMessage();

        //        mail.IsBodyHtml = true;

        //        // เพิ่มที่อยู่อีเมล 'To'
        //        foreach (string toAddress in prop.toEmailAddresses)
        //        {
        //            mail.To.Add(new MailAddress(toAddress));
        //        }

        //        // เพิ่มที่อยู่อีเมล 'Cc'
        //        foreach (string ccAddress in prop.ccEmailAddresses)
        //        {
        //            mail.CC.Add(new MailAddress(ccAddress));
        //        }

        //        mail.From = new MailAddress(prop.emailSender);

        //        // กำหนดเรื่อง
        //        mail.Subject = itemData.subject;

        //        // กำหนดเนื้อหา
        //        mail.Body = "Dear Sir/Madam " + "<BR><BR>" + itemData.body;

        //        foreach (string strAtt in prop.attachmentFilePaths)
        //        {
        //            Attachment attachment = new Attachment(strAtt);
        //            mail.Attachments.Add(attachment);
        //        }

        //        // ส่งอีเมล
        //        smtpClient.Send(mail);
        //        return true;
        //    }
        //    catch(Exception ex)
        //    {
        //        MessageBox.Show("เกิดข้อผิดพลาดในการส่งอีเมล์: " + ex.InnerException + "\n"  + ex.Message , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return false;
        //    }

        //}

        //public bool SendEmail(EmailProperty prop, EmailProperty.replaceWord itemData)
        //{
        //    Outlook.Application app = null;
        //    Outlook.MailItem mail = null;

        //    try
        //    {
        //        // 1. สร้าง Object Outlook Application
        //        app = new Outlook.Application();
        //        mail = (Outlook.MailItem)app.CreateItem(Outlook.OlItemType.olMailItem);

        //        // 2. ค้นหา Account ที่จะใช้ส่ง (เพื่อให้ตรงกับ prop.emailSender)
        //        // หมายเหตุ: Outlook ไม่ใช้ passwordSender เพราะมันใช้ Session ที่ Login ค้างไว้
        //        if (!string.IsNullOrEmpty(prop.emailSender))
        //        {
        //            Outlook.Accounts accounts = app.Session.Accounts;
        //            for (int i = 1; i <= accounts.Count; i++)
        //            {
        //                Outlook.Account acc = accounts.Item(i);
        //                if (acc.SmtpAddress.Equals(prop.emailSender, StringComparison.OrdinalIgnoreCase))
        //                {
        //                    mail.SendUsingAccount = acc;
        //                    break;
        //                }
        //            }
        //        }

        //        // 3. จัดการผู้รับ (To)
        //        if (prop.toEmailAddresses != null)
        //        {
        //            foreach (string toAddress in prop.toEmailAddresses)
        //            {
        //                if (!string.IsNullOrWhiteSpace(toAddress))
        //                {
        //                    Outlook.Recipient dest = mail.Recipients.Add(toAddress);
        //                    dest.Type = (int)Outlook.OlMailRecipientType.olTo;
        //                }
        //            }
        //        }

        //        // 4. จัดการผู้รับสำเนา (CC)
        //        if (prop.ccEmailAddresses != null)
        //        {
        //            foreach (string ccAddress in prop.ccEmailAddresses)
        //            {
        //                if (!string.IsNullOrWhiteSpace(ccAddress))
        //                {
        //                    Outlook.Recipient cc = mail.Recipients.Add(ccAddress);
        //                    cc.Type = (int)Outlook.OlMailRecipientType.olCC;
        //                }
        //            }
        //        }

        //        // สั่งตรวจสอบรายชื่อผู้รับทั้งหมด
        //        mail.Recipients.ResolveAll();

        //        // 5. กำหนดเรื่อง (Subject)
        //        mail.Subject = itemData.subject;

        //        // 6. กำหนดเนื้อหา (ต้องใช้ HTMLBody)
        //        mail.BodyFormat = Outlook.OlBodyFormat.olFormatHTML;
        //        // เติม <html> tag เพื่อความสมบูรณ์ (แต่ไม่เติมก็ได้ Outlook พอเข้าใจ)
        //        mail.HTMLBody = "<html><body>Dear Sir/Madam <br><br>" + itemData.body + "</body></html>";

        //        // 7. การแนบไฟล์
        //        if (prop.attachmentFilePaths != null)
        //        {
        //            foreach (string strAtt in prop.attachmentFilePaths)
        //            {
        //                // เช็คว่าไฟล์มีจริงไหม เพื่อกัน Error
        //                if (System.IO.File.Exists(strAtt))
        //                {
        //                    // Type 1 = olByValue (แนบไฟล์ไปกับเมล)
        //                    mail.Attachments.Add(strAtt, Outlook.OlAttachmentType.olByValue);
        //                }
        //            }
        //        }

        //        // 8. ส่งอีเมล
        //        // หมายเหตุ: อาจจะมี Pop-up Security เด้งถาม User ขึ้นอยู่กับ Policy ของเครื่อง
        //        mail.Send();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        string errorMsg = ex.Message;
        //        // เช็ค InnerException เพราะบางที Error หลักไม่บอกรายละเอียด
        //        if (ex.InnerException != null)
        //        {
        //            errorMsg += "\nInner: " + ex.InnerException.Message;
        //        }

        //        MessageBox.Show("เกิดข้อผิดพลาดในการส่งอีเมล์ (Outlook): " + errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return false;
        //    }
        //    finally
        //    {
        //        // (Optional) คืนค่า Resource
        //        if (mail != null) mail = null;
        //        if (app != null) app = null;
        //    }
        //}

        public bool SendEmail(EmailProperty prop, EmailProperty.replaceWord itemData)
        {
            bool _result = false;
            Outlook.Application app = null;
            Outlook.MailItem mail = null;

            // not on the production database -> never send, see MailSafety
            bool testMode = MailSafety.IsTestMode;
            // In test mode, Group 1000 is internal-only and Group 0 is the explicitly
            // approved QA group which may contain external recipients.
            List<string> internalTestAddresses = new List<string>();
            List<string> externalTestAddresses = new List<string>();
            if (testMode)
            {
                foreach (int groupId in MailSafety.TestMailGroupIds)
                {
                    List<string> groupAddresses = _emailCon.getGroupEmailAddresses(groupId);
                    if (groupId == 0) externalTestAddresses.AddRange(groupAddresses);
                    else internalTestAddresses.AddRange(groupAddresses);
                }
            }

            bool testSend = testMode &&
                            MailSafety.CanSendTestMail(prop.toEmailAddresses, prop.ccEmailAddresses,
                                                       internalTestAddresses, externalTestAddresses);
            bool addRecipients = !testMode || testSend;

            try
            {
                // 1. สร้าง Object Outlook Application
                app = new Outlook.Application();
                mail = (Outlook.MailItem)app.CreateItem(Outlook.OlItemType.olMailItem);


                // 2. จัดการผู้รับ (To)
                if (prop.toEmailAddresses != null && addRecipients)
                {
                    foreach (string toAddress in prop.toEmailAddresses)
                    {
                        if (!string.IsNullOrWhiteSpace(toAddress))
                        {
                            Outlook.Recipient dest = mail.Recipients.Add(toAddress);
                            dest.Type = (int)Outlook.OlMailRecipientType.olTo;
                        }
                    }
                }

                // 3. จัดการผู้รับสำเนา (CC)
                if (prop.ccEmailAddresses != null && addRecipients)
                {
                    foreach (string ccAddress in prop.ccEmailAddresses)
                    {
                        if (!string.IsNullOrWhiteSpace(ccAddress))
                        {
                            Outlook.Recipient cc = mail.Recipients.Add(ccAddress);
                            cc.Type = (int)Outlook.OlMailRecipientType.olCC;
                        }
                    }
                }

                // 4. สั่งตรวจสอบรายชื่อผู้รับทั้งหมด
                mail.Recipients.ResolveAll();

                // 5. กำหนดเรื่อง (Subject)
                // a mail to the test group looks exactly like the real one; only the draft is marked
                bool draftOnly = testMode && !testSend;
                mail.Subject = draftOnly ? MailSafety.TEST_SUBJECT_PREFIX + itemData.subject : itemData.subject;

                // 6. กำหนดเนื้อหา (ต้องใช้ HTMLBody)
                mail.BodyFormat = Outlook.OlBodyFormat.olFormatHTML;
                // เติม <html> tag เพื่อความสมบูรณ์
                // draft: the recipients it would have gone to are listed on top instead of being added
                string testNotice = draftOnly ? MailSafety.IntendedRecipientsHtml(prop.toEmailAddresses, prop.ccEmailAddresses) : "";
                mail.HTMLBody = "<html><body>" + testNotice + "Dear Sir/Madam <br><br>" + itemData.body + "</body></html>";

                

                // 7. การแนบไฟล์
                if (prop.attachmentFilePaths != null)
                {
                    foreach (string strAtt in prop.attachmentFilePaths)
                    {
                        // เช็คว่าไฟล์มีจริงไหม เพื่อกัน Error
                        if (System.IO.File.Exists(strAtt))
                        {
                            // Type 1 = olByValue (แนบไฟล์ไปกับเมล)
                            mail.Attachments.Add(strAtt, Outlook.OlAttachmentType.olByValue);
                        }
                    }
                }

                // 8. test mode: keep it as a draft without recipients - nothing leaves Outlook
                if (testMode && !testSend)
                {
                    mail.Save();
                    return true;   // finally still releases the COM objects
                }

                // 8. ส่งอีเมล
                bool sendSuccess = false;
                Exception sendException = null;
                var sendThread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        mail.Send(); // ส่งตรงเลย ไม่มี .Display(false)
                        sendSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        sendException = ex;
                    }
                });
                sendThread.Start();
                bool finished = sendThread.Join(30000); // รอสูงสุด 30 วินาที (ปรับได้)
                if (!finished)
                {
                    sendThread.Abort(); // บังคับหยุดถ้ายังค้าง
                    throw new TimeoutException("การส่งอีเมลค้างเกิน 30 วินาที (น่าจะมี security prompt เด้ง background)");
                }
                if (sendException != null)
                    throw sendException;

                // (Optional) บังคับ Sync ทันทีถ้าต้องการ (แทนเช็ค Sent Items, ไม่ trigger prompt เพิ่ม)
                if (app.Session.SyncObjects.Count >= 1)
                {
                    app.Session.SyncObjects.Item(1).Start();
                }
                else
                {
                    // Handle case ที่ไม่มี sync object (เช่น throw error หรือ skip)
                }

                _result = true; // *** ส่งสำเร็จ ให้เป็น true ***


            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                // เช็ค InnerException เพราะบางที Error หลักไม่บอกรายละเอียด
                if (ex.InnerException != null)
                {
                    errorMsg += "\nInner: " + ex.InnerException.Message;
                }

                MessageBox.Show("เกิดข้อผิดพลาดในการส่งอีเมล์ (Outlook): " + errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _result = false; // *** เกิด Error ให้เป็น false ***
            }
            finally
            {
                // (Optional) คืนค่า Resource
                if (mail != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(mail);
                if (app != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(app);

                // เคลียร์ตัวแปร
                mail = null;
                app = null;

            }

            return _result;

        }

        public EmailProperty prepareFileInfoEmail(EmailProperty emailProp)
        {
            try
            {
                int subCount;
                DataGridView dgvFile = new DataGridView();
                DataGridView dgslFile = new DataGridView();
                DataGridView dgwfFile = new DataGridView();

                DataGridView dgvInfoEmail = new DataGridView();
                EmailCls emailCls = new EmailCls();
                FileCls fileCls = new FileCls();

                //emailProp = _emailCon.getEmailAddress(emailProp);

                DataTable dtInfoEmail = new DataTable();
                dtInfoEmail.Columns.Add("toEmailAddresses", typeof(List<string>));
                dtInfoEmail.Columns.Add("ccEmailAddresses", typeof(List<string>));
                dtInfoEmail.Columns.Add("productName", typeof(string));
                dtInfoEmail.Columns.Add("key_id", typeof(string));
                dtInfoEmail.Columns.Add("subject", typeof(string));
                dtInfoEmail.Columns.Add("body", typeof(string));
                dtInfoEmail.Columns.Add("FILE_FORMATS_ID", typeof(string));

                DataTable dtFile = new DataTable();

                dtFile.Columns.Add("folder", typeof(string));
                dtFile.Columns.Add("attached", typeof(string));

                DataTable dtslFile = new DataTable();
                dtslFile.Columns.Add("folder", typeof(string));
                dtslFile.Columns.Add("attached", typeof(string));

                DataTable dtwfFile = new DataTable();
                dtwfFile.Columns.Add("folder", typeof(string));
                dtwfFile.Columns.Add("attached", typeof(string));

                EmailProperty.replaceWord rpWord = new EmailProperty.replaceWord();

                shipProp.customerName = emailProp.selectedRowCustomer["customerName"].ToString();

                shipProp.dbProduct = emailProp.selectedRowProduct["DATABASE_NAME"].ToString();
                shipProp.productName = emailProp.selectedRowProduct["PRODUCT_NAME"].ToString();
                shipProp.key = emailProp.selectedRowCustomer["KEY_ID"].ToString();
                shipProp.productNameInEmail = emailProp.selectedRowProduct["NAME_IN_FileServer"].ToString();
                emailProp.productName = shipProp.productName;
                emailProp.key = shipProp.key;
                emailProp.fileFormatsId = emailProp.selectedRowCustomer["FILE_FORMATS_ID"].ToString();
                emailProp.pathShipping = emailProp.selectedRowProduct["PATH_SHIPPING"].ToString();

                // แสดงผลลัพธ์ใน LabelName
                if (emailProp.pathShipping.EndsWith("\\"))
                {
                    // ถ้าลงท้ายด้วย backslash ไม่ต้องทำอะไร
                }
                else
                {
                    //เติม backslash
                    emailProp.pathShipping += "\\";
                }


                rpWord.productName = shipProp.productName;
                rpWord.productNameInEmail = emailProp.selectedRowProduct["NAME_IN_EMAIL"].ToString();
                rpWord.productID = emailProp.selectedRowProduct["PRODUCT_ID"].ToString();
                rpWord.pathPO = emailProp.selectedRowProduct["PATH_PO_ALL"].ToString();
                rpWord.customerName = emailProp.selectedRowCustomer["customerName"].ToString();
               
                // subject เอาไว้ท้าย เพื่อรอนับ
                rpWord.subject = _emailCon.getSubject(emailProp);
                rpWord.body = _emailCon.getBody(emailProp);
                rpWord.signature = _emailCon.getSignature(emailProp);

                emailProp.pathShipping = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-Shipping Data" + "\\";
                emailProp.folderCustomer = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")\\";

                string[] Folders = Directory.GetDirectories(emailProp.pathShipping);

                List<string> zipFileNames = new List<string>();
                int totalZipFiles = 0;
                int count = 1;
                int i = 0;

                string dateToMatch = "";

                string shippingPathLD = "";
                string shippingPathHSLD = "";
                string shippingPathSerialList = "";
                string shippingPathWafer = "";

                string[] srcPath;
                string[] spFile;
                string[] folderEdata;
                string[] folderCustomer;
                string[] folderSerialList;
                string[] folderWafer;

                string[] slFile;
                string[] wfFile;

                
                switch (emailProp.fileFormatsId)
                {

                    //CSV
                    case "1":
                        return emailProp;
                    //XLS XLSX
                    case "2":

                        folderCustomer = Directory.GetDirectories(emailProp.folderCustomer);

                        if (folderCustomer.Length != emailProp.dgvShip.Rows.Count)
                        {
                            MessageBox.Show("จำนวน order กับ จำนวน folder ของดาต้าไม่เท่ากัน " + emailProp.folderCustomer, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.Rows)
                        {
                            shipProp.subFolder = dtShipRow.Cells["subFolder"].Value.ToString();
                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            emailProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            if (shipProp.productName == "1480")
                            {
                                dateToMatch = emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + emailProp.partNo.Substring(7) + ")-" + shipProp.subFolder;
                            }
                            else
                            {
                                dateToMatch = emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + emailProp.partNo + ")-" + shipProp.subFolder;
                            }

                            rpWord.shipDate = emailProp.shipDate;
                            rpWord.formatDate = _emailCon.allFormatDate();
                            rpWord.subject = _emailCon.getSubject(emailProp);
                            rpWord.body = _emailCon.getBody(emailProp);
                            rpWord.signature = _emailCon.getSignature(emailProp);

                            rpWord.subject = strProcessSubject(rpWord);
                            rpWord.body = strProcessBody(rpWord);
                            rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                            var filteredSubfolders = Folders
                            .SelectMany(folder => Directory.GetDirectories(folder)
                                                            .Where(subfolder => Path.GetFileName(subfolder)?.StartsWith(dateToMatch) ?? false))
                            .ToList();

                            bool fileFound = false; // ตัวแปรตรวจสอบว่าพบไฟล์หรือไม่

                            foreach (string subfolder in filteredSubfolders)
                            {
                                string[] files = Directory.GetFiles(subfolder);
                                string targetFileName = dateToMatch;
                                string filePath = Path.Combine(subfolder, targetFileName + ".xls");

                                if (string.IsNullOrEmpty(shipProp.subFolder))
                                {
                                    var fi = Directory.EnumerateFiles(subfolder, "*P)-data*");
                                    if (fi.Any())
                                    {
                                        string foundFile = fi.FirstOrDefault();
                                        if (File.Exists(foundFile))
                                        {
                                            dtFile.Rows.Add(subfolder, foundFile);
                                            fileFound = true;
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    var fi = Directory.EnumerateFiles(subfolder, "*P)-data-" + shipProp.subFolder);
                                    if (fi.Any())
                                    {
                                        string foundFile = fi.FirstOrDefault();
                                        if (File.Exists(foundFile))
                                        {
                                            dtFile.Rows.Add(subfolder, foundFile);
                                            fileFound = true;
                                            continue;
                                        }
                                    }
                                }

                                string extension = Path.GetExtension(filePath);
                                string nameSubfolder = "-" + shipProp.subFolder;
                                int lengthToRemove = nameSubfolder.Length;
                                string cleanedFileName = targetFileName.Substring(0, targetFileName.Length - lengthToRemove);

                                if (extension == "")
                                {
                                    filePath = Path.Combine(subfolder, cleanedFileName + ".xlsx");
                                    if (File.Exists(filePath))
                                    {
                                        dtFile.Rows.Add(subfolder, filePath);
                                        fileFound = true;
                                        continue;
                                    }
                                }
                                else
                                {
                                    filePath = Path.Combine(subfolder, cleanedFileName + ".xls");
                                    if (File.Exists(filePath))
                                    {
                                        dtFile.Rows.Add(subfolder, filePath);
                                        fileFound = true;
                                        continue;
                                    }
                                }
                            }

                            // ตรวจสอบว่าไม่พบไฟล์สำหรับ order นี้
                            if (!fileFound)
                            {
                                string errorMessage = $"ไม่พบไฟล์ข้อมูลสำหรับ Order: {shipProp.fecOrderNo}\n" +
                                                     $"Part No: {emailProp.partNo}\n" +
                                                     $"Sub Folder: {shipProp.subFolder}\n" +
                                                     $"วันที่: {emailProp.shipDate.ToString("yyMMdd")}\n\n" +
                                                     $"กรุณาตรวจสอบไฟล์ในโฟลเดอร์: {emailProp.folderCustomer}";

                                MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (dtFile.Rows.Count == emailProp.dgvShip.Rows.Count)
                            {
                                break;
                            }
                        }
                        emailProp = _emailCon.getEmailAddress(emailProp);

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;

                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;
                        emailProp.isValid = true;
                        return emailProp;
                    //ZIP , //RAR เก็บ zip เฉยๆ
                    case "3":

                        // MOLEX / MOLEX-SG / OPLINK / OPLINK-SUBMARINE: 1012 prepares the zips itself (was program 1084)
                        if (IsCase3PrepareCustomer(rpWord.customerName))
                            return prepareCase3PrepareZip(emailProp, emailProp.dgvShip.Rows.Cast<DataGridViewRow>(), true,
                                                          rpWord, dtFile, dtInfoEmail, dgvFile, dgvInfoEmail);

                        subCount = 1;

                        folderCustomer = Directory.GetDirectories(emailProp.folderCustomer);

                        if (folderCustomer.Length != emailProp.dgvShip.Rows.Count)
                        {
                            MessageBox.Show("จำนวน order กับ จำนวน folder ของดาต้าไม่เท่ากัน " + emailProp.folderCustomer, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.pathShipping = Path.Combine(emailProp.pathShipping , "Send"); 


                        // เรียกใช้ฟังก์ชันเพื่อเก็บชื่อไฟล์และนับจำนวนไฟล์ zip
                        //List<string> zipFileNames = new List<string>();

                        // ระบุ path ที่ต้องการตรวจสอบ , ระบุชื่อเริ่มต้นของไฟล์ zip
                        totalZipFiles = fileCls.CountZipFilesInDirectory(emailProp.pathShipping, emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")", zipFileNames);

                        if (totalZipFiles == 0)
                        {
                            //MessageBox.Show("ไม่พบ file zip ที่  " + emailProp.pathShipping + "\\" + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")");
                            emailProp.isValid = false;
                            return emailProp;
                        }


                        //if (totalZipFiles != emailProp.dgvShip.Rows.Count)
                        //{
                        //    MessageBox.Show("file zip กับจำนวน test report ใน Database ไม่เท่ากัน กรุณาเช็ค path  " + emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")");
                        //    emailProp.isValid = false;
                        //    return emailProp;
                        //}

                        
                        foreach (string file in zipFileNames)
                        {

                            rpWord.folderName  = fileCls.GetZipDirectoryContents(emailProp.pathShipping + "\\" + file);
                            rpWord.folderName = rpWord.folderName.Replace(emailProp.shipDate.ToString("yyMMdd") + "-", "");

                            dtFile.Rows.Add(emailProp.pathShipping +"\\" + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")", emailProp.pathShipping + "\\"+ file);
                            // add subject count / zipfilecount

                            emailProp = _emailCon.getEmailAddress(emailProp);

                            rpWord.shipDate = emailProp.shipDate;
                            rpWord.formatDate = _emailCon.allFormatDate();
                            rpWord.subject = _emailCon.getSubject(emailProp);
                            rpWord.body = _emailCon.getBody(emailProp);
                            rpWord.signature = _emailCon.getSignature(emailProp);

                            if (zipFileNames.Count > 1)
                            {
                                rpWord.subject = strProcessSubject(rpWord);
                                rpWord.subject = rpWord.subject + "(" + count + "/" + zipFileNames.Count + ")";
                            }
                            else
                            {
                                rpWord.subject = strProcessSubject(rpWord);
                            }
                            rpWord.body = strProcessBody(rpWord);
                            rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                            dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses ,emailProp.productName, emailProp.key, rpWord.subject, rpWord.body , emailProp.fileFormatsId);
                            count++;
                        }

                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;


                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;
                        emailProp.isValid = true;
     
                        return emailProp;

                    //TXT
                    case "4":

                        // 1 เมล /1 spec /1 sub / หลายไฟล์ 
                        subCount = 1;


                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.Rows)
                        {
                            DataTable dtSerial = new DataTable();

                            emailProp.productName = shipProp.productName;
                            emailProp.key = dtShipRow.Cells["KEY_ID"].Value.ToString();
                            rpWord.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            //fecorderNo or ManuFactureNo

                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            shipProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            dtSerial = _shipCon.SearchSerialShipping(shipProp);

                            //subject
                            rpWord.Qty = dtSerial.Rows.Count.ToString();
                            rpWord.shipDate = emailProp.shipDate;
                            rpWord.formatDate = _emailCon.allFormatDate();
                            rpWord.subject = _emailCon.getSubject(emailProp);
                            //body
                            rpWord.body = _emailCon.getBody(emailProp);
                            rpWord.signature = _emailCon.getSignature(emailProp);

                            rpWord.subject = strProcessSubject(rpWord);

                            if(rpWord.subject == "")
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            rpWord.body = strProcessBody(rpWord);
                            rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;
                            //to
                            //cc
                            emailProp = _emailCon.getEmailAddress(emailProp);


                            dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses,emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);


                            string folderMain = emailProp.folderCustomer + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo.Substring(7) + ")-" + subCount.ToString() + "\\";

                            //dtSerial = _shipCon.SearchSerialShipping(shipProp);

                            //bool noFilesFound = true; // สร้างตัวแปรเพื่อเก็บค่าว่าไม่มีไฟล์ไหน
                            string message = "";


                            foreach (DataRow row in dtSerial.Rows)
                            {
                                string serial = row["serial"].ToString();
                                string file = folderMain + shipProp.customerPN + "_" + serial + ".txt";
                                if (File.Exists(file))
                                {
                                    // มีไฟล์อยู่
                                    //message += "ไฟล์ " + file + " มีอยู่\n";
                                    dtFile.Rows.Add(shipProp.customerPN + "_" + serial + ".txt", folderMain + shipProp.customerPN + "_" + serial + ".txt");
                                    emailProp.isValid = true;
                                }
                                else
                                {
                                    // ไม่มีไฟล์อยู่
                                    message += "ไฟล์ " + file + " ไม่มีอยู่\n";
                                    emailProp.isValid = false;
                                }

                                //dtFile.Rows.Add(shipProp.customerPN + "_" + serial + ".txt" , folderMain + shipProp.customerPN + "_" + serial + ".txt");


                            }

                            dgvFile.DataSource = dtFile;
                            emailProp.dgvFile = dgvFile;



                            dgvInfoEmail.DataSource = dtInfoEmail;
                            emailProp.dgvInfoEmail = dgvInfoEmail;
                            

                            if (emailProp.isValid == false)
                            {
                                MessageBox.Show(message, "สรุปการตรวจสอบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return emailProp;
                            }

                           

                        }

                        //dgvFile.DataSource = dtFile;
                        //emailProp.dgvFile = dgvFile;

                     

                        //dgvInfoEmail.DataSource = dtInfoEmail;
                        //emailProp.dgvInfoEmail = dgvInfoEmail;
                        //emailProp.isValid = true;
                        return emailProp;

                    //zip file ส่ง
                    case "5":
                        //check จำนวน serial 
                        //check serial 
                        //test 5 june 2025 50 ตัว

                        subCount = 1;
                
                        folderCustomer = Directory.GetDirectories(emailProp.folderCustomer);
                  
                        if (folderCustomer.Length != emailProp.dgvShip.Rows.Count)
                        {
                            MessageBox.Show("จำนวน order กับ จำนวน folder ของดาต้าไม่เท่ากัน " + emailProp.folderCustomer, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.Rows)
                        {

                            emailProp.productName = shipProp.productName;
                            emailProp.key = dtShipRow.Cells["KEY_ID"].Value.ToString();
                            rpWord.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            //fecorderNo or ManuFactureNo

                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            shipProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            //string folderMain = emailProp.folderCustomer + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo.Substring(7) + ")" + "\\";
                            //if (fileCls.CompressFileZip(emailProp.folderCustomer, emailProp.folderCustomer) == false)
                            //{
                            //    emailProp.isValid = false;
                            //    return emailProp;
                            //}

                            if (fileCls.CompressFileManage(shipProp, emailProp) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            
                            rpWord.shipDate = emailProp.shipDate;
                            rpWord.formatDate = _emailCon.allFormatDate();
                            rpWord.subject = _emailCon.getSubject(emailProp);
                            //body
                            rpWord.body = _emailCon.getBody(emailProp);
                            rpWord.signature = _emailCon.getSignature(emailProp);

                            rpWord.subject = strProcessSubject(rpWord);
                            rpWord.body = strProcessBody(rpWord);
                            rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;
                            //to
                            //cc
                            emailProp = _emailCon.getEmailAddress(emailProp);


                            dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses,emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                            dtFile.Rows.Add(emailProp.pathShipping, emailProp.folderCustomer.TrimEnd('\\') + ".zip");
                            dgvFile.DataSource = dtFile;
                            emailProp.dgvFile = dgvFile;

                         

                            dgvInfoEmail.DataSource = dtInfoEmail;
                            emailProp.dgvInfoEmail = dgvInfoEmail;
                            emailProp.isValid = true;
                            break;

                        }

                        return emailProp;

                    case "6":

                        //emailProp.dgvShip.Sort(emailProp.dgvShip.Columns["manuFactureNo"], ListSortDirection.Ascending);
                        if (shipProp.customerName.ToUpper() != "HISILICON" && shipProp.customerName.ToUpper() != "HUAWEI")
                        {
                            MessageBox.Show("case นี้ สำหรับ Hisilicon/Huawei เท่านั้น", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        folderCustomer = Directory.GetDirectories(emailProp.folderCustomer);


                        if (folderCustomer.Length != emailProp.dgvShip.Rows.Count)
                        {
                            MessageBox.Show("จำนวน order กับ จำนวน folder ของดาต้าไม่เท่ากัน " + emailProp.folderCustomer, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.dgvShip.Sort(emailProp.dgvShip.Columns["manuFactureNo"], ListSortDirection.Ascending);


                        shippingPathHSLD = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-HSLD_Data\\";
                        shippingPathSerialList = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-SerialList\\";
                        shippingPathWafer = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-Wafer\\";

                        


                        if (!Directory.Exists(shippingPathHSLD))
                        {
                            Directory.CreateDirectory(shippingPathHSLD);
                        }
                        else
                        {
                            Directory.Delete(shippingPathHSLD, true);
                            Directory.CreateDirectory(shippingPathHSLD);
                        }

                        if (!Directory.Exists(shippingPathSerialList))
                        {
                            Directory.CreateDirectory(shippingPathSerialList);
                        }
                        else
                        {
                            Directory.Delete(shippingPathSerialList, true);
                            Directory.CreateDirectory(shippingPathSerialList);
                        }

                        if (!Directory.Exists(shippingPathWafer))
                        {
                            Directory.CreateDirectory(shippingPathWafer);
                        }
                        else
                        {
                            Directory.Delete(shippingPathWafer, true);
                            Directory.CreateDirectory(shippingPathWafer);
                        }

                        folderEdata = Directory.GetDirectories(shippingPathHSLD);
                        folderSerialList = Directory.GetDirectories(shippingPathSerialList);
                        folderWafer = Directory.GetDirectories(shippingPathWafer);
                       

                        srcPath = new string[emailProp.dgvShip.Rows.Count];
                        spFile = new string[emailProp.dgvShip.Rows.Count];
                        slFile = new string[emailProp.dgvShip.Rows.Count];
                        wfFile = new string[emailProp.dgvShip.Rows.Count];

                        count = 0;
                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.Rows)
                        {

                            rpWord.shipDate = emailProp.shipDate;

                            emailProp.productName = shipProp.productName;

                            emailProp.key = dtShipRow.Cells["KEY_ID"].Value.ToString();
                            rpWord.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            //fecorderNo or ManuFactureNo

                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            shipProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            emailProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            shipProp.qty = dtShipRow.Cells["Qty"].Value.ToString();

                            string edata = Path.Combine(folderCustomer[count], "Edata");
                            var fi = Directory.EnumerateFiles(edata, emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo + ")" + "*");

                            if (fi.Any())
                            {
                                spFile[count] = fi.FirstOrDefault();

                                emailProp.isValid = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบ order or file Edata นี้ " + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (fileCls.fnCopyFile(spFile[count], shippingPathHSLD) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            var wf = Directory.EnumerateFiles(edata, emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo + ")" + "-wafer-*");

                            if (wf.Any())
                            {
                                wfFile[count] = wf.FirstOrDefault();

                                emailProp.isValid = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบ order or file Edata นี้ " + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo + ")" , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (fileCls.fnCopyFile(wfFile[count], shippingPathWafer) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            var sl = Directory.EnumerateFiles(edata, emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "-" + shipProp.qty + "*");

                            if (sl.Any())
                            {
                                slFile[count] = sl.FirstOrDefault();
                                emailProp.isValid = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบ order or serialList นี้ " + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "-" + shipProp.qty ,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (fileCls.fnCopyFile(slFile[count], shippingPathSerialList) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }
                            count++;
                        }


                        //// Data 
                        for (count = 0; count < spFile.Length; count++)
                        {
                            // ดึงไฟล์จากลิสต์โดยใช้ดัชนี
                            dtFile.Rows.Add(shippingPathHSLD, spFile[count]);
                        }
                        emailProp = _emailCon.getEmailAddress(emailProp);

                        rpWord.shipDate = emailProp.shipDate;
                        rpWord.formatDate = _emailCon.allFormatDate();
                        rpWord.subject = _emailCon.getSubject(emailProp);
                        rpWord.body = _emailCon.getBody(emailProp);
                        rpWord.signature = _emailCon.getSignature(emailProp);

                        rpWord.subject = strProcessSubject(rpWord);
                        rpWord.body = strProcessBody(rpWord);
                        rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses,emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                        for (count = 0; count < slFile.Length; count++)
                        {
                            // ดึงไฟล์จากลิสต์โดยใช้ดัชนี
                            dtslFile.Rows.Add(shippingPathSerialList, slFile[count]);
                        }

                        rpWord.subject = "Serial list for Hisilicon" + "(" + shipProp.productNameInEmail + ") " + emailProp.shipDate.ToString("yyyy-MM-dd");
                        rpWord.body = "We attached file(s) of Serial list for Hisilicon in this e-mail";
                        rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                        emailProp.toEmailAddresses = GetDocumentRecipients(shipProp, DOC_SERIAL_LIST, "SERIAL_LIST", "to");
                        if (emailProp.toEmailAddresses == null || emailProp.toEmailAddresses.Count == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.ccEmailAddresses = GetDocumentRecipients(shipProp, DOC_SERIAL_LIST, "SERIAL_LIST", "cc");
                        if (emailProp.ccEmailAddresses == null || emailProp.ccEmailAddresses.Count == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses,emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                        for (count = 0; count < wfFile.Length; count++)
                        {
                            // ดึงไฟล์จากลิสต์โดยใช้ดัชนี
                            dtwfFile.Rows.Add(shippingPathWafer, wfFile[count]);
                        }

                        rpWord.subject = "WAFER data " + emailProp.shipDate.ToString("dd/MMM").ToUpper() + " from Furukawa for Hisilicon";
                        rpWord.body = "We attached Wafer data for Hisilicon in this e-mail";
                        rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                        // email productcustomer WAFER

                        emailProp.toEmailAddresses = GetDocumentRecipients(shipProp, DOC_WAFER, "WAFER", "to");
                        if (emailProp.toEmailAddresses == null || emailProp.toEmailAddresses.Count == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.ccEmailAddresses = GetDocumentRecipients(shipProp, DOC_WAFER, "WAFER", "cc");
                        if (emailProp.ccEmailAddresses == null || emailProp.ccEmailAddresses.Count == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }


                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses,emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;

                        dgslFile.DataSource = dtslFile;
                        emailProp.dgslFile = dgslFile;

                        dgwfFile.DataSource = dtwfFile;
                        emailProp.dgwfFile = dgwfFile;

                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;
                        emailProp.isValid = true;



                        return emailProp;

                    case "7":

                        folderCustomer = Directory.GetDirectories(emailProp.folderCustomer);

                        if (folderCustomer.Length != emailProp.dgvShip.Rows.Count)
                        {
                            MessageBox.Show("จำนวน order กับ จำนวน folder ของดาต้าไม่เท่ากัน " + emailProp.folderCustomer, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.dgvShip.Sort(emailProp.dgvShip.Columns["manuFactureNo"], ListSortDirection.Ascending);

                        shippingPathLD =  emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-LD_Data\\";
                        shipProp.shipdate = emailProp.shipDate.ToString("yyyy-MM-dd");


                        if (!Directory.Exists(shippingPathLD))
                        {
                            Directory.CreateDirectory(shippingPathLD);
                        }
                        else
                        {
                            Directory.Delete(shippingPathLD,true);
                            Directory.CreateDirectory(shippingPathLD);
                        }


                        i = 0;

                        srcPath = new string[emailProp.dgvShip.Rows.Count];
                        spFile = new string[emailProp.dgvShip.Rows.Count];


                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.Rows)
                        {

                            emailProp.productName = shipProp.productName;
                            emailProp.key = dtShipRow.Cells["KEY_ID"].Value.ToString();
                            rpWord.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            shipProp.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            //fecorderNo or ManuFactureNo

                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            shipProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();
                            emailProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            shipProp.subFolder = dtShipRow.Cells["subFolder"].Value.ToString();

                            dateToMatch = emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + emailProp.partNo.Substring(7) + ")-" + shipProp.subFolder;

                            var filteredSubfolders = Folders
                             .SelectMany(folder => Directory.GetDirectories(folder)
                                 .Where(subfolder =>
                                     string.Equals(Path.GetFileName(subfolder), dateToMatch, StringComparison.OrdinalIgnoreCase)))
                             .ToList();

                            if (!filteredSubfolders.Any())
                            {
                                MessageBox.Show(
                                    $"ไม่พบโฟลเดอร์ข้อมูลสำหรับ TestReportNo: {shipProp.testReportNo}\n" +
                                    $"Order: {shipProp.fecOrderNo}\n" +
                                    $"Sub Folder: {shipProp.subFolder}\n" +
                                    $"ชื่อโฟลเดอร์ที่ต้องการ: {dateToMatch}\n\n" +
                                    $"กรุณาตรวจสอบไฟล์ในโฟลเดอร์: {emailProp.folderCustomer}",
                                    "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            foreach (string subfolder in filteredSubfolders)
                            {
                                srcPath[i] = subfolder;
                                DataTable dtSerial = _shipCon.SearchSerialShippingCust(shipProp);
                                if (dtSerial == null)
                                {
                                    MessageBox.Show("พบปัญหาเกี่ยวกับ serial ใน database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else if (dtSerial.Rows.Count == 0)
                                {
                                    MessageBox.Show("ไม่มี serial ใน database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }

                                //foreach (DataRow r in dtSerial.Rows)
                                //{
                                    //string serial = r["serial"].ToString();
                                    //string fileName = serial + ".jpg";
                                    string jpgPath = Path.Combine(subfolder, "JPEG file");

                                    if (Directory.Exists(jpgPath))
                                    {
                                        emailProp.isValid = true;
                                    }
                                    else
                                    {
                                        MessageBox.Show("ไม่พบ path jpg " + jpgPath + "โปรด check ตำแหน่งนี้", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        emailProp.isValid = false;
                                        return emailProp;
                                    }
                                //}

                                if (shipProp.partNo.Contains("QQA-617") || shipProp.partNo.Contains("QQA-657"))
                                {
                                    string sIfImPath = Path.Combine(subfolder, "If-Im data");
                                    if (Directory.Exists(sIfImPath))
                                    {
                                        emailProp.isValid = true;
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Not found If-Im data in Order {shipProp.partNo}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        emailProp.isValid = false;
                                        return emailProp;
                                    }
                                }

                                var fi = Directory.EnumerateFiles(subfolder, shipProp.customerPN + "*");
                                if (fi.Any())
                                {
                                    spFile[i] = fi.FirstOrDefault();
                                    emailProp.isValid = true;
                                }
                                else
                                {
                                    MessageBox.Show("ไม่พบ order or file นี้ " + dateToMatch, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    emailProp.isValid = false;
                                    return emailProp;
                                }

                            }


                            i++;

                        }

                      
                        if (fileCls.fnPrepareAccelinkDirect(srcPath, shippingPathLD, spFile, emailProp.dgvShip.Rows.Count) == false)
                        {
                            MessageBox.Show("ทำการ copy ก่อนเปลี่ยนชื่อไม่สำเร็จ " + shippingPathLD, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        // ด่านที่ 1: หลัง copy เสร็จ เช็คว่าจำนวนโฟลเดอร์ที่เตรียมได้จริงครบตามจำนวน order หรือไม่
                        // กันกรณีโฟลเดอร์ต้นทางถูกลบ/หายระหว่างเตรียมข้อมูล หรือชื่อโฟลเดอร์ซ้ำจน copy ทับกัน
                        int stagedFolderCount = Directory.GetDirectories(shippingPathLD).Length;
                        if (stagedFolderCount != emailProp.dgvShip.Rows.Count)
                        {
                            MessageBox.Show(
                                $"ข้อมูล LD ไม่ครบ! เตรียมได้ {stagedFolderCount} โฟลเดอร์ จากที่ต้องส่งทั้งหมด {emailProp.dgvShip.Rows.Count} order\n\n" +
                                $"อาจมีโฟลเดอร์ข้อมูลถูกลบหรือหายระหว่างเตรียมข้อมูล กรุณาตรวจสอบแล้วเตรียมใหม่อีกครั้ง\n" +
                                $"ตำแหน่ง: {shippingPathLD}",
                                "ข้อมูลไม่ครบ - ยกเลิกการส่ง", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        if (fileCls.fnCopyEData(shippingPathLD, emailProp.shipDate) == false)
                        {
                            MessageBox.Show("ทำการ copy ก่อนเปลี่ยนชื่อไม่สำเร็จ " + shippingPathLD, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        folderEdata = Directory.GetDirectories(shippingPathLD);

                        foreach (string ffe in folderEdata)
                        {
                            if (fileCls.CompressFileZip(ffe, ffe) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }
                        }



                        // เรียกใช้ฟังก์ชันเพื่อเก็บชื่อไฟล์และนับจำนวนไฟล์ zip
                        //List<string> zipFileNames = new List<string>();

                        // ระบุ path ที่ต้องการตรวจสอบ , ระบุชื่อเริ่มต้นของไฟล์ zip
                        totalZipFiles = fileCls.CountZipFilesInDirectory(shippingPathLD, "Edata_" + emailProp.shipDate.ToString("yyyyMMdd"), zipFileNames);

                        if (totalZipFiles == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        // ด่านที่ 2 (สุดท้ายก่อนส่ง): นับจำนวนโฟลเดอร์ข้อมูล (order) ที่อยู่ในไฟล์ zip จริง
                        // ต้องเท่ากับจำนวน order ทั้งหมด ไม่งั้นถือว่าข้อมูลไม่ครบ ห้ามส่งเด็ดขาด
                        int orderFoldersInZip = 0;
                        foreach (string zipName in zipFileNames)
                        {
                            orderFoldersInZip += fileCls.CountTopLevelFoldersInZip(shippingPathLD + "\\" + zipName);
                        }

                        if (orderFoldersInZip != emailProp.dgvShip.Rows.Count)
                        {
                            MessageBox.Show(
                                $"ข้อมูล LD ในไฟล์ zip ไม่ครบ! พบข้อมูล {orderFoldersInZip} order จากที่ต้องส่งทั้งหมด {emailProp.dgvShip.Rows.Count} order\n\n" +
                                $"อาจมีโฟลเดอร์ข้อมูลถูกลบหรือหายระหว่างเตรียม/บีบอัดข้อมูล กรุณาตรวจสอบแล้วเตรียมใหม่อีกครั้ง\n" +
                                $"ตำแหน่ง: {shippingPathLD}",
                                "ข้อมูลไม่ครบ - ยกเลิกการส่ง", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        

                        foreach (string file in zipFileNames)
                        {

                            //rpWord.folderName = fileCls.GetZipDirectoryContents(shippingPathLD + "\\" + file);

                            dtFile.Rows.Add(shippingPathLD, shippingPathLD + "\\" + file);
                            // add subject count / zipfilecount

                            emailProp = _emailCon.getEmailAddress(emailProp);

                            rpWord.shipDate = emailProp.shipDate;
                            rpWord.formatDate = _emailCon.allFormatDate();
                            rpWord.subject = _emailCon.getSubject(emailProp);
                            rpWord.body = _emailCon.getBody(emailProp);
                            rpWord.signature = _emailCon.getSignature(emailProp);

                            if (zipFileNames.Count > 1)
                            {
                                rpWord.subject = strProcessSubject(rpWord);
                                rpWord.subject = rpWord.subject + "(" + count + "/" + zipFileNames.Count + ")";
                            }
                            else
                            {
                                rpWord.subject = strProcessSubject(rpWord);
                            }
                            rpWord.body = strProcessBody(rpWord);
                            rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                            dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses,emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);
                            count++;
                        }

                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;


                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;
                        emailProp.isValid = true;

                        return emailProp;

                    case "8":

                        //emailProp.dgvShip.Sort(emailProp.dgvShip.Columns["manuFactureNo"], ListSortDirection.Ascending);
                        if (shipProp.customerName.ToUpper() != "HISILICON" && shipProp.customerName.ToUpper() != "HUAWEI")
                        {
                            MessageBox.Show("case นี้ สำหรับ Hisilicon/Huawei เท่านั้น", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        folderCustomer = Directory.GetDirectories(emailProp.folderCustomer);

                        if (folderCustomer.Length != emailProp.dgvShip.Rows.Count)
                        {
                            MessageBox.Show("จำนวน order กับ จำนวน folder ของดาต้าไม่เท่ากัน " + emailProp.folderCustomer, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        //emailProp.dgvShip.Sort(emailProp.dgvShip.Columns["manuFactureNo"], ListSortDirection.Ascending);

                        shippingPathHSLD = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-LD_Data\\";
                        shippingPathSerialList = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-SerialList\\";

                        

                        if (!Directory.Exists(shippingPathHSLD))
                        {
                            Directory.CreateDirectory(shippingPathHSLD);
                        }
                        else
                        {
                            Directory.Delete(shippingPathHSLD, true);
                            Directory.CreateDirectory(shippingPathHSLD);
                        }

                        if (!Directory.Exists(shippingPathSerialList))
                        {
                            Directory.CreateDirectory(shippingPathSerialList);
                        }
                        else
                        {
                            Directory.Delete(shippingPathSerialList, true);
                            Directory.CreateDirectory(shippingPathSerialList);
                        }

                        folderEdata = Directory.GetDirectories(shippingPathHSLD);
                        folderSerialList = Directory.GetDirectories(shippingPathSerialList);
                       

                        srcPath = new string[emailProp.dgvShip.Rows.Count];
                        spFile = new string[emailProp.dgvShip.Rows.Count];
                        slFile = new string[emailProp.dgvShip.Rows.Count];

                        count = 0;
                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.Rows)
                        {

                            rpWord.shipDate = emailProp.shipDate;

                            emailProp.productName = shipProp.productName;

                            emailProp.key = dtShipRow.Cells["KEY_ID"].Value.ToString();
                            rpWord.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            //fecorderNo or ManuFactureNo

                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            shipProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            emailProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            shipProp.qty = dtShipRow.Cells["Qty"].Value.ToString();

                            shipProp.subFolder = dtShipRow.Cells["subFolder"].Value.ToString();

                            string edata = Path.Combine(folderCustomer[count], "Edata");
                            var fi = Directory.EnumerateFiles(edata, emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "("+ emailProp.partNo + ")" + "-" + shipProp.subFolder + "*" );

                            if (fi.Any())
                            {
                                spFile[count] = fi.FirstOrDefault();

                                emailProp.isValid = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบ order or file Edata นี้ " + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (fileCls.fnCopyFile(spFile[count], shippingPathHSLD) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            var sl = Directory.EnumerateFiles(edata, emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "-" + shipProp.qty + "pcs-" + shipProp.subFolder + "*");

                            if (sl.Any())
                            {
                                slFile[count] = sl.FirstOrDefault();
                                emailProp.isValid = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบ order or serialList นี้ " + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "-" + shipProp.qty + shipProp.subFolder, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (fileCls.fnCopyFile(slFile[count], shippingPathSerialList) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }
                            count++;
                        }


                        //// Data 
                        for (count = 0; count < spFile.Length; count++)
                        {
                            // ดึงไฟล์จากลิสต์โดยใช้ดัชนี
                            dtFile.Rows.Add(shippingPathHSLD, spFile[count]);
                        }
                        emailProp = _emailCon.getEmailAddress(emailProp);

                        rpWord.shipDate = emailProp.shipDate;
                        rpWord.formatDate = _emailCon.allFormatDate();
                        rpWord.subject = _emailCon.getSubject(emailProp);
                        rpWord.body = _emailCon.getBody(emailProp);
                        rpWord.signature = _emailCon.getSignature(emailProp);

                        rpWord.subject = strProcessSubject(rpWord);
                        rpWord.body = strProcessBody(rpWord);
                        rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses,emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                        for (count = 0; count < slFile.Length; count++)
                        {
                            // ดึงไฟล์จากลิสต์โดยใช้ดัชนี
                            dtslFile.Rows.Add(shippingPathSerialList, slFile[count]);
                        }

                        rpWord.subject = "Serial list for Hisilicon" + "(" + shipProp.productNameInEmail + ") " + emailProp.shipDate.ToString("yyyy-MM-dd");
                        rpWord.body = "We attached file(s) of Serial list for Hisilicon in this e-mail";
                        rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                        emailProp.toEmailAddresses = GetDocumentRecipients(shipProp, DOC_SERIAL_LIST, "SERIAL_LIST", "to");
                        if (emailProp.toEmailAddresses == null || emailProp.toEmailAddresses.Count == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.ccEmailAddresses = GetDocumentRecipients(shipProp, DOC_SERIAL_LIST, "SERIAL_LIST", "cc");
                        if (emailProp.ccEmailAddresses == null || emailProp.ccEmailAddresses.Count == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses,emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;

                        dgslFile.DataSource = dtslFile;
                        emailProp.dgslFile = dgslFile;

                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;
                        emailProp.isValid = true;

                        return emailProp;

                    case "10": // XTERA-INC, SMTC-CORP : yyMMdd-{Shipping-PO-all column V}(short Part Number).xls
                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.Rows)
                        {
                            if (dtShipRow.IsNewRow)
                                continue;

                            shipProp.subFolder = dtShipRow.Cells["subFolder"].Value?.ToString()?.Trim() ?? "";
                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value?.ToString()?.Trim() ?? "";
                            rpWord.manuFactureNo = shipProp.fecOrderNo;

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value?.ToString()?.Trim() ?? "";
                            rpWord.testReportNo = shipProp.testReportNo;

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value?.ToString()?.Trim() ?? "";
                            rpWord.customerPN = shipProp.customerPN;

                            emailProp.partNo = dtShipRow.Cells["Part_No"].Value?.ToString()?.Trim() ?? "";

                            string datePrefix = emailProp.shipDate.ToString("yyMMdd");

                            string shortPartNo = (shipProp.productName == "1480" && emailProp.partNo.Length >= 7)
                                                 ? emailProp.partNo.Substring(7)
                                                 : emailProp.partNo;

                            if (string.IsNullOrEmpty(shipProp.fecOrderNo) || string.IsNullOrEmpty(shortPartNo))
                            {
                                MessageBox.Show($"ข้อมูล FEC Order Number หรือ Part Number ไม่สมบูรณ์ ไม่สามารถค้นหาโฟลเดอร์ได้\n" +
                                                $"FEC Order No: {shipProp.fecOrderNo}\n" +
                                                $"Part No: {shortPartNo}\n" +
                                                $"ในโฟลเดอร์: {emailProp.folderCustomer}",
                                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                emailProp.isValid = false;
                                return emailProp;
                            }

                            string folderNameToSearch = !string.IsNullOrEmpty(shipProp.subFolder)
                                ? $"{datePrefix}-{shipProp.fecOrderNo}({shortPartNo})-{shipProp.subFolder}"
                                : $"{datePrefix}-{shipProp.fecOrderNo}({shortPartNo})";

                            string poFilePrefix = $"{datePrefix}-";
                            string poFileSuffix = $"({shortPartNo})";
                            string specialPO = GetCase10SpecialPO(rpWord, shipProp.testReportNo);

                            if (string.IsNullOrEmpty(specialPO))
                            {
                                MessageBox.Show($"ไม่พบ Special PO ใน Shipping-PO-all\n" +
                                                $"Test Report No: {shipProp.testReportNo}\n" +
                                                $"ไฟล์: {Case10SpecialPOAllPath}",
                                                "ไม่พบ Special PO",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);

                                emailProp.isValid = false;
                                return emailProp;
                            }

                            string expectedFileName = $"{poFilePrefix}{specialPO}{poFileSuffix}";

                            rpWord.shipDate = emailProp.shipDate;
                            rpWord.formatDate = _emailCon.allFormatDate();
                            rpWord.subject = _emailCon.getSubject(emailProp);
                            rpWord.body = _emailCon.getBody(emailProp);
                            rpWord.signature = _emailCon.getSignature(emailProp);

                            rpWord.subject = strProcessSubject(rpWord);
                            rpWord.body = strProcessBody(rpWord);
                            rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                            bool fileFound = false;
                            string lastCheckedFolder = "";
                            string foundOrderFolder = "";

                            List<string> matchedOrderFolders = new List<string>();

                            // 1. เช็คกรณี emailProp.folderCustomer เองคือ folder ที่ต้องการแล้ว
                            if (Path.GetFileName(emailProp.folderCustomer.TrimEnd('\\'))
                                .Equals(folderNameToSearch, StringComparison.OrdinalIgnoreCase))
                            {
                                matchedOrderFolders.Add(emailProp.folderCustomer.TrimEnd('\\'));
                            }

                            // 2. scan ลงไปทุก folder ย่อย โดย match ชื่อ folder แบบเท่ากันเท่านั้น
                            matchedOrderFolders.AddRange(
                                Directory.GetDirectories(emailProp.folderCustomer, "*", SearchOption.AllDirectories)
                                    .Where(dir => Path.GetFileName(dir)
                                        .Equals(folderNameToSearch, StringComparison.OrdinalIgnoreCase))
                            );

                            matchedOrderFolders = matchedOrderFolders
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToList();

                            List<string> targetFoldersToCheck = new List<string>();

                            foreach (string orderFolder in matchedOrderFolders)
                            {
                                string doubleNestedPath = Path.Combine(orderFolder, folderNameToSearch);

                                if (Directory.Exists(doubleNestedPath))
                                {
                                    targetFoldersToCheck.Add(doubleNestedPath);
                                }
                            }

                            targetFoldersToCheck = targetFoldersToCheck
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToList();

                            foreach (string targetDir in targetFoldersToCheck)
                            {
                                foundOrderFolder = targetDir;
                                lastCheckedFolder = targetDir;

                                if (!Directory.Exists(targetDir))
                                    continue;

                                string[] filesInFolder = Directory.GetFiles(targetDir)
                                    .Where(f =>
                                    {
                                        string fileNameNoExt = Path.GetFileNameWithoutExtension(f);

                                        bool isExcel =
                                            f.EndsWith(".xls", StringComparison.OrdinalIgnoreCase); //||
                                            // f.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
                                        bool matchFileName =
                                            fileNameNoExt.Equals(expectedFileName, StringComparison.OrdinalIgnoreCase);

                                        return isExcel && matchFileName;
                                    })
                                    .ToArray();

                                if (filesInFolder.Length > 0)
                                {
                                    dtFile.Rows.Add(targetDir, filesInFolder[0]);
                                    fileFound = true;
                                    break;
                                }
                            }
                            if (!fileFound && matchedOrderFolders.Count == 0)
                            {
                                MessageBox.Show($"ไม่พบโฟลเดอร์ดาต้า {folderNameToSearch}",
                                                "ไม่พบโฟลเดอร์ดาต้า",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);

                                emailProp.isValid = false;
                                return emailProp;
                            }


                            if (!fileFound && targetFoldersToCheck.Count == 0)
                            {
                                MessageBox.Show($"ไม่พบโฟลเดอร์ดาต้าชั้นที่ 2 {folderNameToSearch}\\{folderNameToSearch}",
                                                "ไม่พบโฟลเดอร์ดาต้าชั้นที่ 2",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);

                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (!fileFound)
                            {
                                MessageBox.Show($"ไม่พบไฟล์ดาต้า {expectedFileName}.xls",
                                                "ไม่พบไฟล์ดาต้า",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);

                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (dtFile.Rows.Count == emailProp.dgvShip.Rows.Count)
                            {
                                break;
                            }
                        }
emailProp = _emailCon.getEmailAddress(emailProp);

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses,
                                             emailProp.ccEmailAddresses,
                                             emailProp.productName,
                                             emailProp.key,
                                             rpWord.subject,
                                             rpWord.body,
                                             emailProp.fileFormatsId);

                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;

                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;

                        emailProp.isValid = true;
                        return emailProp;

                    case "13": // 1480Dual (COHERENT, INFINERA) : yyMMdd-{FEC order}(full Part No)[-sub]\yyMMdd-{FEC order}(full Part No)_Edata\*.txt
                        return prepareCase13TxtEdata(emailProp, emailProp.dgvShip.Rows.Cast<DataGridViewRow>(),
                                                     rpWord, dtFile, dtInfoEmail, dgvFile, dgvInfoEmail);

                    case "14": // 1480Dual MOLEX : one txt per order -> yyMMdd-MOLEX(48Dual)-n folder + .zip
                        return prepareCase14TxtZip(emailProp, emailProp.dgvShip.Rows.Cast<DataGridViewRow>(), true,
                                                   rpWord, dtFile, dtInfoEmail, dgvFile, dgvInfoEmail);

                    //Nothing
                    default:
                        return emailProp;

                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2146233087) // ตรวจสอบ HResult เฉพาะเจาะจง
                {
                    MessageBox.Show("พบข้อผิดพลาดของระบบ: ไม่สามารถหา End of Central Directory (EOCD) ของ zip ได้ (ไฟล์ปลายทางอาจจะเป็น .Rar หรือ อื่นๆ)", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if(ex.HResult == -2147024893)
                {
                    MessageBox.Show("พบข้อผิดพลาดของระบบ: กรุณาเช็ค E-data folder\n Details: " + ex.Message , "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("เกิดข้อผิดพลาดของระบบ: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                emailProp.isValid = false;
                return emailProp;
            }
        }

        // One mail may carry at most this much E-data; above it the txt files are split into numbered sub-folders, one mail each.
        private const long CASE13_MAX_MAIL_BYTES = 10L * 1024 * 1024;
        private const int CASE13_MAX_MAIL_FILES = 256;

        // Format 13 (1480Dual txt per serial). For every order row:
        //   1. {customer folder}\yyMMdd-{FEC order}(full Part No)-{sub}   (or without -{sub})
        //   2. ...\yyMMdd-{FEC order}(full Part No)_Edata
        //   3. number of *.txt must equal Qty (serial rows of that test report in shipping)
        // The txt files of all orders go out together; over 10 MB or 256 files they are copied into
        // {customer folder}\yyMMdd-{customer}({product})_Edata-1, -2, ... and every sub-folder is one mail.
        private EmailProperty prepareCase13TxtEdata(EmailProperty emailProp, IEnumerable<DataGridViewRow> shipRows,
                                                    EmailProperty.replaceWord rpWord,
                                                    DataTable dtFile, DataTable dtInfoEmail,
                                                    DataGridView dgvFile, DataGridView dgvInfoEmail)
        {
            string datePrefix = emailProp.shipDate.ToString("yyMMdd");
            List<string> txtFiles = new List<string>();
            List<string> orderNos = new List<string>();

            if (!Directory.Exists(emailProp.folderCustomer))
            {
                MessageBox.Show($"ไม่พบโฟลเดอร์ลูกค้า\n{emailProp.folderCustomer}",
                                "ไม่พบโฟลเดอร์", MessageBoxButtons.OK, MessageBoxIcon.Error);
                emailProp.isValid = false;
                return emailProp;
            }

            foreach (DataGridViewRow dtShipRow in shipRows)
            {
                if (dtShipRow.IsNewRow)
                    continue;

                string fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value?.ToString()?.Trim() ?? "";
                string partNo = dtShipRow.Cells["Part_No"].Value?.ToString()?.Trim() ?? "";
                string subFolder = dtShipRow.Cells["subFolder"].Value?.ToString()?.Trim() ?? "";
                string testReportNo = dtShipRow.Cells["TestReportNo"].Value?.ToString()?.Trim() ?? "";
                string qtyText = dtShipRow.Cells["Qty"].Value?.ToString()?.Trim() ?? "";

                rpWord.customerPN = dtShipRow.Cells["customerPN"].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(rpWord.testReportNo))
                    rpWord.testReportNo = testReportNo;

                string orderName = $"{datePrefix}-{fecOrderNo}({partNo})";
                string orderFolderName = string.IsNullOrEmpty(subFolder) ? orderName : $"{orderName}-{subFolder}";

                // 1. order folder
                string orderFolder = Path.Combine(emailProp.folderCustomer, orderFolderName);
                if (!Directory.Exists(orderFolder))
                    orderFolder = Path.Combine(emailProp.folderCustomer, orderName);

                if (!Directory.Exists(orderFolder))
                {
                    MessageBox.Show($"ไม่พบเทสรีพอตหมายเลข {testReportNo}\n" +
                                    $"ไม่พบโฟลเดอร์: {orderFolderName}\n" +
                                    $"ใน: {emailProp.folderCustomer}",
                                    "ไม่พบเทสรีพอต", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    emailProp.isValid = false;
                    return emailProp;
                }

                // 2. _Edata folder
                string edataFolder = Path.Combine(orderFolder, orderName + "_Edata");
                if (!Directory.Exists(edataFolder))
                {
                    MessageBox.Show($"ไม่พบเทสรีพอตหมายเลข {testReportNo}\n" +
                                    $"ไม่พบโฟลเดอร์: {orderName}_Edata\n" +
                                    $"ใน: {orderFolder}",
                                    "ไม่พบเทสรีพอต", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    emailProp.isValid = false;
                    return emailProp;
                }

                // 3. txt count vs serial rows in shipping ("*.txt" alone would also match e.g. ".txt~")
                string[] txtInOrder = Directory.GetFiles(edataFolder, "*.txt", SearchOption.TopDirectoryOnly)
                    .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                int qty;
                if (!int.TryParse(qtyText, out qty) || txtInOrder.Length != qty)
                {
                    MessageBox.Show($"จำนวนไฟล์ txt ไม่ตรงกับจำนวนในฐานข้อมูล\n" +
                                    $"เทสรีพอตหมายเลข: {testReportNo}\n" +
                                    $"ไฟล์ txt: {txtInOrder.Length}\n" +
                                    $"ฐานข้อมูล (shipping): {qtyText}\n" +
                                    $"โฟลเดอร์: {edataFolder}",
                                    "จำนวนไฟล์ไม่ตรง", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    emailProp.isValid = false;
                    return emailProp;
                }

                txtFiles.AddRange(txtInOrder);
                if (!orderNos.Contains(fecOrderNo))
                    orderNos.Add(fecOrderNo);
            }

            if (txtFiles.Count == 0)
            {
                MessageBox.Show("ไม่พบไฟล์ txt ที่จะส่ง", "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);
                emailProp.isValid = false;
                return emailProp;
            }

            // all orders go into one mail / one set of sub-folders, so a serial name may appear only once
            List<string> duplicateNames = txtFiles
                .GroupBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => string.Join("\n", g))
                .ToList();
            if (duplicateNames.Any())
            {
                MessageBox.Show("พบไฟล์ txt ชื่อซ้ำกันในหลาย order:\n" + string.Join("\n\n", duplicateNames),
                                "ไฟล์ซ้ำ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                emailProp.isValid = false;
                return emailProp;
            }

            // 4. split when over 10 MB or 256 files
            long totalBytes = txtFiles.Sum(f => new FileInfo(f).Length);
            List<List<string>> mails = new List<List<string>>();

            if (totalBytes > CASE13_MAX_MAIL_BYTES || txtFiles.Count > CASE13_MAX_MAIL_FILES)
            {
                List<string> current = new List<string>();
                long currentBytes = 0;
                foreach (string file in txtFiles)
                {
                    long size = new FileInfo(file).Length;
                    if (current.Count > 0 &&
                        (currentBytes + size > CASE13_MAX_MAIL_BYTES || current.Count >= CASE13_MAX_MAIL_FILES))
                    {
                        mails.Add(current);
                        current = new List<string>();
                        currentBytes = 0;
                    }
                    current.Add(file);
                    currentBytes += size;
                }
                mails.Add(current);

                // same input always gives the same split, so a second run only overwrites the same copies
                string splitBaseName = $"{datePrefix}-{rpWord.customerName}({rpWord.productNameInEmail})_Edata";
                for (int m = 0; m < mails.Count; m++)
                {
                    string splitFolder = Path.Combine(emailProp.folderCustomer, $"{splitBaseName}-{m + 1}");
                    Directory.CreateDirectory(splitFolder);

                    List<string> copies = new List<string>();
                    foreach (string file in mails[m])
                    {
                        string target = Path.Combine(splitFolder, Path.GetFileName(file));
                        File.Copy(file, target, true);
                        copies.Add(target);
                        dtFile.Rows.Add(splitFolder, target);
                    }
                    mails[m] = copies;
                }
            }
            else
            {
                mails.Add(txtFiles);
                foreach (string file in txtFiles)
                    dtFile.Rows.Add(Path.GetDirectoryName(file), file);
            }

            emailProp = _emailCon.getEmailAddress(emailProp);

            rpWord.manuFactureNo = string.Join(", ", orderNos);
            rpWord.shipDate = emailProp.shipDate;
            rpWord.formatDate = _emailCon.allFormatDate();
            rpWord.signature = _emailCon.getSignature(emailProp);

            for (int m = 0; m < mails.Count; m++)
            {
                rpWord.Qty = mails[m].Count.ToString();
                // "FolderName" in the body: names (not UNC paths) of the E-data folders in this mail
                rpWord.folderName = string.Join("<BR>", mails[m]
                    .Select(f => Path.GetFileName(Path.GetDirectoryName(f)))
                    .Distinct(StringComparer.OrdinalIgnoreCase));
                rpWord.subject = _emailCon.getSubject(emailProp);
                rpWord.body = _emailCon.getBody(emailProp);
                rpWord.subject = strProcessSubject(rpWord);
                rpWord.body = strProcessBody(rpWord);
                rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                string subject = mails.Count > 1 ? $"{rpWord.subject} ({m + 1}/{mails.Count})" : rpWord.subject;

                dtInfoEmail.Rows.Add(emailProp.toEmailAddresses,
                                     emailProp.ccEmailAddresses,
                                     emailProp.productName,
                                     emailProp.key,
                                     subject,
                                     rpWord.body,
                                     emailProp.fileFormatsId);
            }

            dgvFile.DataSource = dtFile;
            emailProp.dgvFile = dgvFile;

            dgvInfoEmail.DataSource = dtInfoEmail;
            emailProp.dgvInfoEmail = dgvInfoEmail;

            emailProp.isValid = true;
            return emailProp;
        }

        // Format 14 (1480Dual MOLEX, work order Y-3ER4002C). One txt per order, for every order row:
        //   1. (all orders only) test report folders in the customer folder must not outnumber the orders in the database
        //   2. {customer folder}\yyMMdd-{FEC order}(full Part No)-{sub}   (or without -{sub})
        //   3. {Customer PN}_{FEC order without '-'}_{Qty}PCS_{Part No up to the first '-'}_{yyMMdd}-{sub}.txt in it
        //      e.g. NA_301235900610001_10PCS_FRL1441D4045_260512-1.txt
        // Each txt is copied (only the txt) into {Shipping Data}\yyMMdd-MOLEX(48Dual)-n\{its test report folder name}\ -
        // a new -n whenever the next file would pass 10 MB / 256 files (size before zip) - and each -n is zipped next
        // to it as yyMMdd-MOLEX(48Dual)-n.zip with the same layout. One zip = one mail.
        private EmailProperty prepareCase14TxtZip(EmailProperty emailProp, IEnumerable<DataGridViewRow> shipRows, bool checkFolderCount,
                                                  EmailProperty.replaceWord rpWord,
                                                  DataTable dtFile, DataTable dtInfoEmail,
                                                  DataGridView dgvFile, DataGridView dgvInfoEmail)
        {
            string datePrefix = emailProp.shipDate.ToString("yyMMdd");
            List<DataGridViewRow> rows = shipRows.Where(r => !r.IsNewRow).ToList();
            List<string> txtFiles = new List<string>();
            Dictionary<string, int> qtyByFile = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            List<string> orderNos = new List<string>();

            if (!Directory.Exists(emailProp.folderCustomer))
            {
                MessageBox.Show($"ไม่พบโฟลเดอร์ลูกค้า\n{emailProp.folderCustomer}",
                                "ไม่พบโฟลเดอร์", MessageBoxButtons.OK, MessageBoxIcon.Error);
                emailProp.isValid = false;
                return emailProp;
            }

            // 1. more test report folders than orders in the database = something is shipped that the database does not know
            if (checkFolderCount)
            {
                int reportFolders = Directory.GetDirectories(emailProp.folderCustomer).Length;
                if (reportFolders > rows.Count)
                {
                    MessageBox.Show($"จำนวนเทสรีพอตมากกว่าดาต้าเบส\n" +
                                    $"โฟลเดอร์เทสรีพอต: {reportFolders}\n" +
                                    $"ในดาต้าเบส: {rows.Count} order\n" +
                                    $"โฟลเดอร์: {emailProp.folderCustomer}",
                                    "จำนวนเทสรีพอตไม่ตรง", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    emailProp.isValid = false;
                    return emailProp;
                }
            }

            foreach (DataGridViewRow dtShipRow in rows)
            {
                string fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value?.ToString()?.Trim() ?? "";
                string partNo = dtShipRow.Cells["Part_No"].Value?.ToString()?.Trim() ?? "";
                string subFolder = dtShipRow.Cells["subFolder"].Value?.ToString()?.Trim() ?? "";
                string testReportNo = dtShipRow.Cells["TestReportNo"].Value?.ToString()?.Trim() ?? "";
                string qtyText = dtShipRow.Cells["Qty"].Value?.ToString()?.Trim() ?? "";
                string customerPN = dtShipRow.Cells["customerPN"].Value?.ToString()?.Trim() ?? "";

                rpWord.customerPN = customerPN;
                if (string.IsNullOrEmpty(rpWord.testReportNo))
                    rpWord.testReportNo = testReportNo;

                string orderName = $"{datePrefix}-{fecOrderNo}({partNo})";
                string orderFolderName = string.IsNullOrEmpty(subFolder) ? orderName : $"{orderName}-{subFolder}";

                // 2. test report folder
                string orderFolder = Path.Combine(emailProp.folderCustomer, orderFolderName);
                if (!Directory.Exists(orderFolder))
                    orderFolder = Path.Combine(emailProp.folderCustomer, orderName);

                if (!Directory.Exists(orderFolder))
                {
                    MessageBox.Show($"ไม่พบเทสรีพอตหมายเลข {testReportNo}\n" +
                                    $"ไม่พบโฟลเดอร์: {orderFolderName}\n" +
                                    $"ใน: {emailProp.folderCustomer}",
                                    "ไม่พบเทสรีพอต", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    emailProp.isValid = false;
                    return emailProp;
                }

                // 3. txt named after the order - the Qty in the name has to match the serials in shipping
                string mpn = partNo.Split('-')[0];
                string expectedTxt = $"{customerPN}_{fecOrderNo.Replace("-", "")}_{qtyText}PCS_{mpn}_{datePrefix}" +
                                     (string.IsNullOrEmpty(subFolder) ? "" : "-" + subFolder) + ".txt";
                string txtPath = Directory.GetFiles(orderFolder, "*.txt", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(f => Path.GetFileName(f).Equals(expectedTxt, StringComparison.OrdinalIgnoreCase));

                if (txtPath == null)
                {
                    string[] foundTxt = Directory.GetFiles(orderFolder, "*.txt", SearchOption.TopDirectoryOnly)
                        .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                        .Select(Path.GetFileName).ToArray();
                    MessageBox.Show($"ไม่พบไฟล์ดาต้า\n" +
                                    $"เทสรีพอตหมายเลข: {testReportNo}\n" +
                                    $"ชื่อไฟล์ที่ต้องการ: {expectedTxt}\n" +
                                    $"ไฟล์ txt ที่มีอยู่: {(foundTxt.Length == 0 ? "-" : string.Join(", ", foundTxt))}\n" +
                                    $"โฟลเดอร์: {orderFolder}",
                                    "ไม่พบไฟล์ดาต้า", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    emailProp.isValid = false;
                    return emailProp;
                }

                int qty;
                int.TryParse(qtyText, out qty);
                txtFiles.Add(txtPath);
                qtyByFile[txtPath] = qty;
                if (!orderNos.Contains(fecOrderNo))
                    orderNos.Add(fecOrderNo);
            }

            if (txtFiles.Count == 0)
            {
                MessageBox.Show("ไม่พบไฟล์ txt ที่จะส่ง", "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);
                emailProp.isValid = false;
                return emailProp;
            }

            // 4. split by size before zip (same limits as format 13); a file bigger than the limit goes alone
            List<List<string>> groups = new List<List<string>>();
            List<string> current = new List<string>();
            long currentBytes = 0;
            foreach (string file in txtFiles)
            {
                long size = new FileInfo(file).Length;
                if (current.Count > 0 &&
                    (currentBytes + size > CASE13_MAX_MAIL_BYTES || current.Count >= CASE13_MAX_MAIL_FILES))
                {
                    groups.Add(current);
                    current = new List<string>();
                    currentBytes = 0;
                }
                current.Add(file);
                currentBytes += size;
            }
            groups.Add(current);

            // 5. yyMMdd-MOLEX(48Dual)-n\{test report folder}\{txt} - the order folder keeps its name, only its txt goes in -
            //    and the zip gets exactly that layout from this list (never whatever else is lying in the folder)
            string baseName = $"{datePrefix}-{rpWord.customerName}({rpWord.productNameInEmail})";
            List<string> zipPaths = new List<string>();
            for (int g = 0; g < groups.Count; g++)
            {
                string folder = Path.Combine(emailProp.pathShipping, $"{baseName}-{g + 1}");
                string zipPath = folder + ".zip";
                Directory.CreateDirectory(folder);

                using (FileStream zipStream = new FileStream(zipPath, FileMode.Create))
                using (System.IO.Compression.ZipArchive zip =
                           new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create))
                {
                    foreach (string file in groups[g])
                    {
                        string reportFolderName = Path.GetFileName(Path.GetDirectoryName(file));
                        string reportFolder = Path.Combine(folder, reportFolderName);
                        Directory.CreateDirectory(reportFolder);

                        string copy = Path.Combine(reportFolder, Path.GetFileName(file));
                        File.Copy(file, copy, true);

                        zip.CreateEntry(reportFolderName + "/");
                        System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, copy, reportFolderName + "/" + Path.GetFileName(copy),
                                                                                    System.IO.Compression.CompressionLevel.Optimal);
                    }
                }

                // the zip is what goes out: read it back, every test report folder of the group with its txt has to be inside
                int txtEntries, reportFolders;
                using (System.IO.Compression.ZipArchive check = System.IO.Compression.ZipFile.OpenRead(zipPath))
                {
                    txtEntries = check.Entries.Count(e => e.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && e.FullName.Contains("/"));
                    reportFolders = check.Entries.Select(e => e.FullName.Split('/')[0]).Distinct(StringComparer.OrdinalIgnoreCase).Count();
                }
                if (txtEntries != groups[g].Count || reportFolders != groups[g].Count)
                {
                    MessageBox.Show($"ไฟล์ zip ไม่ครบ! พบเทสรีพอต {reportFolders} โฟลเดอร์ / txt {txtEntries} ไฟล์ จากที่ต้องมี {groups[g].Count}\n{zipPath}",
                                    "ข้อมูลไม่ครบ - ยกเลิกการส่ง", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    emailProp.isValid = false;
                    return emailProp;
                }

                zipPaths.Add(zipPath);
                dtFile.Rows.Add(folder, zipPath);
            }

            // 6. one mail per zip
            emailProp = _emailCon.getEmailAddress(emailProp);

            rpWord.manuFactureNo = string.Join(", ", orderNos);
            rpWord.shipDate = emailProp.shipDate;
            rpWord.formatDate = _emailCon.allFormatDate();
            rpWord.signature = _emailCon.getSignature(emailProp);

            for (int g = 0; g < groups.Count; g++)
            {
                rpWord.Qty = groups[g].Sum(f => qtyByFile[f]).ToString();
                rpWord.folderName = Path.GetFileNameWithoutExtension(zipPaths[g]);
                rpWord.subject = _emailCon.getSubject(emailProp);
                rpWord.body = _emailCon.getBody(emailProp);
                rpWord.subject = strProcessSubject(rpWord);
                rpWord.body = strProcessBody(rpWord);
                rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                string subject = groups.Count > 1 ? $"{rpWord.subject}({g + 1}/{groups.Count})" : rpWord.subject;

                dtInfoEmail.Rows.Add(emailProp.toEmailAddresses,
                                     emailProp.ccEmailAddresses,
                                     emailProp.productName,
                                     emailProp.key,
                                     subject,
                                     rpWord.body,
                                     emailProp.fileFormatsId);
            }

            dgvFile.DataSource = dtFile;
            emailProp.dgvFile = dgvFile;

            dgvInfoEmail.DataSource = dtInfoEmail;
            emailProp.dgvInfoEmail = dgvInfoEmail;

            emailProp.isValid = true;
            return emailProp;
        }

        // Format 3 ("New flow of ID=3") for these customers, any product (1480, 980Dual): the zips used to be prepared
        // by program 1084 in Shipping Data\Send, 1012 now prepares them itself.
        private static readonly string[] CASE3_PREPARE_CUSTOMERS = { "MOLEX", "MOLEX-SG", "OPLINK", "OPLINK-SUBMARINE" };

        private static bool IsCase3PrepareCustomer(string customerName)
        {
            return CASE3_PREPARE_CUSTOMERS.Any(c => c.Equals(customerName?.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        //   1. (all orders only) test report folders must not outnumber the orders in the database, and every order
        //      needs its folder yyMMdd-{FEC order}(short Part No)-{sub}
        //   2-4. copy each test report folder (everything except "For QA check") into Shipping Data\yyMMdd-{customer}({product})-n,
        //        a new -n whenever the next folder would pass 10 MB / 256 files - a test report folder is never split
        //   5. zip each -n next to it as yyMMdd-{customer}({product})-n.zip ({test report folder}/{files}, like 1084)
        //   6. one mail per zip, subject and body as the old format 3: "(i/N)" after the subject, folder names in the body
        // Nothing is overwritten: when a -n folder or zip is already there it stops (maybe prepared or sent already).
        private EmailProperty prepareCase3PrepareZip(EmailProperty emailProp, IEnumerable<DataGridViewRow> shipRows, bool checkFolderCount,
                                                     EmailProperty.replaceWord rpWord,
                                                     DataTable dtFile, DataTable dtInfoEmail,
                                                     DataGridView dgvFile, DataGridView dgvInfoEmail)
        {
            string datePrefix = emailProp.shipDate.ToString("yyMMdd");
            string baseName = $"{datePrefix}-{rpWord.customerName}({rpWord.productNameInEmail})";
            List<DataGridViewRow> rows = shipRows.Where(r => !r.IsNewRow).ToList();
            List<string> reportFolders = new List<string>();

            if (!Directory.Exists(emailProp.folderCustomer))
            {
                MessageBox.Show($"ไม่พบโฟลเดอร์ลูกค้า\n{emailProp.folderCustomer}",
                                "ไม่พบโฟลเดอร์", MessageBoxButtons.OK, MessageBoxIcon.Error);
                emailProp.isValid = false;
                return emailProp;
            }

            // 1. more test report folders than orders in the database = something is shipped that the database does not know
            if (checkFolderCount)
            {
                int folderCount = Directory.GetDirectories(emailProp.folderCustomer).Length;
                if (folderCount > rows.Count)
                {
                    MessageBox.Show($"จำนวนเทสรีพอตมากกว่าดาต้าเบส กรุณาตรวจสอบอีกครั้ง\n" +
                                    $"โฟลเดอร์เทสรีพอต: {folderCount}\n" +
                                    $"ในดาต้าเบส: {rows.Count} order\n" +
                                    $"โฟลเดอร์: {emailProp.folderCustomer}",
                                    "จำนวนเทสรีพอตไม่ตรง", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    emailProp.isValid = false;
                    return emailProp;
                }
            }

            // 1. every order has its test report folder
            foreach (DataGridViewRow dtShipRow in rows)
            {
                string fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value?.ToString()?.Trim() ?? "";
                string partNo = dtShipRow.Cells["Part_No"].Value?.ToString()?.Trim() ?? "";
                string subFolder = dtShipRow.Cells["subFolder"].Value?.ToString()?.Trim() ?? "";
                string testReportNo = dtShipRow.Cells["TestReportNo"].Value?.ToString()?.Trim() ?? "";

                string shortPartNo = partNo.Length > 7 ? partNo.Substring(7) : partNo;
                string orderName = $"{datePrefix}-{fecOrderNo}({shortPartNo})";
                string orderFolderName = string.IsNullOrEmpty(subFolder) ? orderName : $"{orderName}-{subFolder}";

                string orderFolder = Path.Combine(emailProp.folderCustomer, orderFolderName);
                if (!Directory.Exists(orderFolder))
                    orderFolder = Path.Combine(emailProp.folderCustomer, orderName);

                if (!Directory.Exists(orderFolder))
                {
                    MessageBox.Show($"ไม่พบเทสรีพอตหมายเลข {testReportNo}\n" +
                                    $"ไม่พบโฟลเดอร์: {orderFolderName}\n" +
                                    $"ใน: {emailProp.folderCustomer}",
                                    "ไม่พบเทสรีพอต", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    emailProp.isValid = false;
                    return emailProp;
                }

                if (!reportFolders.Contains(orderFolder, StringComparer.OrdinalIgnoreCase))
                    reportFolders.Add(orderFolder);
            }

            // prepared (or sent) already? never overwrite - the user checks and deletes it first
            Regex ownOutput = new Regex("^" + Regex.Escape(baseName) + @"-\d+(\.zip)?$", RegexOptions.IgnoreCase);
            string[] existing = Directory.GetFileSystemEntries(emailProp.pathShipping)
                .Select(Path.GetFileName)
                .Where(n => ownOutput.IsMatch(n))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (existing.Length > 0)
            {
                MessageBox.Show($"พบข้อมูลที่เตรียมไว้แล้ว อาจเตรียมหรือส่ง E-data ของ shipment นี้ไปแล้ว\n" +
                                $"กรุณาตรวจสอบ ถ้าจะเตรียมใหม่ให้ลบรายการเหล่านี้ก่อน:\n" +
                                string.Join("\n", existing) + "\n" +
                                $"ใน: {emailProp.pathShipping}",
                                "มีข้อมูลที่เตรียมไว้แล้ว", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                emailProp.isValid = false;
                return emailProp;
            }

            // same order as 1084 (folder name), files of each test report folder without "For QA check"
            reportFolders.Sort(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, List<string>> filesByFolder = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (string reportFolder in reportFolders)
            {
                List<string> files = Directory.GetFiles(reportFolder, "*", SearchOption.AllDirectories)
                    .Where(f => !f.Substring(reportFolder.Length).TrimStart('\\').Split('\\')
                                  .Any(part => part.Equals("For QA check", StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (files.Count == 0)
                {
                    MessageBox.Show($"ไม่พบไฟล์ดาต้าในโฟลเดอร์เทสรีพอต\n{reportFolder}",
                                    "ไม่พบไฟล์ดาต้า", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    emailProp.isValid = false;
                    return emailProp;
                }
                filesByFolder[reportFolder] = files;
            }

            // 4. whole test report folders per -n (same limits as format 13); one bigger than the limit goes alone
            List<List<string>> groups = new List<List<string>>();
            List<string> current = new List<string>();
            long currentBytes = 0;
            int currentFiles = 0;
            foreach (string reportFolder in reportFolders)
            {
                long size = filesByFolder[reportFolder].Sum(f => new FileInfo(f).Length);
                int count = filesByFolder[reportFolder].Count;
                if (current.Count > 0 &&
                    (currentBytes + size > CASE13_MAX_MAIL_BYTES || currentFiles + count > CASE13_MAX_MAIL_FILES))
                {
                    groups.Add(current);
                    current = new List<string>();
                    currentBytes = 0;
                    currentFiles = 0;
                }
                current.Add(reportFolder);
                currentBytes += size;
                currentFiles += count;
            }
            groups.Add(current);

            // 2, 3, 5. yyMMdd-{customer}({product})-n\{test report folder}\... and the zip of exactly those files
            List<string> zipPaths = new List<string>();
            for (int g = 0; g < groups.Count; g++)
            {
                string folder = Path.Combine(emailProp.pathShipping, $"{baseName}-{g + 1}");
                string zipPath = folder + ".zip";
                int expectedFiles = 0;

                using (FileStream zipStream = new FileStream(zipPath, FileMode.CreateNew))
                using (System.IO.Compression.ZipArchive zip =
                           new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create))
                {
                    foreach (string reportFolder in groups[g])
                    {
                        string reportFolderName = Path.GetFileName(reportFolder);
                        foreach (string file in filesByFolder[reportFolder])
                        {
                            string relative = file.Substring(reportFolder.Length).TrimStart('\\');
                            string copy = Path.Combine(folder, reportFolderName, relative);
                            Directory.CreateDirectory(Path.GetDirectoryName(copy));
                            File.Copy(file, copy, false);

                            System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, copy,
                                reportFolderName + "/" + relative.Replace('\\', '/'),
                                System.IO.Compression.CompressionLevel.Optimal);
                            expectedFiles++;
                        }
                    }
                }

                // the zip is what goes out: read it back, every test report folder and every file has to be inside
                int entries, zipReportFolders;
                using (System.IO.Compression.ZipArchive check = System.IO.Compression.ZipFile.OpenRead(zipPath))
                {
                    entries = check.Entries.Count(e => !e.FullName.EndsWith("/"));
                    zipReportFolders = check.Entries.Select(e => e.FullName.Split('/')[0]).Distinct(StringComparer.OrdinalIgnoreCase).Count();
                }
                if (entries != expectedFiles || zipReportFolders != groups[g].Count)
                {
                    MessageBox.Show($"ไฟล์ zip ไม่ครบ! พบเทสรีพอต {zipReportFolders}/{groups[g].Count} โฟลเดอร์, ไฟล์ {entries}/{expectedFiles}\n{zipPath}",
                                    "ข้อมูลไม่ครบ - ยกเลิกการส่ง", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    emailProp.isValid = false;
                    return emailProp;
                }

                zipPaths.Add(zipPath);
                dtFile.Rows.Add(folder, zipPath);
            }

            // 6. one mail per zip, same subject / body handling as the old format 3
            emailProp = _emailCon.getEmailAddress(emailProp);
            rpWord.shipDate = emailProp.shipDate;
            rpWord.formatDate = _emailCon.allFormatDate();
            rpWord.signature = _emailCon.getSignature(emailProp);

            for (int g = 0; g < zipPaths.Count; g++)
            {
                rpWord.folderName = fileCls.GetZipDirectoryContents(zipPaths[g]).Replace(datePrefix + "-", "");
                rpWord.subject = _emailCon.getSubject(emailProp);
                rpWord.body = _emailCon.getBody(emailProp);

                rpWord.subject = strProcessSubject(rpWord);
                if (zipPaths.Count > 1)
                    rpWord.subject = rpWord.subject + "(" + (g + 1) + "/" + zipPaths.Count + ")";
                rpWord.body = strProcessBody(rpWord);
                rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName,
                                     emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);
            }

            dgvFile.DataSource = dtFile;
            emailProp.dgvFile = dgvFile;

            dgvInfoEmail.DataSource = dtInfoEmail;
            emailProp.dgvInfoEmail = dgvInfoEmail;

            emailProp.isValid = true;
            return emailProp;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------- Email Selection -----------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------
        public EmailProperty prepareFileInfoEmailSelection(EmailProperty emailProp)
        {
            try
            {
                int subCount;
                DataGridView dgvFile = new DataGridView();
                DataGridView dgslFile = new DataGridView();
                DataGridView dgwfFile = new DataGridView();

                DataGridView dgvInfoEmail = new DataGridView();
                EmailCls emailCls = new EmailCls();
                FileCls fileCls = new FileCls();

                //emailProp = _emailCon.getEmailAddress(emailProp);

                DataTable dtInfoEmail = new DataTable();
                dtInfoEmail.Columns.Add("toEmailAddresses", typeof(List<string>));
                dtInfoEmail.Columns.Add("ccEmailAddresses", typeof(List<string>));
                dtInfoEmail.Columns.Add("productName", typeof(string));
                dtInfoEmail.Columns.Add("key_id", typeof(string));
                dtInfoEmail.Columns.Add("subject", typeof(string));
                dtInfoEmail.Columns.Add("body", typeof(string));
                dtInfoEmail.Columns.Add("FILE_FORMATS_ID", typeof(string));

                DataTable dtFile = new DataTable();

                dtFile.Columns.Add("folder", typeof(string));
                dtFile.Columns.Add("attached", typeof(string));

                DataTable dtslFile = new DataTable();
                dtslFile.Columns.Add("folder", typeof(string));
                dtslFile.Columns.Add("attached", typeof(string));

                DataTable dtwfFile = new DataTable();
                dtwfFile.Columns.Add("folder", typeof(string));
                dtwfFile.Columns.Add("attached", typeof(string));

                EmailProperty.replaceWord rpWord = new EmailProperty.replaceWord();

                shipProp.customerName = emailProp.selectedRowCustomer["customerName"].ToString();
                shipProp.dbProduct = emailProp.selectedRowProduct["DATABASE_NAME"].ToString();
                shipProp.productName = emailProp.selectedRowProduct["PRODUCT_NAME"].ToString();
                shipProp.key = emailProp.selectedRowCustomer["KEY_ID"].ToString();
                shipProp.productNameInEmail = emailProp.selectedRowProduct["NAME_IN_FileServer"].ToString();
                emailProp.productName = shipProp.productName;
                emailProp.key = shipProp.key;
                emailProp.fileFormatsId = emailProp.selectedRowCustomer["FILE_FORMATS_ID"].ToString();
                emailProp.pathShipping = emailProp.selectedRowProduct["PATH_SHIPPING"].ToString();

                // แสดงผลลัพธ์ใน LabelName
                if (emailProp.pathShipping.EndsWith("\\"))
                {
                    // ถ้าลงท้ายด้วย backslash ไม่ต้องทำอะไร
                }
                else
                {
                    //เติม backslash
                    emailProp.pathShipping += "\\";
                }


                rpWord.productName = shipProp.productName;
                rpWord.productNameInEmail = emailProp.selectedRowProduct["NAME_IN_EMAIL"].ToString();
                rpWord.productID = emailProp.selectedRowProduct["PRODUCT_ID"].ToString();
                rpWord.pathPO = emailProp.selectedRowProduct["PATH_PO_ALL"].ToString();
                rpWord.customerName = emailProp.selectedRowCustomer["customerName"].ToString();
                
                // subject เอาไว้ท้าย เพื่อรอนับ
                rpWord.subject = _emailCon.getSubject(emailProp);
                rpWord.body = _emailCon.getBody(emailProp);
                rpWord.signature = _emailCon.getSignature(emailProp);

                emailProp.pathShipping = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-Shipping Data" + "\\";
                emailProp.folderCustomer = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")\\";

                string[] Folders = Directory.GetDirectories(emailProp.pathShipping);

                List<string> zipFileNames = new List<string>();
                int totalZipFiles = 0;
                int count = 1;
                int i = 0;

                string dateToMatch = "";

                string shippingPathLD = "";
                string shippingPathHSLD = "";
                string shippingPathSerialList = "";
                string shippingPathWafer = "";

                string[] srcPath;
                string[] spFile;
                string[] folderEdata;
                string[] folderCustomer;
                string[] folderSerialList;
                string[] folderWafer;

                string[] slFile;
                string[] wfFile;


                switch (emailProp.fileFormatsId)
                {

                    //CSV
                    case "1":
                        return emailProp;
                    //XLS XLSX
                    case "2":

                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.SelectedRows)
                        {
                            shipProp.subFolder = dtShipRow.Cells["subFolder"].Value.ToString();
                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            emailProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            dateToMatch = emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + emailProp.partNo.Substring(7) + ")-" + shipProp.subFolder;

                            rpWord.shipDate = emailProp.shipDate;
                            rpWord.formatDate = _emailCon.allFormatDate();
                            rpWord.subject = _emailCon.getSubject(emailProp);
                            rpWord.body = _emailCon.getBody(emailProp);
                            rpWord.signature = _emailCon.getSignature(emailProp);

                            rpWord.subject = strProcessSubject(rpWord);
                            rpWord.body = strProcessBody(rpWord);
                            rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                            var filteredSubfolders = Folders
                            .SelectMany(folder => Directory.GetDirectories(folder)
                                                            .Where(subfolder => Path.GetFileName(subfolder)?.StartsWith(dateToMatch) ?? false))
                            .ToList();

                            bool fileFound = false; // ตัวแปรตรวจสอบว่าพบไฟล์หรือไม่

                            foreach (string subfolder in filteredSubfolders)
                            {
                                string[] files = Directory.GetFiles(subfolder);
                                string targetFileName = emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + emailProp.partNo.Substring(7) + ")";
                                string filePath = Path.Combine(subfolder, targetFileName + ".xls");

                                if (shipProp.subFolder == "")
                                {
                                    var fi = Directory.EnumerateFiles(subfolder, "*P)-data*");
                                    if (fi.Any())
                                    {
                                        string foundFile = fi.FirstOrDefault();
                                        if (File.Exists(foundFile))
                                        {
                                            dtFile.Rows.Add(subfolder, foundFile);
                                            fileFound = true;
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    var fi = Directory.EnumerateFiles(subfolder, "*P)-data-" + shipProp.subFolder);
                                    if (fi.Any())
                                    {
                                        string foundFile = fi.FirstOrDefault();
                                        if (File.Exists(foundFile))
                                        {
                                            dtFile.Rows.Add(subfolder, foundFile);
                                            fileFound = true;
                                            continue;
                                        }
                                    }
                                }

                                string extension = Path.GetExtension(filePath);
                                if (extension == "")
                                {
                                    filePath = Path.Combine(subfolder, targetFileName + ".xlsx");
                                    if (File.Exists(filePath))
                                    {
                                        dtFile.Rows.Add(subfolder, filePath);
                                        fileFound = true;
                                        continue;
                                    }
                                }
                                else
                                {
                                    filePath = Path.Combine(subfolder, targetFileName + ".xls");
                                    if (File.Exists(filePath))
                                    {
                                        dtFile.Rows.Add(subfolder, filePath);
                                        fileFound = true;
                                        continue;
                                    }
                                }
                            }

                            // ตรวจสอบว่าไม่พบไฟล์สำหรับ order นี้
                            if (!fileFound)
                            {
                                string errorMessage = $"ไม่พบไฟล์ข้อมูลสำหรับ Order: {shipProp.fecOrderNo}\n" +
                                                     $"Part No: {emailProp.partNo}\n" +
                                                     $"Sub Folder: {shipProp.subFolder}\n" +
                                                     $"วันที่: {emailProp.shipDate.ToString("yyMMdd")}\n\n" +
                                                     $"กรุณาตรวจสอบไฟล์ในโฟลเดอร์: {emailProp.folderCustomer}";

                                MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (dtFile.Rows.Count == emailProp.dgvShip.SelectedRows.Count)
                            {
                                break;
                            }
                        }
                        emailProp = _emailCon.getEmailAddress(emailProp);

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);
                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;

                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;
                        emailProp.isValid = true;
                        return emailProp;
                    //ZIP , //RAR เก็บ zip เฉยๆ
                    case "3":

                        // MOLEX / MOLEX-SG / OPLINK / OPLINK-SUBMARINE, selected orders only (no folder-count check)
                        if (IsCase3PrepareCustomer(rpWord.customerName))
                            return prepareCase3PrepareZip(emailProp, emailProp.dgvShip.SelectedRows.Cast<DataGridViewRow>()
                                                                          .OrderBy(r => r.Index), false,
                                                          rpWord, dtFile, dtInfoEmail, dgvFile, dgvInfoEmail);

                        subCount = 1;

                        emailProp.pathShipping = Path.Combine(emailProp.pathShipping , "Send");

                        // เรียกใช้ฟังก์ชันเพื่อเก็บชื่อไฟล์และนับจำนวนไฟล์ zip
                        //List<string> zipFileNames = new List<string>();

                        // ระบุ path ที่ต้องการตรวจสอบ , ระบุชื่อเริ่มต้นของไฟล์ zip
                        totalZipFiles = fileCls.CountZipFilesInDirectory(emailProp.pathShipping, emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")", zipFileNames);
                        
                        //if (totalZipFiles != emailProp.dgvShip.SelectedRows.Count)
                        //{
                        //    MessageBox.Show("file zip กับจำนวน test report ใน Database ไม่เท่ากัน กรุณาเช็ค path  " + emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")");
                        //    emailProp.isValid = false;
                        //    return emailProp;
                        //}

                        if (totalZipFiles == 0)
                        {
                            //MessageBox.Show("ไม่พบ file zip ที่  " + emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")");
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        List<string> selectedFolder = new List<string>();
                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.SelectedRows)
                        {
                            shipProp.subFolder = dtShipRow.Cells["subFolder"].Value.ToString();
                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            //shipProp.qty = dtShipRow.Cells["qty"].Value.ToString();

                            emailProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            dateToMatch = emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + emailProp.partNo.Substring(7) + ")-" + shipProp.subFolder;

                            //string FullName = Path.Combine(emailProp.folderCustomer, dateToMatch);
                            selectedFolder.Add(dateToMatch);

                        }

                        List<string> pathZip = new List<string>();

                        foreach (string zf in zipFileNames)
                        {
                            pathZip.Add(Path.Combine(emailProp.pathShipping, zf));
                        }

                        DataTable foundFolder = fileCls.CheckFoldersInZip(selectedFolder, pathZip);

                        if (foundFolder.Rows.Count > 0)
                        {
                            //MessageBox.Show("All selected files were found.");

                            // Do something with resultTable
                        }
                        else
                        {
                            //MessageBox.Show("No files were found.");
                            emailProp.isValid = false;

                            return emailProp;
                        }



                        foreach (DataRow df in foundFolder.Rows)
                        {
                            //
                            rpWord.folderName = fileCls.GetZipDirectoryContents(emailProp.pathShipping + "\\" + df["folder"].ToString());
                            rpWord.folderName = rpWord.folderName.Replace(emailProp.shipDate.ToString("yyMMdd") + "-", "");

                            dtFile.Rows.Add(emailProp.pathShipping + "\\" + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")", emailProp.pathShipping + "\\" + df["folder"].ToString());
                            // add subject count / zipfilecount

                            emailProp = _emailCon.getEmailAddress(emailProp);

                            rpWord.shipDate = emailProp.shipDate;
                            rpWord.formatDate = _emailCon.allFormatDate();
                            rpWord.subject = _emailCon.getSubject(emailProp);
                            rpWord.body = _emailCon.getBody(emailProp);
                            rpWord.signature = _emailCon.getSignature(emailProp);

                            if (foundFolder.Rows.Count > 1)
                            {
                                rpWord.subject = strProcessSubject(rpWord);
                                rpWord.subject = rpWord.subject + "(" + count + "/" + zipFileNames.Count + ")";
                            }
                            else
                            {
                                rpWord.subject = strProcessSubject(rpWord);
                            }

                            rpWord.body = strProcessBody(rpWord);
                            rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                            dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);
                            count++;
                        }

                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;


                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;
                        emailProp.isValid = true;

                        return emailProp;

                    //TXT
                    case "4":

                        // 1 เมล /1 spec /1 sub / หลายไฟล์ 
                        subCount = 1;


                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.SelectedRows)
                        {
                            DataTable dtSerial = new DataTable();

                            emailProp.productName = shipProp.productName;
                            emailProp.key = dtShipRow.Cells["KEY_ID"].Value.ToString();
                            rpWord.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            //fecorderNo or ManuFactureNo

                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            shipProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();


                            string folderMain = emailProp.folderCustomer + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo.Substring(7) + ")-" + subCount.ToString() + "\\";

                            dtSerial = _shipCon.SearchSerialShipping(shipProp);

                            //bool noFilesFound = true; // สร้างตัวแปรเพื่อเก็บค่าว่าไม่มีไฟล์ไหน
                            string message = "";


                            foreach (DataRow row in dtSerial.Rows)
                            {
                                string serial = row["serial"].ToString();
                                string file = folderMain + shipProp.customerPN + "_" + serial + ".txt";
                                if (File.Exists(file))
                                {
                                    // มีไฟล์อยู่
                                    //message += "ไฟล์ " + file + " มีอยู่\n";
                                    dtFile.Rows.Add(shipProp.customerPN + "_" + serial + ".txt", folderMain + shipProp.customerPN + "_" + serial + ".txt");
                                }
                                else
                                {
                                    // ไม่มีไฟล์อยู่
                                    message += "ไฟล์ " + file + " ไม่มีอยู่\n";
                                    emailProp.isValid = false;
                                }

                                //dtFile.Rows.Add(shipProp.customerPN + "_" + serial + ".txt" , folderMain + shipProp.customerPN + "_" + serial + ".txt");


                            }

                            if (emailProp.isValid == false)
                            {
                                MessageBox.Show(message, "สรุปการตรวจสอบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return emailProp;
                            }

                            //subject
                            rpWord.Qty = dtSerial.Rows.Count.ToString();
                            rpWord.shipDate = emailProp.shipDate;
                            rpWord.formatDate = _emailCon.allFormatDate();
                            rpWord.subject = _emailCon.getSubject(emailProp);
                            //body
                            rpWord.body = _emailCon.getBody(emailProp);
                            rpWord.signature = _emailCon.getSignature(emailProp);

                            rpWord.subject = strProcessSubject(rpWord);
                            rpWord.body = strProcessBody(rpWord);
                            rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;
                            //to
                            //cc
                            emailProp = _emailCon.getEmailAddress(emailProp);


                            dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);


                        }

                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;



                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;
                        emailProp.isValid = true;
                        return emailProp;

                    //zip file ส่ง
                    case "5":

                        subCount = 1;
                        //DataTable dtFile = new DataTable();

                        //dtFile.Columns.Add("file", typeof(string));
                        //dtFile.Columns.Add("attached", typeof(string));

                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.SelectedRows)
                        {

                            emailProp.productName = shipProp.productName;
                            emailProp.key = dtShipRow.Cells["KEY_ID"].Value.ToString();
                            rpWord.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            //fecorderNo or ManuFactureNo

                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            shipProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            //string folderMain = emailProp.folderCustomer + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo.Substring(7) + ")" + "\\";
                            if (fileCls.CompressFileManage(shipProp, emailProp) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            rpWord.shipDate = emailProp.shipDate;
                            rpWord.formatDate = _emailCon.allFormatDate();
                            rpWord.subject = _emailCon.getSubject(emailProp);
                            //body
                            rpWord.body = _emailCon.getBody(emailProp);
                            rpWord.signature = _emailCon.getSignature(emailProp);

                            rpWord.subject = strProcessSubject(rpWord);
                            rpWord.body = strProcessBody(rpWord);
                            rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;
                            //to
                            //cc
                            emailProp = _emailCon.getEmailAddress(emailProp);


                            dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                            dtFile.Rows.Add(emailProp.pathShipping, emailProp.folderCustomer.TrimEnd('\\') + ".zip");
                            dgvFile.DataSource = dtFile;
                            emailProp.dgvFile = dgvFile;



                            dgvInfoEmail.DataSource = dtInfoEmail;
                            emailProp.dgvInfoEmail = dgvInfoEmail;
                            emailProp.isValid = true;
                            break;

                        }

                        return emailProp;

                    case "6":

                        //emailProp.dgvShip.Sort(emailProp.dgvShip.Columns["manuFactureNo"], ListSortDirection.Ascending);
                        if (shipProp.customerName.ToUpper() != "HISILICON" && shipProp.customerName.ToUpper() != "HUAWEI")
                        {
                            MessageBox.Show("case นี้ สำหรับ Hisilicon/Huawei เท่านั้น", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        shippingPathHSLD = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-HSLD_Data\\";
                        shippingPathSerialList = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-SerialList\\";
                        shippingPathWafer = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-Wafer\\";




                        if (!Directory.Exists(shippingPathHSLD))
                        {
                            Directory.CreateDirectory(shippingPathHSLD);
                        }
                        else
                        {
                            Directory.Delete(shippingPathHSLD, true);
                            Directory.CreateDirectory(shippingPathHSLD);
                        }

                        if (!Directory.Exists(shippingPathSerialList))
                        {
                            Directory.CreateDirectory(shippingPathSerialList);
                        }
                        else
                        {
                            Directory.Delete(shippingPathSerialList, true);
                            Directory.CreateDirectory(shippingPathSerialList);
                        }

                        if (!Directory.Exists(shippingPathWafer))
                        {
                            Directory.CreateDirectory(shippingPathWafer);
                        }
                        else
                        {
                            Directory.Delete(shippingPathWafer, true);
                            Directory.CreateDirectory(shippingPathWafer);
                        }

                        folderEdata = Directory.GetDirectories(shippingPathHSLD);
                        folderSerialList = Directory.GetDirectories(shippingPathSerialList);
                        folderWafer = Directory.GetDirectories(shippingPathWafer);
                        folderCustomer = Directory.GetDirectories(emailProp.folderCustomer);


                        //   if (folderCustomer.Length != emailProp.dgvShip.Rows.Count)
                        //   {
                        //       MessageBox.Show("จำนวน order กับ จำนวน folder ของดาต้าไม่เท่ากัน " + emailProp.folderCustomer);
                        //       emailProp.isValid = false;
                        //       return emailProp;
                        //   }

                        srcPath = new string[emailProp.dgvShip.SelectedRows.Count];
                        spFile = new string[emailProp.dgvShip.SelectedRows.Count];
                        slFile = new string[emailProp.dgvShip.SelectedRows.Count];
                        wfFile = new string[emailProp.dgvShip.SelectedRows.Count];

                        count = 0;
                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.SelectedRows)
                        {

                            rpWord.shipDate = emailProp.shipDate;

                            emailProp.productName = shipProp.productName;

                            emailProp.key = dtShipRow.Cells["KEY_ID"].Value.ToString();
                            rpWord.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            //fecorderNo or ManuFactureNo

                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            shipProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            emailProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            shipProp.qty = dtShipRow.Cells["Qty"].Value.ToString();

                            string edata = Path.Combine(folderCustomer[count], "Edata");
                            var fi = Directory.EnumerateFiles(edata, emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo + ")" + "*");

                            if (fi.Any())
                            {
                                spFile[count] = fi.FirstOrDefault();

                                emailProp.isValid = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบ order or file Edata นี้ " + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (fileCls.fnCopyFile(spFile[count], shippingPathHSLD) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            var wf = Directory.EnumerateFiles(edata, emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo + ")" + "-wafer-*");

                            if (wf.Any())
                            {
                                wfFile[count] = wf.FirstOrDefault();

                                emailProp.isValid = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบ order or file Edata นี้ " + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (fileCls.fnCopyFile(wfFile[count], shippingPathWafer) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            var sl = Directory.EnumerateFiles(edata, emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "-" + shipProp.qty + "*");

                            if (sl.Any())
                            {
                                slFile[count] = sl.FirstOrDefault();
                                emailProp.isValid = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบ order or serialList นี้ " + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "-" + shipProp.qty, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (fileCls.fnCopyFile(slFile[count], shippingPathSerialList) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }
                            count++;
                        }


                        //// Data 
                        for (count = 0; count < spFile.Length; count++)
                        {
                            // ดึงไฟล์จากลิสต์โดยใช้ดัชนี
                            dtFile.Rows.Add(shippingPathHSLD, spFile[count]);
                        }
                        emailProp = _emailCon.getEmailAddress(emailProp);

                        rpWord.shipDate = emailProp.shipDate;
                        rpWord.formatDate = _emailCon.allFormatDate();
                        rpWord.subject = _emailCon.getSubject(emailProp);
                        rpWord.body = _emailCon.getBody(emailProp);
                        rpWord.signature = _emailCon.getSignature(emailProp);

                        rpWord.subject = strProcessSubject(rpWord);
                        rpWord.body = strProcessBody(rpWord);
                        rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                        for (count = 0; count < slFile.Length; count++)
                        {
                            // ดึงไฟล์จากลิสต์โดยใช้ดัชนี
                            dtslFile.Rows.Add(shippingPathSerialList, slFile[count]);
                        }

                        emailProp.toEmailAddresses = GetDocumentRecipients(shipProp, DOC_SERIAL_LIST, "SERIAL_LIST", "to");
                        if (emailProp.toEmailAddresses == null || emailProp.toEmailAddresses.Count == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.ccEmailAddresses = GetDocumentRecipients(shipProp, DOC_SERIAL_LIST, "SERIAL_LIST", "cc");
                        if (emailProp.ccEmailAddresses == null || emailProp.ccEmailAddresses.Count == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        rpWord.subject = "Serial list for Hisilicon" + "(" + shipProp.productNameInEmail + ") " + emailProp.shipDate.ToString("yyyy-MM-dd");
                        rpWord.body = "We attached file(s) Serial list for Hisilicon in this e-mail";
                        rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                        for (count = 0; count < wfFile.Length; count++)
                        {
                            // ดึงไฟล์จากลิสต์โดยใช้ดัชนี
                            dtwfFile.Rows.Add(shippingPathWafer, wfFile[count]);
                        }

                        emailProp.toEmailAddresses = GetDocumentRecipients(shipProp, DOC_WAFER, "WAFER", "to");
                        if (emailProp.toEmailAddresses == null || emailProp.toEmailAddresses.Count == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.ccEmailAddresses = GetDocumentRecipients(shipProp, DOC_WAFER, "WAFER", "cc");
                        if (emailProp.ccEmailAddresses == null || emailProp.ccEmailAddresses.Count == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }


                        rpWord.subject = "WAFER data " + emailProp.shipDate.ToString("dd/MMM").ToUpper() + " from Furukawa for Hisilicon";
                        rpWord.body = "We attached Wafer data for Hisilicon in this e-mail";
                        rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;

                        dgslFile.DataSource = dtslFile;
                        emailProp.dgslFile = dgslFile;

                        dgwfFile.DataSource = dtwfFile;
                        emailProp.dgwfFile = dgwfFile;

                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;
                        emailProp.isValid = true;



                        return emailProp;

                    case "7":

                        //emailProp.dgvShip.Sort(emailProp.dgvShip.Columns["manuFactureNo"], ListSortDirection.Ascending);

                        shippingPathLD = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-LD_Data\\";
                        shipProp.shipdate = emailProp.shipDate.ToString("yyyy-MM-dd");


                     

                        if (!Directory.Exists(shippingPathLD))
                        {
                            Directory.CreateDirectory(shippingPathLD);
                        }
                        else
                        {
                            Directory.Delete(shippingPathLD, true);
                            Directory.CreateDirectory(shippingPathLD);
                        }

                        folderCustomer = Directory.GetDirectories(emailProp.folderCustomer);

                        i = 0;

                        srcPath = new string[emailProp.dgvShip.SelectedRows.Count];
                        spFile = new string[emailProp.dgvShip.SelectedRows.Count];

                        //   if (folderCustomer.Length != emailProp.dgvShip.Rows.Count)
                        //   {
                        //       MessageBox.Show("จำนวน order กับ จำนวน folder ของดาต้าไม่เท่ากัน " + emailProp.folderCustomer);
                        //       emailProp.isValid = false;
                        //       return emailProp;
                        //   }

                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.SelectedRows)
                        {

                            emailProp.productName = shipProp.productName;
                            emailProp.key = dtShipRow.Cells["KEY_ID"].Value.ToString();
                            rpWord.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            shipProp.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            //fecorderNo or ManuFactureNo

                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            shipProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();
                            emailProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            shipProp.subFolder = dtShipRow.Cells["subFolder"].Value.ToString();

                            dateToMatch = emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + emailProp.partNo.Substring(7) + ")-" + shipProp.subFolder;
                            
                            var filteredSubfolders = Folders
                             .SelectMany(folder => Directory.GetDirectories(folder)
                                 .Where(subfolder =>
                                     string.Equals(Path.GetFileName(subfolder), dateToMatch, StringComparison.OrdinalIgnoreCase)))
                             .ToList();

                            if (!filteredSubfolders.Any())
                            {
                                MessageBox.Show(
                                    $"ไม่พบโฟลเดอร์ข้อมูลสำหรับ TestReportNo: {shipProp.testReportNo}\n" +
                                    $"Order: {shipProp.fecOrderNo}\n" +
                                    $"Sub Folder: {shipProp.subFolder}\n" +
                                    $"ชื่อโฟลเดอร์ที่ต้องการ: {dateToMatch}\n\n" +
                                    $"กรุณาตรวจสอบไฟล์ในโฟลเดอร์: {emailProp.folderCustomer}",
                                    "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            foreach (string subfolder in filteredSubfolders)
                            {
                                srcPath[i] = subfolder;
                                DataTable dtSerial = _shipCon.SearchSerialShippingCust(shipProp);
                                if (dtSerial == null)
                                {
                                    MessageBox.Show("พบปัญหาเกี่ยวกับ serial ใน database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else if (dtSerial.Rows.Count == 0)
                                {
                                    MessageBox.Show("ไม่มี serial ใน database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }

                                //foreach (DataRow r in dtSerial.Rows)
                                //{
                                //string serial = r["serial"].ToString();
                                //string fileName = serial + ".jpg";
                                string jpgPath = Path.Combine(subfolder, "JPEG file");

                                if (Directory.Exists(jpgPath))
                                {
                                    emailProp.isValid = true;
                                }
                                else
                                {
                                    MessageBox.Show("ไม่พบ path jpg " + jpgPath + "โปรด check ตำแหน่งนี้", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    emailProp.isValid = false;
                                    return emailProp;
                                }
                                //}

                                if (shipProp.partNo.Contains("QQA-617") || shipProp.partNo.Contains("QQA-657"))
                                {
                                    string sIfImPath = Path.Combine(subfolder, "If-Im data");
                                    if (Directory.Exists(sIfImPath))
                                    {
                                        emailProp.isValid = true;
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Not found If-Im data in Order {shipProp.partNo}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        emailProp.isValid = false;
                                        return emailProp;
                                    }
                                }

                                var fi = Directory.EnumerateFiles(subfolder, shipProp.customerPN + "*");
                                if (fi.Any())
                                {
                                    spFile[i] = fi.FirstOrDefault();
                                    emailProp.isValid = true;
                                }
                                else
                                {
                                    MessageBox.Show("ไม่พบ order or file นี้ " + dateToMatch, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    emailProp.isValid = false;
                                    return emailProp;
                                }

                            }


                            i++;

                        }


                        if (fileCls.fnPrepareAccelinkDirect(srcPath, shippingPathLD, spFile, emailProp.dgvShip.SelectedRows.Count) == false)
                        {
                            MessageBox.Show("ทำการ copy ก่อนเปลี่ยนชื่อไม่สำเร็จ " + shippingPathLD, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        if (fileCls.fnCopyEData(shippingPathLD, emailProp.shipDate) == false)
                        {
                            MessageBox.Show("ทำการ copy ก่อนเปลี่ยนชื่อไม่สำเร็จ " + shippingPathLD, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        folderEdata = Directory.GetDirectories(shippingPathLD);

                        foreach (string ffe in folderEdata)
                        {
                            if (fileCls.CompressFileZip(ffe, ffe) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }
                        }



                        // เรียกใช้ฟังก์ชันเพื่อเก็บชื่อไฟล์และนับจำนวนไฟล์ zip
                        //List<string> zipFileNames = new List<string>();

                        // ระบุ path ที่ต้องการตรวจสอบ , ระบุชื่อเริ่มต้นของไฟล์ zip
                        totalZipFiles = fileCls.CountZipFilesInDirectory(shippingPathLD, "Edata_" + emailProp.shipDate.ToString("yyyyMMdd"), zipFileNames);

                        if (totalZipFiles == 0)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }



                        foreach (string file in zipFileNames)
                        {

                            //rpWord.folderName = fileCls.GetZipDirectoryContents(shippingPathLD + "\\" + file);

                            dtFile.Rows.Add(shippingPathLD, shippingPathLD + "\\" + file);
                            // add subject count / zipfilecount

                            emailProp = _emailCon.getEmailAddress(emailProp);

                            rpWord.shipDate = emailProp.shipDate;
                            rpWord.formatDate = _emailCon.allFormatDate();
                            rpWord.subject = _emailCon.getSubject(emailProp);
                            rpWord.body = _emailCon.getBody(emailProp);
                            rpWord.signature = _emailCon.getSignature(emailProp);

                            if (zipFileNames.Count > 1)
                            {
                                rpWord.subject = strProcessSubject(rpWord);
                                rpWord.subject = rpWord.subject + "(" + count + "/" + zipFileNames.Count + ")";
                            }
                            else
                            {
                                rpWord.subject = strProcessSubject(rpWord);
                            }
                            rpWord.body = strProcessBody(rpWord);
                            rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                            dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);
                            count++;
                        }

                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;


                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;
                        emailProp.isValid = true;

                        return emailProp;

                    case "8":

                        //emailProp.dgvShip.Sort(emailProp.dgvShip.Columns["manuFactureNo"], ListSortDirection.Ascending);

                        //emailProp.dgvShip.Sort(emailProp.dgvShip.Columns["manuFactureNo"], ListSortDirection.Ascending);
                        if (shipProp.customerName.ToUpper() != "HISILICON" && shipProp.customerName.ToUpper() != "HUAWEI")
                        {
                            MessageBox.Show("case นี้ สำหรับ Hisilicon/Huawei เท่านั้น", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        shippingPathHSLD = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-LD_Data\\";
                        shippingPathSerialList = emailProp.pathShipping + emailProp.shipDate.ToString("yyMMdd") + "-" + rpWord.customerName + "(" + rpWord.productNameInEmail + ")-SerialList\\";



                        if (!Directory.Exists(shippingPathHSLD))
                        {
                            Directory.CreateDirectory(shippingPathHSLD);
                        }
                        else
                        {
                            Directory.Delete(shippingPathHSLD, true);
                            Directory.CreateDirectory(shippingPathHSLD);
                        }

                        if (!Directory.Exists(shippingPathSerialList))
                        {
                            Directory.CreateDirectory(shippingPathSerialList);
                        }
                        else
                        {
                            Directory.Delete(shippingPathSerialList, true);
                            Directory.CreateDirectory(shippingPathSerialList);
                        }

                        folderEdata = Directory.GetDirectories(shippingPathHSLD);
                        folderSerialList = Directory.GetDirectories(shippingPathSerialList);
                        folderCustomer = Directory.GetDirectories(emailProp.folderCustomer);

                        //   if (folderCustomer.Length != emailProp.dgvShip.Rows.Count)
                        //   {
                        //       MessageBox.Show("จำนวน order กับ จำนวน folder ของดาต้าไม่เท่ากัน " + emailProp.folderCustomer);
                        //       emailProp.isValid = false;
                        //       return emailProp;
                        //   }

                        srcPath = new string[emailProp.dgvShip.SelectedRows.Count];
                        spFile = new string[emailProp.dgvShip.SelectedRows.Count];
                        slFile = new string[emailProp.dgvShip.SelectedRows.Count];

                        count = 0;
                        foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.SelectedRows)
                        {

                            rpWord.shipDate = emailProp.shipDate;

                            emailProp.productName = shipProp.productName;

                            emailProp.key = dtShipRow.Cells["KEY_ID"].Value.ToString();
                            rpWord.customerName = dtShipRow.Cells["customerName"].Value.ToString();
                            //fecorderNo or ManuFactureNo

                            shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();
                            rpWord.manuFactureNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                            shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                            rpWord.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();

                            shipProp.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();
                            rpWord.customerPN = dtShipRow.Cells["customerPN"].Value.ToString();

                            shipProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            emailProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();

                            shipProp.qty = dtShipRow.Cells["Qty"].Value.ToString();

                            shipProp.subFolder = dtShipRow.Cells["subFolder"].Value.ToString();

                            dateToMatch = emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + emailProp.partNo.Substring(7) + ")-" + shipProp.subFolder;

                            string FullName = Path.Combine(emailProp.folderCustomer, dateToMatch);

                            //string edata = Path.Combine(folderCustomer[count], "Edata");
                            string edata = Path.Combine(FullName, "Edata");

                            var fi = Directory.EnumerateFiles(edata, emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo + ")-" + shipProp.subFolder + "*");

                            if (fi.Any())
                            {
                                spFile[count] = fi.FirstOrDefault();

                                emailProp.isValid = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบ order or file Edata นี้ " + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "(" + shipProp.partNo + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (fileCls.fnCopyFile(spFile[count], shippingPathHSLD) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            var sl = Directory.EnumerateFiles(edata, emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "-" + shipProp.qty + "*");

                            if (sl.Any())
                            {
                                slFile[count] = sl.FirstOrDefault();
                                emailProp.isValid = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบ order or serialList นี้ " + emailProp.shipDate.ToString("yyMMdd") + "-" + shipProp.fecOrderNo + "-" + shipProp.qty, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                emailProp.isValid = false;
                                return emailProp;
                            }

                            if (fileCls.fnCopyFile(slFile[count], shippingPathSerialList) == false)
                            {
                                emailProp.isValid = false;
                                return emailProp;
                            }
                            count++;
                        }


                        //// Data 
                        for (count = 0; count < spFile.Length; count++)
                        {
                            // ดึงไฟล์จากลิสต์โดยใช้ดัชนี
                            dtFile.Rows.Add(shippingPathHSLD, spFile[count]);
                        }
                        emailProp = _emailCon.getEmailAddress(emailProp);

                        rpWord.shipDate = emailProp.shipDate;
                        rpWord.formatDate = _emailCon.allFormatDate();
                        rpWord.subject = _emailCon.getSubject(emailProp);
                        rpWord.body = _emailCon.getBody(emailProp);
                        rpWord.signature = _emailCon.getSignature(emailProp);

                        rpWord.subject = strProcessSubject(rpWord);
                        rpWord.body = strProcessBody(rpWord);
                        rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                        for (count = 0; count < slFile.Length; count++)
                        {
                            // ดึงไฟล์จากลิสต์โดยใช้ดัชนี
                            dtslFile.Rows.Add(shippingPathSerialList, slFile[count]);
                        }

                        rpWord.subject = "Serial list for Hisilicon" + "(" + shipProp.productNameInEmail + ") " + emailProp.shipDate.ToString("yyyy-MM-dd");
                        rpWord.body = "We attached file(s) of Serial list for Hisilicon in this e-mail";
                        rpWord.body = rpWord.body + "<BR>" + "<BR>" + rpWord.signature;

                        dtInfoEmail.Rows.Add(emailProp.toEmailAddresses, emailProp.ccEmailAddresses, emailProp.productName, emailProp.key, rpWord.subject, rpWord.body, emailProp.fileFormatsId);

                        dgvFile.DataSource = dtFile;
                        emailProp.dgvFile = dgvFile;

                        dgslFile.DataSource = dtslFile;
                        emailProp.dgslFile = dgslFile;

                        dgvInfoEmail.DataSource = dtInfoEmail;
                        emailProp.dgvInfoEmail = dgvInfoEmail;
                        emailProp.isValid = true;

                        return emailProp;

                    case "13": // 1480Dual txt, selected orders only
                        return prepareCase13TxtEdata(emailProp, emailProp.dgvShip.SelectedRows.Cast<DataGridViewRow>()
                                                                     .OrderBy(r => r.Index),
                                                     rpWord, dtFile, dtInfoEmail, dgvFile, dgvInfoEmail);

                    case "14": // 1480Dual MOLEX, selected orders only (no folder-count check: the other orders' folders are there too)
                        return prepareCase14TxtZip(emailProp, emailProp.dgvShip.SelectedRows.Cast<DataGridViewRow>()
                                                                  .OrderBy(r => r.Index), false,
                                                   rpWord, dtFile, dtInfoEmail, dgvFile, dgvInfoEmail);

                    //Nothing
                    default:
                        emailProp.isValid = false;
                        MessageBox.Show("เคสนี้อาจจะยังไม่ถูก setting");
                        return emailProp;

                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2146233087) // ตรวจสอบ HResult เฉพาะเจาะจง
                {
                    MessageBox.Show("พบข้อผิดพลาดของระบบ: ไม่สามารถหา End of Central Directory (EOCD) ของ zip ได้ (ไฟล์ปลายทางอาจจะเป็น .Rar หรือ อื่นๆ", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("เกิดข้อผิดพลาดของระบบ: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                emailProp.isValid = false;
                return emailProp;
            }
        }


        public DataGridView prepareDetailEmail(EmailProperty.replaceWord rpWord)
        {
            DataGridView grid = new DataGridView();
            DataTable dt = new DataTable();

            switch (rpWord.fileFormatsId)
            {

                //CSV
                case "1":
                    return grid;
                //XLS XLSX
                case "2":
                    //

                    dt.Columns.Add("subject", typeof(string));
                    dt.Columns.Add("body", typeof(string));

                    dt.Rows.Add(rpWord.subject, rpWord.body);
                    grid.DataSource = dt;


                    return grid;
                //ZIP  //RAR
                case "3":
                    return grid;
                //TXT
                case "4":
                    dt.Columns.Add("subject", typeof(string));
                    dt.Columns.Add("body", typeof(string));

                    dt.Rows.Add(rpWord.subject, rpWord.body);
                    grid.DataSource = dt;
                    return grid;
                //Nothing
                default:
                    return grid;
            }
        }

        public bool CheckOutlookAccountExists(string targetEmail)
        {
            Outlook.Application app = null;
            try
            {
                // พยายามเปิด Outlook
                app = new Outlook.Application();

                if (app.Session == null || app.Session.Accounts == null)
                    return false;

                Outlook.Accounts accounts = app.Session.Accounts;

                // วนลูปหา
                for (int i = 1; i <= accounts.Count; i++)
                {
                    Outlook.Account acc = accounts.Item(i);
                    if (acc.SmtpAddress.Equals(targetEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        // เจอแล้ว! คืนค่า True
                        return true;
                    }
                }

                // วนจนจบแล้วไม่เจอ
                return false;
            }
            catch
            {
                // กรณี Error (เช่น เครื่องไม่มี Outlook เลย)
                return false;
            }
            finally
            {
                // คืน Resource ทันทีเพราะใช้แค่นิดเดียว
                if (app != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
            }
        }

        

    }
}
