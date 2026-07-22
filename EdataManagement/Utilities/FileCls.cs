using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Data;
using IniParser;
using IniParser.Model;
using System.Configuration;
using EdataManagement.Property;
using EdataManagement.Controllers;

namespace EdataManagement.Utilities
{
    public class FileCls
    {

        //ShippingProperty shipProp = new ShippingProperty();
        //EmailProperty emailProp = new EmailProperty();

        public bool CompressFileZip(string sourceFolderPath, string zipFilePath)
        {
            try
            {

                // แสดงผลลัพธ์ใน zipFilePath แบบ ไม่มี \\
                if (zipFilePath.EndsWith("\\"))
                {
                    // ถ้าลงท้ายด้วย backslash ไม่ต้องทำอะไร
                    zipFilePath = zipFilePath.TrimEnd('\\');
                }

                // แสดงผลลัพธ์ใน zipFilePath แบบ ไม่มี \\
                if (sourceFolderPath.EndsWith("\\"))
                {
                    // ถ้าลงท้ายด้วย backslash ไม่ต้องทำอะไร
                    sourceFolderPath = sourceFolderPath.TrimEnd('\\');
                }

                // ถ้าไฟล์ .zip มีอยู่แล้ว
                if (File.Exists(zipFilePath + ".zip"))
                {
                    // ลบไฟล์ .zip เดิมทิ้ง
                    File.Delete(zipFilePath + ".zip");
                }

                ZipFile.CreateFromDirectory(sourceFolderPath, zipFilePath + ".zip");

                List<int> folderIndexesToDelete = new List<int>();

                string directoryNameToDelete = "For QA check";

                using (ZipArchive zipArchive = ZipFile.Open(zipFilePath + ".zip", ZipArchiveMode.Update))
                {
                    for (int i = 0; i < zipArchive.Entries.Count; i++)
                    {
                        ZipArchiveEntry entry = zipArchive.Entries[i];

                        // ตรวจสอบว่า Entry เป็นโฟลเดอร์และมีชื่อเป็น "For QA check"
                        if (entry.FullName.IndexOf(directoryNameToDelete, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            // เก็บ index ของ Entry ที่ต้องการลบ
                            folderIndexesToDelete.Add(i);
                        }
                    }

                    foreach (int index in folderIndexesToDelete.OrderByDescending(x => x))
                    {
                        // ลบ Entry ที่ต้องการออกจาก Zip Archive
                        zipArchive.Entries[index].Delete();
                    }

                }

                //MessageBox.Show("ไฟล์ Zip ถูกสร้างขึ้นเรียบร้อยแล้ว");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = "เกิดข้อผิดพลาดในการบีบอัดไฟล์: " + ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " innerexception : " + ex.InnerException.Message;
                }
                MessageBox.Show(errorMessage);
                return false;
            }
        }

        //public bool CompressFileManage(ShippingProperty shipProp, EmailProperty emailProp)
        //{

        //    string sourceFolder = emailProp.folderCustomer; // \\\\office_SRV\\C06_Shipping-Data\\Test\\1480LDM\\250327-Shipping Data\\250327-XTERA(48)\\
        //    string zipFilePath = emailProp.folderCustomer + ".zip"; // ปรับเป็น path ปลายทาง

        //    try
        //    {
        //        using (ZipArchive zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
        //        {

        //            foreach (string subFolderPath in Directory.GetDirectories(sourceFolder))
        //            {
        //                string folderName = Path.GetFileName(subFolderPath);


        //                // ดึง specNo จากในวงเล็บ
        //                string specNo = shipProp.partNo.Substring(7);

        //                // ดึง shipdate
        //                string shipdate = emailProp.shipDate.ToString("yyMMdd");

        //                // ดึง subFolderNum
        //                string subFolderNum = shipProp.subFolder;

        //                // ดึง orderNo
        //                string orderNo = shipProp.fecOrderNo;

        //                // สร้างชื่อไฟล์ .xls
        //                string xlsFileName = $"{shipdate}-{orderNo}({specNo}).xls";
        //                string xlsFilePath = Path.Combine(subFolderPath, xlsFileName);

