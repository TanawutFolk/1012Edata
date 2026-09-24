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
    public class SettingUserModels
    {
        OutputOnDbProperty _resultData = new OutputOnDbProperty();
        SettingUserServices _services = new SettingUserServices();

        public OutputOnDbProperty searchUserForLogin(SettingUserProperty dataItem)
        {
            _resultData = _services.searchUserForLogin(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty searchAllUsers()
        {
            _resultData = _services.searchAllUsers();
            return _resultData;
        }

        public OutputOnDbProperty searchEmployee(SettingUserProperty dataItem)
        {
            _resultData = _services.searchEmployee(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty searchUserRow(SettingUserProperty dataItem)
        {
            _resultData = _services.searchUserRow(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty countActiveAdmins(SettingUserProperty dataItem)
        {
            _resultData = _services.countActiveAdmins(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty insertUser(SettingUserProperty dataItem)
        {
            _resultData = _services.insertUser(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty updateUserRole(SettingUserProperty dataItem)
        {
            _resultData = _services.updateUserRole(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty updateUserInuse(SettingUserProperty dataItem)
        {
            _resultData = _services.updateUserInuse(dataItem);
            return _resultData;
        }
    }
}
