using System;
using System.Configuration; // สำหรับอ่าน app.config
using System.Net;
using System.Linq; // สำหรับ .Any() และ .Split()

/// <summary>
/// คลาสสำหรับเช็ค IP ของเครื่องว่าตรงกับ allowIP ใน app.config หรือไม่
/// allowIP ใน app.config สามารถเป็น string เดี่ยว เช่น "192.168.1.100" หรือ list เช่น "192.168.1.100,192.168.1.101"
/// </summary>
public static class IPChecker
{
    /// <summary>
    /// เช็คว่า IP ของเครื่องนี้ allowed หรือไม่
    /// </summary>
    /// <returns>true ถ้า allowed, false ถ้าไม่</returns>
    public static bool IsAllowed()
    {
        string allowedIPsString = GetAllowedIPs();
        if (string.IsNullOrEmpty(allowedIPsString))
        {
            return false;
        }

        string[] allowedIPs = allowedIPsString.Split(',').Select(ip => ip.Trim()).ToArray();
        string localIP = GetLocalIP();
        if (string.IsNullOrEmpty(localIP))
        {
            return true;
        }

        return allowedIPs.Any(allowIP => allowIP.Equals(localIP, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// ดึง string ของ allowed IPs จาก app.config (เช่น "192.168.1.100,192.168.1.101")
    /// </summary>
    /// <returns>allowed IPs string หรือ empty ถ้าไม่มี</returns>
    public static string GetAllowedIPs()
    {
        return ConfigurationManager.AppSettings["allowIP"] ?? "";
    }

    /// <summary>
    /// ดึง local IPv4 ของเครื่อง
    /// </summary>
    /// <returns>IP string หรือ "ไม่สามารถดึง IP ได้" ถ้าดึงไม่ได้</returns>
    public static string GetLocalIP()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "ไม่สามารถดึง IP ได้";
        }
        catch
        {
            return "ไม่สามารถดึง IP ได้";
        }
    }
}