        //                // สร้าง path สำหรับ serial_no.txt
        //                string innerFolder = Path.Combine(subFolderPath, $"{shipdate}-{orderNo}({specNo})");
        //                List<string> serialNoFileNames = new List<string>();

        //                // ดึงรายชื่อไฟล์จาก DataTable ที่มาจาก Database เพื่อใส่ลงใน serialNoFileNames เพื่อเติม .txt หาดาต้าใน folder innerFolder
        //                foreach (DataRow row in shipProp.dtSerial.Rows)
        //                {
        //                    serialNoFileNames.Add(row["serial"].ToString() + ".txt");
        //                }

        //                // ดึงรายชื่อไฟล์จาก innerFolder
        //                string[] innerFiles = Directory.GetFiles(innerFolder, "*.txt");
        //                List<string> innerFileNames = innerFiles.Select(Path.GetFileName).ToList();

        //                // สร้าง List<string> เพื่อเก็บไฟล์ serial_no*.txt
        //                List<string> serialNoFiles = new List<string>();
        //                serialNoFiles.AddRange(Directory.GetFiles(innerFolder, "*.txt"));

        //                // ตรวจสอบจำนวนไฟล์
        //                if (innerFiles.Length != serialNoFileNames.Count)
        //                {
        //                    if (innerFiles.Length < serialNoFileNames.Count)
        //                    {
        //                        Console.WriteLine($"มีไฟล์ขาดหาย {serialNoFileNames.Count - innerFiles.Length} ไฟล์");
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine($"มีไฟล์เกินมา {innerFiles.Length - serialNoFileNames.Count} ไฟล์");
        //                    }
        //                }
        //                else
        //                {
        //                    // ตรวจสอบชื่อไฟล์
        //                    var missingFiles = serialNoFileNames.Except(innerFileNames).ToList();
        //                    var extraFiles = innerFileNames.Except(serialNoFileNames).ToList();

        //                    if (missingFiles.Any() || extraFiles.Any())
        //                    {
        //                        if (missingFiles.Any())
        //                        {
        //                            Console.WriteLine("มีไฟล์ขาดหาย: " + string.Join(", ", missingFiles));
        //                        }
        //                        if (extraFiles.Any())
        //                        {
        //                            Console.WriteLine("มีไฟล์เกินมา: " + string.Join(", ", extraFiles));
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("จำนวนและชื่อไฟล์ตรงกัน สามารถสร้าง zip ได้");
        //                        // สร้าง zip ที่นี่
        //                        //if (File.Exists(xlsFilePath))
        //                        //{
        //                        //    string zipFolderName = folderName;
        //                        //    zip.CreateEntryFromFile(xlsFilePath, Path.Combine(zipFolderName, xlsFileName));
        //                        //    foreach (string serialNoFile in serialNoFiles)
        //                        //    {
        //                        //        string serialNoFileName = Path.GetFileName(serialNoFile);
        //                        //        zip.CreateEntryFromFile(serialNoFile, Path.Combine(zipFolderName, serialNoFileName));
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    Console.WriteLine($"ไม่พบไฟล์ในโฟลเดอร์: {folderName}");
        //                        //    return false;
        //                        //}
        //                    }
        //                }

        //            }
        //        }
        //        Console.WriteLine("สร้างไฟล์ ZIP เสร็จแล้ว!");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"เกิดข้อผิดพลาด: {ex.Message}");
        //        return false;
        //    }
        //}


