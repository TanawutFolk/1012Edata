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
    public class SettingModels
    {
        OutputOnDbProperty _resultData = new OutputOnDbProperty();
        SettingServices _services = new SettingServices();

        public OutputOnDbProperty maxCusID(SettingProperty dataItem)
        {
            _resultData = _services.maxCusID(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty maxKeyID(SettingProperty dataItem)
        {
            _resultData = _services.maxKeyID(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty insertUpdateKeySendMethods(SettingProperty dataItem)
        {
            _resultData = _services.insertUpdateKeySendMethods(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty insertUpdateEmailSubject(SettingProperty dataItem)
        {
            _resultData = _services.insertUpdateEmailSubject(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty insertUpdateEmailBody(SettingProperty dataItem)
        {
            _resultData = _services.insertUpdateEmailBody(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty insertUpdateEmailAttach(SettingProperty dataItem)
        {
            _resultData = _services.insertUpdateEmailAttach(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty insertUpdateKeyGroupEmail(SettingProperty dataItem)
        {
            _resultData = _services.insertUpdateKeyGroupEmail(dataItem);
            return _resultData;
        }


        //controller ให้ใช้ datatable ไม่ก็ ตัวแปรรับมาจาก Property Setting ก็ดี
        //public OutputOnDbProperty infoPOallValue(SettingProperty dataItem)
        //{
        //    _resultData = _services.infoPOallValue(dataItem);
        //    return _resultData;
        //}

    }
}
