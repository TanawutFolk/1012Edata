using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace EdataManagement.Utilities
{
    /// <summary>
    /// After the FTP upload of an order the user uploads the same serial files to DUC by hand.
    /// 1012 prepares them: every serial folder ({Customer PN}_Furukawa_{qty}_{FEC order}-n) that held an
    /// uploaded .txt is zipped into {yyMMdd-CUSTOMER(48)}-For DUC\{serial folder}.zip, next to the customer folder.
    /// The zip is the same as Windows "Send to > Compressed folder": the folder itself on top, then its files.
    /// Sending the order again rebuilds its zip.
    /// </summary>
    public static class DucZipCls
    {
        public const string FOLDER_SUFFIX = "-For DUC";

        /// <summary>{customer folder}-For DUC, next to the customer folder.</summary>
        public static string DucFolderOf(string customerFolder)
        {
            string trimmed = customerFolder.TrimEnd('\\', '/');
            return Path.Combine(Path.GetDirectoryName(trimmed), Path.GetFileName(trimmed) + FOLDER_SUFFIX);
        }

        /// <summary>The For DUC folder also contains the customer name - never take it for the customer folder.</summary>
        public static bool IsDucFolder(string folder)
        {
            return folder.TrimEnd('\\', '/').EndsWith(FOLDER_SUFFIX, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Zip the serial folders of uploadedFiles into the For DUC folder and return the zip paths.
        /// Throws when a file is not in {customer folder}\{order folder}\{serial folder}\ or a zip does not read back complete.
        /// </summary>
        public static List<string> PrepareZips(string customerFolder, IEnumerable<string> uploadedFiles)
        {
            string customer = Path.GetFullPath(customerFolder).TrimEnd('\\', '/');
            List<string> serialFolders = uploadedFiles
                .Select(f => Path.GetDirectoryName(Path.GetFullPath(f)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // only {customer}\{order}\{serial}\ - never zip an order folder (LDS, xls, For QA check)
            foreach (string folder in serialFolders)
            {
                DirectoryInfo order = Directory.GetParent(folder);
                if (order == null || order.Parent == null ||
                    !string.Equals(order.Parent.FullName.TrimEnd('\\'), customer, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("ไฟล์ serial ไม่ได้อยู่ในโฟลเดอร์ {order}\\{serial} ของ " + customer + "\n" + folder);
                }
            }

            string ducFolder = DucFolderOf(customer);
            Directory.CreateDirectory(ducFolder);

            List<string> zipPaths = new List<string>();
            foreach (string folder in serialFolders)
            {
                string name = Path.GetFileName(folder);
                string zipPath = Path.Combine(ducFolder, name + ".zip");
                string tempPath = zipPath + ".tmp";
                List<string> files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                try
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);

                    using (FileStream stream = new FileStream(tempPath, FileMode.CreateNew))
                    using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Create))
                    {
                        zip.CreateEntry(name + "/").LastWriteTime = Directory.GetLastWriteTime(folder);
                        foreach (string file in files)
                        {
                            string relative = file.Substring(folder.Length).TrimStart('\\').Replace('\\', '/');
                            zip.CreateEntryFromFile(file, name + "/" + relative, CompressionLevel.Optimal);
                        }
                    }

                    // read it back: every file of the serial folder, same size
                    using (ZipArchive check = ZipFile.OpenRead(tempPath))
                    {
                        Dictionary<string, long> entries = check.Entries
                            .Where(e => !e.FullName.EndsWith("/"))
                            .ToDictionary(e => e.FullName, e => e.Length, StringComparer.OrdinalIgnoreCase);
                        int complete = files.Count(f =>
                        {
                            long length;
                            string entryName = name + "/" + f.Substring(folder.Length).TrimStart('\\').Replace('\\', '/');
                            return entries.TryGetValue(entryName, out length) && length == new FileInfo(f).Length;
                        });
                        if (entries.Count != files.Count || complete != files.Count)
                        {
                            throw new InvalidOperationException($"ไฟล์ zip ไม่ครบ ({complete}/{files.Count} ไฟล์)\n{zipPath}");
                        }
                    }

                    if (File.Exists(zipPath)) File.Delete(zipPath);
                    File.Move(tempPath, zipPath);
                }
                finally
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                }

                zipPaths.Add(zipPath);
            }

            return zipPaths;
        }
    }
}