        public bool CompressFileManage(ShippingProperty shipProp, EmailProperty emailProp)
        {
            ShippingControllers _shipCon = new ShippingControllers();

            string sourceFolder = emailProp.folderCustomer; // path จริง
            if (sourceFolder.EndsWith("\\"))
            {
                sourceFolder = sourceFolder.TrimEnd('\\');
            }

            string zipFilePath = sourceFolder + ".zip"; // path ปลายทาง

            // ลบไฟล์ ZIP เดิมถ้ามี
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            try
            {
                using (ZipArchive zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (DataGridViewRow dtShipRow in emailProp.dgvShip.SelectedRows)
                    {
                        // ดึงข้อมูลจาก DataGridView
                        shipProp.partNo = dtShipRow.Cells["Part_No"].Value.ToString();
                        string shipdate = emailProp.shipDate.ToString("yyMMdd");
                        shipProp.subFolder = dtShipRow.Cells["subFolder"].Value.ToString();
                        shipProp.testReportNo = dtShipRow.Cells["TestReportNo"].Value.ToString();
                        shipProp.fecOrderNo = dtShipRow.Cells["manuFactureNo"].Value.ToString();

                        // ค้นหา serial จากฐานข้อมูล
                        shipProp.dtSerial = _shipCon.SearchSerialShipping(shipProp);

                        if (shipProp.dtSerial.Rows.Count == 0)
                        {
                            emailProp.strLog += $"เกิดข้อผิดพลาด ไม่พบข้อมูล serial ของ order {shipProp.fecOrderNo}\n";
                            MessageBox.Show(emailProp.strLog, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }

                        // สร้าง path ของโฟลเดอร์ย่อย
                        string subFolderPath = Path.Combine(emailProp.folderCustomer,
                            $"{shipdate}-{shipProp.fecOrderNo}({shipProp.partNo.Substring(7)})-{shipProp.subFolder}");
                        string folderName = Path.GetFileName(subFolderPath);

                        // สร้างชื่อไฟล์ .xls
                        string xlsFileName = $"{shipdate}-{shipProp.fecOrderNo}({shipProp.partNo.Substring(7)}).xls";
                        string xlsFilePath = Path.Combine(subFolderPath, xlsFileName);

                        // ตรวจสอบว่าไฟล์ .xls มีอยู่
                        if (!File.Exists(xlsFilePath))
                        {
                            emailProp.strLog += $"ไม่พบไฟล์ {xlsFileName} ในโฟลเดอร์: {folderName}\n";
                            MessageBox.Show(emailProp.strLog, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }

                        // สร้าง path สำหรับ innerFolder
                        string innerFolder = Path.Combine(subFolderPath,
                            $"{shipdate}-{shipProp.fecOrderNo}({shipProp.partNo.Substring(7)})-{shipProp.subFolder}");

                        // ดึงรายชื่อไฟล์จาก DataTable
                        List<string> serialNoFileNames = new List<string>();
                        foreach (DataRow row in shipProp.dtSerial.Rows)
                        {
                            serialNoFileNames.Add(row["serial"].ToString() + ".txt");
                        }

                        // ดึงรายชื่อไฟล์จาก innerFolder (ทุกประเภท)
                        string[] innerFiles = Directory.GetFiles(innerFolder, "*.*");
                        List<string> innerFileNames = innerFiles.Select(Path.GetFileName).ToList();

                        //// สร้าง List<string> เพื่อเก็บไฟล์ serial_no*.txt
                        List<string> serialNoFiles = new List<string>();
                        serialNoFiles.AddRange(Directory.GetFiles(innerFolder, "*.txt"));

                        // ตรวจสอบจำนวนไฟล์
                        if (innerFileNames.Count != serialNoFileNames.Count)
                        {
                            if (innerFileNames.Count < serialNoFileNames.Count)
                            {
                                emailProp.strLog += $"มีไฟล์ขาดหาย {serialNoFileNames.Count - innerFileNames.Count} ไฟล์\n";
                            }
                            else
                            {
                                emailProp.strLog += $"มีไฟล์เกินมา {innerFileNames.Count - serialNoFileNames.Count} ไฟล์\n";
                            }
                            MessageBox.Show(emailProp.strLog + "Test Report " + shipProp.testReportNo,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }

                        // ตรวจสอบชื่อไฟล์
                        var missingFiles = serialNoFileNames.Except(innerFileNames).ToList();
                        var extraFiles = innerFileNames.Except(serialNoFileNames).ToList();
                        if (missingFiles.Any() || extraFiles.Any())
                        {
                            if (missingFiles.Any())
                            {
                                emailProp.strLog += "มีไฟล์ขาดหาย: " + string.Join(", ", missingFiles) + "\n";
                            }
                            if (extraFiles.Any())
                            {
                                emailProp.strLog += "มีไฟล์เกินมา: " + string.Join(", ", extraFiles) + "\n";
                            }
                            MessageBox.Show(emailProp.strLog + "Test Report " + shipProp.testReportNo,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }

                        // เพิ่มไฟล์ .xls เข้า ZIP
                        zip.CreateEntryFromFile(xlsFilePath, Path.Combine(folderName, xlsFileName));

                        // เพิ่มไฟล์ .txt จาก innerFolder ลงใน ZIP โดยคงชื่อไฟล์เดิม
                        foreach (string serialFileName in serialNoFileNames)
                        {
                            string serialFilePath = Path.Combine(innerFolder, serialFileName);
                            if (File.Exists(serialFilePath))
                            {
                                zip.CreateEntryFromFile(serialFilePath, Path.Combine(folderName, serialFileName));
                            }
                        }
                    }
                }
                Console.WriteLine("สร้างไฟล์ ZIP เสร็จแล้ว!");
                return true;
            }
            catch (Exception ex)
            {
                emailProp.strLog += $"เกิดข้อผิดพลาด: {ex.Message}\n";
                MessageBox.Show(emailProp.strLog, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

    public string FindFolder(string rootFolderPath, string targetFolderName)
        {
            // ตรวจสอบว่าโฟลเดอร์ "TEST DATA" อยู่ในโฟลเดอร์ที่กำหนดหรือไม่
            if (Path.GetFileName(rootFolderPath).Equals(targetFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return rootFolderPath;
            }

            // หาโฟลเดอร์ "TEST DATA" ในโฟลเดอร์ที่ใหญ่ขึ้น
            foreach (string subFolderPath in Directory.GetDirectories(rootFolderPath))
            {
                string testDataFolder = FindFolder(subFolderPath, targetFolderName);
                if (!string.IsNullOrEmpty(testDataFolder))
                {
                    return testDataFolder;
                }
            }

            // หาไม่พบโฟลเดอร์ "TEST DATA"
            return null;
        }

        public int CountZipFilesInDirectory(string directoryPath, string baseFileName, List<string> zipFileNames)
        {
            try
            {
                // ค้นหาไฟล์ที่มีชื่อเริ่มต้นด้วย baseFileName ในโฟลเดอร์และโฟลเดอร์ย่อย
                string[] files = Directory.GetFiles(directoryPath, $"{baseFileName}*", System.IO.SearchOption.AllDirectories);

                // นับจำนวนไฟล์ที่ตรงกับรูปแบบของชื่อไฟล์ และเก็บชื่อไฟล์ไว้
                int count = 0;
                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileName(filePath);
                    if (fileName.StartsWith(baseFileName) && (fileName.EndsWith(".zip") || fileName.EndsWith(".rar")))
                    {
                        count++;
                        zipFileNames.Add(fileName);
                    }
                }

                //zipFileNames จุดนี้จะได้ไฟล์มา

                Console.WriteLine("จำนวนไฟล์ zip ที่พบ: " + count);
                Console.WriteLine("รายการไฟล์ zip:");
                foreach (string fileName in zipFileNames)
                {


                }

                return count;
            }
            catch (Exception ex)
            {
                // จัดการ Exception ที่เกิดขึ้น (เช่น แสดงข้อความผิดพลาด, บันทึกล็อก, รีไฟล์ Exception)
                MessageBox.Show($"Error: {ex.Message}");
                //Console.WriteLine($"Error: {ex.Message}");
                return 0; // หรือให้รีเทิร์นค่าที่คุณต้องการในกรณี Exception
            }
        }

        // นับจำนวนโฟลเดอร์ระดับบนสุดในไฟล์ zip (1 order = 1 โฟลเดอร์)
        // ใช้ตรวจสอบว่าข้อมูลที่จะส่งครบตามจำนวน order หรือไม่ (อ่านแค่ header ของ zip ไม่ได้แตกไฟล์)
        public int CountTopLevelFoldersInZip(string zipFilePath)
        {
            if (!File.Exists(zipFilePath))
                return 0;

            HashSet<string> topLevelFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (ZipArchive zipArchive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    string[] pathParts = entry.FullName.Split('/');
                    if (pathParts.Length > 1 && !string.IsNullOrEmpty(pathParts[0]))
                    {
                        topLevelFolders.Add(pathParts[0]);
                    }
                }
            }

            return topLevelFolders.Count;
        }

        public string GetZipDirectoryContents(string zipFilePath)
        {
            // สร้าง StringBuilder เพื่อเก็บข้อมูล directory
            StringBuilder directoryList = new StringBuilder();

            // ตรวจสอบว่าไฟล์ Zip มีอยู่หรือไม่
            if (!File.Exists(zipFilePath))
            {
                throw new FileNotFoundException("File not found", zipFilePath);
            }

            // ใช้ HashSet เพื่อเก็บ directory ที่ไม่ซ้ำกัน
            HashSet<string> uniqueDirectories = new HashSet<string>();

            using (ZipArchive zipArchive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    // Split the entry path by "/" to get the top-level directory
                    string[] pathParts = entry.FullName.Split('/');

                    // ตรวจสอบว่า path มีหลายส่วนและไม่ใช่ค่าว่าง
                    if (pathParts.Length > 1 && !string.IsNullOrEmpty(pathParts[0]))
                    {
                        // เพิ่ม directory ระดับบนสุดใน HashSet เพื่อหลีกเลี่ยงซ้ำ
                        uniqueDirectories.Add(pathParts[0]);
                    }
                }
            }

            // สร้างผลลัพธ์จาก HashSet
            foreach (string directory in uniqueDirectories)
            {
                // หาตำแหน่งของ "-" ตัวแรก
                int dashIndex = directory.IndexOf("-");

                // ตรวจสอบว่ามี "-" ในข้อความหรือไม่ แล้วเอาข้อความหลังจาก "-" ตัวแรก
                string formattedDirectory = (dashIndex != -1) ? directory.Substring(dashIndex + 1) : directory;

                // เพิ่มข้อความที่ตัดแล้วลงใน StringBuilder
                directoryList.Append(formattedDirectory);
                directoryList.Append("<BR>");
            }

            return directoryList.ToString();
        }

        public string GetZipDirectoryContentsSupportVB(string zipFilePath)
        {
            // สร้าง StringBuilder เพื่อเก็บข้อมูล directory
            StringBuilder directoryList = new StringBuilder();

            // ตรวจสอบว่าไฟล์ Zip มีอยู่หรือไม่
            if (!File.Exists(zipFilePath))
            {
                throw new FileNotFoundException("File not found", zipFilePath);
            }

            using (ZipArchive zipArchive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    // Check if entry is a directory
                    if (entry.FullName.EndsWith("/"))
                    {
                        // Check if directory is at position 2 and has compressed length of 0 zipArchive.Entries.ToList().IndexOf(entry) == 2 && 
                        if (entry.CompressedLength == 0)
                        {
                            // Exclude directory with specific name
                            if (!entry.FullName.Contains("For QA check"))
                            {
                                string[] pathParts = entry.FullName.Split('/');

                                // นำ directory ที่พบมาเพิ่มเข้าไปใน StringBuilder
                                directoryList.Append(pathParts[1]);
                                directoryList.Append("<BR>");
                            }
                        }
                    }
                }
            }

            return directoryList.ToString();
        }

        void SafelyCreateZipFromDirectory(string sourceDirectoryName, string zipFilePath)
        {



            using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                foreach (var file in Directory.GetFiles(sourceDirectoryName))
                {
                    var entryName = Path.GetFileName(file);
                    var entry = archive.CreateEntry(entryName);
                    entry.LastWriteTime = File.GetLastWriteTime(file);
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var stream = entry.Open())
                    {
                        fs.CopyTo(stream);
                    }
                }
            }
        }

        public bool fnCopyEDataAccelink(string shippingPath, DateTime shipdate)
        {
            bool bfnStatus = false;
            string strShipDataFolderPath;
            int intTempFileCount;
            double douTempFileSize;
            int intDataFileCount;
            double douDataFileSize;
            string strEdataFolderName;
            string strSourceFolderPath;
            string strDestFolderPath;
            string strDestEdataFolderPath;

            bfnStatus = false;
            int iTotalTemp = 1;

            foreach (string sFolder in Directory.GetDirectories(shippingPath))
            {
                strShipDataFolderPath = sFolder;

                strEdataFolderName = Path.GetFileName(strShipDataFolderPath);
                strEdataFolderName = strEdataFolderName.Substring(strEdataFolderName.LastIndexOf("(") + 1);
                strEdataFolderName = strEdataFolderName.Replace(")", "");

                intDataFileCount = Directory.GetFiles(strShipDataFolderPath, "*.*", System.IO.SearchOption.AllDirectories).Length;

                DirectoryInfo dInfoDataFolder = new DirectoryInfo(strShipDataFolderPath);
                douDataFileSize = DirectorySize(dInfoDataFolder, true);

            NextDestFolder:
                strDestFolderPath = Path.Combine(shippingPath, "Edata_" + shipdate.ToString("YYYYMMDD") + "-" + iTotalTemp);
                strDestEdataFolderPath = Path.Combine(strDestFolderPath, strEdataFolderName);

                if (!Directory.Exists(strDestFolderPath))
                {
                    Directory.CreateDirectory(strDestFolderPath);
                    intTempFileCount = 0;
                    douTempFileSize = 0.0;
                }
                else
                {
                    if (!Directory.Exists(strDestEdataFolderPath))
                    {
                        intTempFileCount = Directory.GetFiles(strDestFolderPath, "*.*", System.IO.SearchOption.AllDirectories).Length;

                        DirectoryInfo dInfoTempFolder = new DirectoryInfo(strDestFolderPath);
                        douTempFileSize = DirectorySize(dInfoTempFolder, true);
                    }
                    else
                    {
                        iTotalTemp++;
                        goto NextDestFolder;
                    }
                }

                if ((intTempFileCount + intDataFileCount > 255) || (((douTempFileSize + douDataFileSize) / 1000000) > 10))
                {
                    if (douTempFileSize == 0 && ((intDataFileCount > 255) || (((douDataFileSize) / 1000000) > 10)))
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(strShipDataFolderPath, strDestEdataFolderPath, true);
                    }
                    else
                    {
                        iTotalTemp++;
                        goto NextDestFolder;
                    }
                }
                else
                {
                    FileSystem.MoveDirectory(strShipDataFolderPath, strDestEdataFolderPath, true);
                }

                bfnStatus = true;
            }

            return bfnStatus;
        }

        public static long DirectorySize(DirectoryInfo directory, bool includeSubDir)
        {
            long size = 0;
            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo file in files)
            {
                size += file.Length;
            }

            if (includeSubDir)
            {
                DirectoryInfo[] dirs = directory.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    size += DirectorySize(dir, true);
                }
            }

