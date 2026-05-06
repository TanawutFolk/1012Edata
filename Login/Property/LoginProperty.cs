using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.Property
{
    public class LoginProperty
    {
        public bool loginStatus { get; set; }
        public EmpProperty empDetial { get; set; }
    }

    public class EmpProperty
    {
        public string code { get; set; }
        public string name { get; set; }
        public string sername { get; set; }
        public string pass { get; set; }
        public string position { get; set; }
        public string depertment { get; set; }
        public string email { get; set; }
        public string startwork { get; set; }
    }
}
