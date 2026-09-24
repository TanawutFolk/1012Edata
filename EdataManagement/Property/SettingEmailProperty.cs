using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.Property
{
    /// <summary>
    /// Data carrier for the Setting screen (frmSetting).
    /// </summary>
    public class SettingEmailProperty
    {
        // product context
        public string productId { get; set; }
        public string productName { get; set; }   // 1480 / 980 / 980Dual / 1480VE -> table suffix
        public string dbProduct { get; set; }     // ddb / 980ldm / ... -> info_cus_condition lives here

        // customer context
        public string customerName { get; set; }
        public int totalSpecs { get; set; }
        public int emailSpecs { get; set; }

        // group / member context
        public int groupId { get; set; }
        public int newGroupId { get; set; }
        public int emailId { get; set; }
        public string emailAddress { get; set; }
        public int recipientId { get; set; }      // 1 = TO, 2 = CC (info_recipient_type)

        // E-data type (info_document): 1 = Edata / LD data (group per spec), 2 = Serial List, 3 = Wafer (key_group_email_document)
        public int documentId { get; set; } = 1;

        public string createUser { get; set; }
    }
}