            return size;
        }


        //แก้เรื่องของการหยิบ sourcefile // filesp
        public bool fnPrepareAccelinkDirect(string[] srcPath, string desPath, string[] filesp, int iCountShipRow)
        {
            bool bfnStatus = false;
            //int iTotalTemp = 1;

            for (int iCountOrder = 0; iCountOrder < iCountShipRow; iCountOrder++)
            {
                string strSourceFolderPath = srcPath[iCountOrder].ToString();
                string strSourceFolderJPEGPath = Path.Combine(strSourceFolderPath, "JPEG file");
                string strSourceFolderIfImPath = Path.Combine(strSourceFolderPath, "If-Im data");

                string strSourceFolderName = "";
                //strSourceFolderName = Mid(strSourceFolderPath, InStrRev(strSourceFolderPath, "\\") + 1);
                int index = strSourceFolderPath.LastIndexOf("\\");
                if (index != -1)
                {
                    strSourceFolderName = strSourceFolderPath.Substring(index + 1);
                }

                string strDestEdataFolderPath = "";
                strDestEdataFolderPath = Path.Combine(desPath, strSourceFolderName);
                string strDestEdataFolderJPEGPath = Path.Combine(strDestEdataFolderPath, "JPEG file");
                string strDestEdataFolderIfImPath = Path.Combine(strDestEdataFolderPath, "If-Im data");

                if (!Directory.Exists(strDestEdataFolderPath))
                    Directory.CreateDirectory(strDestEdataFolderPath);

                if (strSourceFolderName.Contains("QQA-617") || strSourceFolderName.Contains("QQA-657"))
                {
                    if (!Directory.Exists(strDestEdataFolderIfImPath))
                        Directory.CreateDirectory(strDestEdataFolderIfImPath);
                }

                string sourceFile = filesp[iCountOrder]; // เส้นทางไฟล์ต้นทาง
                string fileName = Path.GetFileName(sourceFile); // ชื่อไฟล์ต้นทาง
                string destinationFile = Path.Combine(strDestEdataFolderPath, fileName); // สร้างเส้นทางไฟล์ปลายทาง

                // ทำการคัดลอกไฟล์
                File.Copy(sourceFile, destinationFile, true);


                FileSystem.CopyDirectory(strSourceFolderJPEGPath, strDestEdataFolderJPEGPath, true);

                if (strSourceFolderName.Contains("QQA-617") || strSourceFolderName.Contains("QQA-657"))
                {
                    FileSystem.CopyDirectory(strSourceFolderIfImPath, strDestEdataFolderIfImPath, true);
                }

                bfnStatus = true;
            }

            return bfnStatus;
        }

