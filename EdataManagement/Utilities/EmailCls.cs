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

            try
            {
                // 1. สร้าง Object Outlook Application
                app = new Outlook.Application();
                mail = (Outlook.MailItem)app.CreateItem(Outlook.OlItemType.olMailItem);


                // 2. จัดการผู้รับ (To)
                if (prop.toEmailAddresses != null)
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
                if (prop.ccEmailAddresses != null)
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
                mail.Subject = itemData.subject;

                // 6. กำหนดเนื้อหา (ต้องใช้ HTMLBody)
                mail.BodyFormat = Outlook.OlBodyFormat.olFormatHTML;
                // เติม <html> tag เพื่อความสมบูรณ์
                mail.HTMLBody = "<html><body>Dear Sir/Madam <br><br>" + itemData.body + "</body></html>";

                

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

                        emailProp.toEmailAddresses = fileCls.GetEmailAddresses(shipProp.productName + shipProp.customerName + "SERIAL_LIST", "to");
                        if (emailProp.toEmailAddresses.Count == 0 || emailProp.toEmailAddresses == null)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.ccEmailAddresses = fileCls.GetEmailAddresses(shipProp.productName + shipProp.customerName + "SERIAL_LIST", "cc");
                        if (emailProp.ccEmailAddresses.Count == 0 || emailProp.ccEmailAddresses == null)
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

                        emailProp.toEmailAddresses = fileCls.GetEmailAddresses(shipProp.productName + shipProp.customerName + "WAFER", "to");
                        if (emailProp.toEmailAddresses.Count == 0 || emailProp.toEmailAddresses == null)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.ccEmailAddresses = fileCls.GetEmailAddresses(shipProp.productName + shipProp.customerName + "WAFER", "cc");
                        if (emailProp.ccEmailAddresses.Count == 0 || emailProp.ccEmailAddresses == null)
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

                        if (fileCls.fnCopyEData(shippingPathLD, emailProp.shipDate) == false)
                        {
                            MessageBox.Show("ทำการ copy ก่อนเปลี่ยนชื่อไม่สำเร็จ " + shippingPathLD, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        folderEdata = Directory.GetDirectories(shippingPathLD);

                        foreach (string ffe in folderEdata)
                        {
                            if(fileCls.CompressFileZip( ffe,  ffe) == false)
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

                        emailProp.toEmailAddresses = fileCls.GetEmailAddresses(shipProp.productName + shipProp.customerName + "SERIAL_LIST", "to");
                        if (emailProp.toEmailAddresses.Count == 0 || emailProp.toEmailAddresses == null)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.ccEmailAddresses = fileCls.GetEmailAddresses(shipProp.productName + shipProp.customerName + "SERIAL_LIST", "cc");
                        if (emailProp.ccEmailAddresses.Count == 0 || emailProp.ccEmailAddresses == null)
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

                    case "10":

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

                            List<string> folderPatternsToSearch = new List<string>();

                            if (!string.IsNullOrEmpty(shipProp.subFolder))
                            {
                                folderPatternsToSearch.Add($"{datePrefix}-{shipProp.fecOrderNo}({shortPartNo})-{shipProp.subFolder}");
                            }

                            folderPatternsToSearch.Add($"{datePrefix}-{shipProp.fecOrderNo}({shortPartNo})");

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

                            foreach (string searchPattern in folderPatternsToSearch)
                            {
                                List<string> matchedOrderFolders = new List<string>();

                                // 1. เช็คกรณี emailProp.folderCustomer เองคือ folder ที่ต้องการแล้ว
                                if (Path.GetFileName(emailProp.folderCustomer.TrimEnd('\\'))
                                    .Equals(searchPattern, StringComparison.OrdinalIgnoreCase))
                                {
                                    matchedOrderFolders.Add(emailProp.folderCustomer.TrimEnd('\\'));
                                }

                                // 2. scan ลงไปทุก folder ย่อย เหมือน logic เวอร์ชันเก่า
                                matchedOrderFolders.AddRange(
                                    Directory.GetDirectories(emailProp.folderCustomer, "*", SearchOption.AllDirectories)
                                        .Where(dir => Path.GetFileName(dir)
                                            .Equals(searchPattern, StringComparison.OrdinalIgnoreCase))
                                );

                                // กัน duplicate
                                matchedOrderFolders = matchedOrderFolders
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .ToList();

                                foreach (string orderFolder in matchedOrderFolders)
                                {
                                    foundOrderFolder = orderFolder;

                                    // โครงสร้างจริง:
                                    // ...\OrderFolder\OrderFolder\Excel.xlsx
                                    string doubleNestedPath = Path.Combine(orderFolder, searchPattern);

                                    // fallback:
                                    // ...\OrderFolder\Excel.xlsx
                                    string singlePath = orderFolder;

                                    string[] pathsToCheck = new string[]
                                    {
                                        doubleNestedPath,
                                        singlePath
                                    };

                                    foreach (string targetDir in pathsToCheck)
                                    {
                                        lastCheckedFolder = targetDir;

                                        if (!Directory.Exists(targetDir))
                                            continue;

                                        string[] filesInFolder = Directory.GetFiles(targetDir)
                                            .Where(f =>
                                            {
                                                string fileNameNoExt = Path.GetFileNameWithoutExtension(f);

                                                bool isExcel =
                                                    f.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) ||
                                                    f.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);

                                                bool matchDate =
                                                    fileNameNoExt.StartsWith($"{datePrefix}-", StringComparison.OrdinalIgnoreCase);

                                                bool matchPart =
                                                    fileNameNoExt.EndsWith($"({shortPartNo})", StringComparison.OrdinalIgnoreCase);

                                                return isExcel && matchDate && matchPart;
                                            })
                                            .ToArray();

                                        // fallback แบบเวอร์ชันเก่า:
                                        // ถ้าไม่เจอจาก date + part ให้หยิบ Excel ใน folder นั้น
                                        if (filesInFolder.Length == 0)
                                        {
                                            filesInFolder = Directory.GetFiles(targetDir)
                                                .Where(f =>
                                                    f.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) ||
                                                    f.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                                                .OrderByDescending(f =>
                                                    Path.GetFileNameWithoutExtension(f)
                                                        .IndexOf("PO-", StringComparison.OrdinalIgnoreCase) >= 0)
                                                .ToArray();
                                        }

                                        if (filesInFolder.Length > 0)
                                        {
                                            dtFile.Rows.Add(targetDir, filesInFolder[0]);
                                            fileFound = true;
                                            break;
                                        }
                                    }

                                    if (fileFound) break;
                                }

                                if (fileFound) break;
                            }

                            //if (!fileFound)
                            //{
                            //    string errorMessage = $"ไม่พบไฟล์ข้อมูลที่ถูกต้องสำหรับ Order: {shipProp.fecOrderNo}\n" +
                            //                          $"Part No: {emailProp.partNo}\n" +
                            //                          $"Short Part No: {shortPartNo}\n" +
                            //                          $"Sub Folder: {shipProp.subFolder}\n" +
                            //                          $"วันที่: {datePrefix}\n\n" +
                            //                          $"พยายามค้นหา Folder Pattern:\n" +
                            //                          string.Join("\n", folderPatternsToSearch) + "\n\n" +
                            //                          $"Root ที่ใช้ค้นหา:\n" +
                            //                          $"{emailProp.folderCustomer}\n\n" +
                            //                          $"Order Folder ที่เจอ:\n" +
                            //                          $"{foundOrderFolder}\n\n" +
                            //                          $"Folder ล่าสุดที่ตรวจสอบ:\n" +
                            //                          $"{lastCheckedFolder}\n\n" +
                            //                          $"ไฟล์ที่คาดหวัง:\n" +
                            //                          $"{datePrefix}-*({shortPartNo}).xls(x)";

                            //    MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //    emailProp.isValid = false;
                            //    return emailProp;
                            //}

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

                        emailProp.toEmailAddresses = fileCls.GetEmailAddresses(shipProp.productName + shipProp.customerName + "SERIAL_LIST", "to");
                        if (emailProp.toEmailAddresses.Count == 0 || emailProp.toEmailAddresses == null)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.ccEmailAddresses = fileCls.GetEmailAddresses(shipProp.productName + shipProp.customerName + "SERIAL_LIST", "cc");
                        if (emailProp.ccEmailAddresses.Count == 0 || emailProp.ccEmailAddresses == null)
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

                        emailProp.toEmailAddresses = fileCls.GetEmailAddresses(shipProp.productName + shipProp.customerName + "WAFER", "to");
                        if (emailProp.toEmailAddresses.Count == 0 || emailProp.toEmailAddresses == null)
                        {
                            emailProp.isValid = false;
                            return emailProp;
                        }

                        emailProp.ccEmailAddresses = fileCls.GetEmailAddresses(shipProp.productName + shipProp.customerName + "WAFER", "cc");
                        if (emailProp.ccEmailAddresses.Count == 0 || emailProp.ccEmailAddresses == null)
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
