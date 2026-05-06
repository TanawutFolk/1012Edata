using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace BusinessData.Property
{
    public class OutputOnDbProperty
    {
        public bool StatusOnDb { get; set; }
        public string ClassOnDb { get; set; }
        public string MethodOnDb { get; set; }
        public DataTable ResultOnDb { get; set; }
        public string MessageOnDb {get; set;}
        public string TotalCountOnDb { get; set; }
        public string IndexIdOnDb { get; set; }
    }
}