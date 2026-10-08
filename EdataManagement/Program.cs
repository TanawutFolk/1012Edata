using EdataManagement.Property;
using EdataManagement.Utilities;
using EdataManagement.Views;
using EdataManagement.Views.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
 
namespace EdataManagement
{
    internal static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // เช็ค IP ก่อนเลย ถ้าไม่ allowed หยุดโปรแกรม
            if (!IPChecker.IsAllowed())
            {
                string allowedIPs = IPChecker.GetAllowedIPs();
                string currentIP = IPChecker.GetLocalIP();

                MessageBox.Show($"IP ของเครื่องนี้ไม่ได้รับอนุญาต\n\n" +
                                $"IP ที่อนุญาต: {allowedIPs}\n" +
                                $"IP ของเครื่องนี้: {currentIP}",
                                "Access Denied",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Stop);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // เปิด frmMain เป็นหน้าแรก
            Application.Run(new frmLogin());
        }
    }
}
