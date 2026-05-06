using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.Property
{
    public class ExcelProperty
    {
        public string pathPOall { get; set; }
        public int startRow { get; set; }
        public string keyColumn{ get; set; }
        public string keyColumnName { get; set; }
        public string valueColumn { get; set; }
        public string valueColumnName { get; set; }
        public string input { get; set; }

        //where ที่ใช้
        public string productID { get; set; }
        public string VARIABLE { get; set; }
    }
}