        public bool fnCopyEData(string shippingPathLD, DateTime shipdate)
        {
            bool bfnStatus = false;
            string strShipDataFolderPath;
            int intTempFileCount;
            double douTempFileSize;
            int intDataFileCount;
            double douDataFileSize;
            string strEdataFolderName;
            string strDestFolderPath;
            string strDestEdataFolderPath;

            bfnStatus = false;
            int iTotalTemp = 1;

            //shippingPathLD = shippingPathLD + "-LD_Data";

            foreach (string sFolder in Directory.GetDirectories(shippingPathLD))
            {
                strShipDataFolderPath = sFolder;

                strEdataFolderName = Path.GetFileName(strShipDataFolderPath);
                strEdataFolderName = strEdataFolderName.Substring(strEdataFolderName.LastIndexOf("(") + 1);
                strEdataFolderName = strEdataFolderName.Replace(")", "");

                intDataFileCount = Directory.GetFiles(strShipDataFolderPath, "*.*", System.IO.SearchOption.AllDirectories).Length;

                DirectoryInfo dInfoDataFolder = new DirectoryInfo(strShipDataFolderPath);
                douDataFileSize = DirectorySize(dInfoDataFolder, true);

                while (true)
                {
                    strDestFolderPath = Path.Combine(shippingPathLD, "Edata_" + shipdate.ToString("yyyyMMdd") + "-" + iTotalTemp);
                    strDestEdataFolderPath = Path.Combine(strDestFolderPath, strEdataFolderName);

                    if (!Directory.Exists(strDestFolderPath))
                    {
                        Directory.CreateDirectory(strDestFolderPath);
                        intTempFileCount = 0;
                        douTempFileSize = 0.0;
                    }
                    else
                    {
                        if (!Directory.Exists(strDestEdataFolderPath))
                        {
                            intTempFileCount = Directory.GetFiles(strDestFolderPath, "*.*", System.IO.SearchOption.AllDirectories).Length;

                            DirectoryInfo dInfoTempFolder = new DirectoryInfo(strDestFolderPath);
                            douTempFileSize = DirectorySize(dInfoTempFolder, true);
                        }
                        else
                        {
                            iTotalTemp++;
                            continue; // ไปยังการวนลูปครั้งถัดไป
                        }
                    }

                    if ((intTempFileCount + intDataFileCount > 255) || (((douTempFileSize + douDataFileSize) / 1000000) > 10))
                    {
                        if (douTempFileSize == 0 && ((intDataFileCount > 255) || (((douDataFileSize) / 1000000) > 10)))
                        {
                            FileSystem.MoveDirectory(strShipDataFolderPath, strDestEdataFolderPath, true);
                        }
                        else
                        {
                            iTotalTemp++;
                            continue; // ไปยังการวนลูปครั้งถัดไป
                        }
                    }
                    else
                    {
                        FileSystem.MoveDirectory(strShipDataFolderPath, strDestEdataFolderPath, true);
                    }

                    bfnStatus = true;
                    break; // ออกจากลูป while

                }
            }

            return bfnStatus;
        }

