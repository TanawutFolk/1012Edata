using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdataManagement.Property
{
    public class ShippingProperty
    {
        public string conn { get; set; }
        public string key { get; set; }
        public string shipdate { get; set; }
        public string customerPN { get; set; }
        public string partNo { get; set; }
        public string customerName { get; set; }
        public string sendMethodsID  { get ; set; }
        public string qty { get; set; }
        public string fecOrderNo { get; set; }
        public string testReportNo {get;set; }
        public string productName { get; set; }
        public string dbProduct { get; set; }
        public string productNameInEmail { get; set; }
        public string productNameInFileServer { get; set; }
        public DataGridView dataGridViewShipping { get; set; }
        public string subFolder { get; set; }
        public DataTable dtSerial { get; set; }
    }
}
