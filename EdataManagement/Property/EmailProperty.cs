using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdataManagement.Property
{
    public class EmailProperty
    {
        //key / value
        public string key { get; set; }
        public string value { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public string signature { get; set; }
        public string productName { get; set; }
        public string partNo { get; set; }
        public string custName { get; set; }
        public string manuFactureNo { get; set; } //ManufactureNo / FECorderNo
        public int EmailId { get; set; }
        public string EmailAddress { get; set; }
        public int GroupId { get; set; }
        public int RecipientId { get; set; }
        public string fileFormatsId { get; set; }
        public string RecipientType { get; set; }
        public string pathShipping { get; set; }
        public DateTime shipDate { get; set; } //Shipdate
        public string folderCustomer { get; set; }
        public List<string> toEmailAddresses { get; set; }
        public List<string> ccEmailAddresses { get; set; }
        public List<string> attachmentFilePaths { get; set; }
        public int thisCount { get; set; }
        public int totalCount { get; set; }
        public DataTable dtShip { get; set; }
        public DataRowView selectedRowProduct { get; set; }
        public DataRowView selectedRowCustomer { get; set; }
        public DataGridView dgvShip { get; set; }
        public DataGridView dgvFile { get; set; }
        public DataGridView dgslFile { get; set; }
        public DataGridView dgwfFile { get; set; }
        //public List<string> dtFile { get; set; }
        public DataGridView dgvInfoEmail { get; set; }
        public bool isValid { get; set; }
        public string strLog { get; set; }
        public string emailSender {
            get
            {
                return ConfigurationManager.AppSettings["emailSender"];
            }
        }
        public string passwordSender
        {
            get
            {
                return ConfigurationManager.AppSettings["passwordSender"];
            }
        }
        public string smtpHost
        {
            get
            {
                return ConfigurationManager.AppSettings["smtpHost"];
            }
        }

        public string targetName
        {
            get
            {
                return ConfigurationManager.AppSettings["targetName"];
            }
        }

        public class replaceWord
        {

            public string productID { get; set; }
            public string subject { get; set; }
            public string body { get; set; }
            public string signature { get; set; }
            public string folderName { get; set; } // replace folderName to body
            public string manuFactureNo { get; set; } //ManufactureNo / FECorderNo
            public string Qty { get; set; } //Qty serial group by testReport
            public DateTime shipDate { get; set; } //Shipdate+FormatDate
            public string customerPN { get; set; } //CustomerPN
            public string productName { get; set; } //ProductName
            public string productNameInEmail { get; set; } //ProductName
            public string customerName { get; set; } //CustomerName
            public DataTable formatDate { get; set; }
            public string testReportNo { get; set; }
            public string pathPO { get; set; }
            public string HAWBNo { get; set; }
            public int thisCount { get; set; }
            public int totalCount { get; set; }
            public string fileFormatsId { get; set; }

            
        }

    }
}
