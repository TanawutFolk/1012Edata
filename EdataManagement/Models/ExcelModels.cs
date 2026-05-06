using BusinessData.Property;
using EdataManagement.Property;
using EdataManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.Models
{
    public class ExcelModels
    {
        OutputOnDbProperty _resultData = new OutputOnDbProperty();
        ExcelServices _services = new ExcelServices();


        //controller ให้ใช้ datatable ไม่ก็ ตัวแปรรับมาจาก Property Setting ก็ดี
        public OutputOnDbProperty infoPOallValue(ExcelProperty dataItem)
        {
            _resultData = _services.infoPOallValue(dataItem);
            return _resultData;
        }
    }
}
