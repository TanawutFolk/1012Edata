using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.Property
{
    /// <summary>
    /// A person allowed to open the Setting screen (table user_setting_edata
    /// joined with mfg.info_employees).
    /// </summary>
    public class SettingUserProperty
    {
        public const string ROLE_ADMIN = "ADMIN";
        public const string ROLE_USER = "USER";

        public string empCode { get; set; }
        public string empName { get; set; }
        public string empSurname { get; set; }
        public string empDept { get; set; }
        public string empSection { get; set; }
        public string empEmail { get; set; }
        public string empStatus { get; set; }     // ACTIVE / RESIGNED

        public string role { get; set; }          // ADMIN / USER, empty = not registered
        public int inuse { get; set; }
        public string remark { get; set; }
        public string createUser { get; set; }

        public bool isAdmin
        {
            get { return string.Equals(role, ROLE_ADMIN, StringComparison.OrdinalIgnoreCase); }
        }

        public bool isActiveEmployee
        {
            get { return !string.Equals(empStatus, "RESIGNED", StringComparison.OrdinalIgnoreCase); }
        }

        public string fullName
        {
            get { return ((empName ?? "") + " " + (empSurname ?? "")).Trim(); }
        }
    }
}