        public bool fnCopyFile(string src, string dest)
        {
            try
            {
                string fileName = Path.GetFileName(src); // ชื่อไฟล์ต้นทาง
                string destinationFile = Path.Combine(dest, fileName); // สร้างเส้นทางไฟล์ปลายทาง

                // ทำการคัดลอกไฟล์
                File.Copy(src, destinationFile, true);

                return true;
            }

            catch (Exception ex)
            {
                string errorMessage = "เกิดข้อผิดพลาดในการ copy file: " + ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " innerexception : " + ex.InnerException.Message;
                }
                MessageBox.Show(errorMessage);
                return false;
            }
        }


        public DataTable CheckFoldersInZip(List<string> selectedFolders, List<string> zipFiles)
        {

            Dictionary<string, HashSet<string>> zipFileContents = new Dictionary<string, HashSet<string>>();

            // Read all directories from each ZIP archive
            foreach (string zipFilePath in zipFiles)
            {
                if (File.Exists(zipFilePath))
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                    {
                        HashSet<string> folderNames = new HashSet<string>();

                        foreach (var entry in archive.Entries)
                        {
                            string folder = Path.GetDirectoryName(entry.FullName);
                            if (!string.IsNullOrEmpty(folder))
                            {
                                folderNames.Add(folder);
                            }
                        }
                        zipFileContents[zipFilePath] = folderNames;
                    }
                }
            }

            // Initialize a DataTable to store the result
            DataTable resultTable = new DataTable();
            resultTable.Columns.Add("folder", typeof(string));
            resultTable.Columns.Add("attached", typeof(string));

            List<string> missingFolders = new List<string>();
            bool allFoldersFound = true;

            // Check each selected folder in the ZIP files
            foreach (string folder in selectedFolders)
            {
                bool folderFound = false;
                string zipFileName = "";
                foreach (var zipEntry in zipFileContents)
                {
                    foreach (var entry in zipEntry.Value)
                    {
                        if (entry.Contains(folder))
                        {
                            folderFound = true;
                            zipFileName = Path.GetFileName(zipEntry.Key);
                            bool zipFileNameExists = false;
                            foreach (DataRow row in resultTable.Rows)
                            {
                                if (row["folder"].ToString() == zipFileName)
                                {
                                    zipFileNameExists = true;
                                    break;
                                }
                            }

                            // Add the row if zipFileName does not exist in resultTable
                            if (!zipFileNameExists)
                            {
                                resultTable.Rows.Add(zipFileName, folder);
                            }

                            break;
                        }
                    }

                    if (folderFound)
                    {
                        break;
                    }
                }

                if (!folderFound)
                {
                    missingFolders.Add(folder);
                    allFoldersFound = false;
                }
            }

            if (resultTable.Rows.Count == 0)
            {
                MessageBox.Show("No folders were found.");
                return resultTable;
            }

            if (!allFoldersFound)
            {
                string missingFolderList = string.Join(", ", missingFolders);
                MessageBox.Show("The following folders are missing: " + missingFolderList);
            }

            return resultTable;
        }

        public List<string> GetEmailAddresses(string section, string key)
        {
            try
            {
                string filePath = ConfigurationManager.AppSettings["SettingEmail"].ToString();
                List<string> emailAddresses = new List<string>();

                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    bool sectionFound = false;

                    // แปลง section และ key ให้เป็นตัวพิมพ์ใหญ่
                    string upperSection = section.ToUpper();
                    string upperKey = key.ToUpper();

                    foreach (string line in lines)
                    {
                        string upperLine = line.ToUpper().Trim();
                        if (upperLine.StartsWith($"[{upperSection}]"))
                        {
                            sectionFound = true;
                            continue;
                        }

                        if (sectionFound)
                        {
                            if (upperLine.StartsWith("[") && upperLine.EndsWith("]"))
                            {
                                // พบ section ใหม่, หยุดการค้นหา
                                break;
                            }

                            if (upperLine.StartsWith($"{upperKey}="))
                            {
                                string[] addresses = line.Substring(line.IndexOf('=') + 1).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string address in addresses)
                                {
                                    emailAddresses.Add(address.Trim());
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("ไม่พบไฟล์ email setting :" + filePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }

                if (emailAddresses.Count == 0)
                {
                    MessageBox.Show("โปรดตรวจสอบไฟล์ email setting :" + filePath + "\n" + "[ " + section + " ] และ " + key + "= มีตามนี้หรือไม่", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return emailAddresses.Count > 0 ? emailAddresses : null;

            }
            catch(Exception ex)
            {
                MessageBox.Show("พบข้อบกพร่อง : " + ex.InnerException , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
