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
    public class SettingEmailModels
    {
        OutputOnDbProperty _resultData = new OutputOnDbProperty();
        SettingEmailServices _services = new SettingEmailServices();

        public OutputOnDbProperty searchCustomers(SettingEmailProperty dataItem)
        {
            _resultData = _services.searchCustomers(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty searchGroupOfCustomer(SettingEmailProperty dataItem)
        {
            _resultData = _services.searchGroupOfCustomer(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty searchDocumentGroup(SettingEmailProperty dataItem)
        {
            _resultData = _services.searchDocumentGroup(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty searchEdataTypes()
        {
            _resultData = _services.searchEdataTypes();
            return _resultData;
        }

        public OutputOnDbProperty searchCustomersSharingGroup(SettingEmailProperty dataItem)
        {
            _resultData = _services.searchCustomersSharingGroup(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty searchGroupMembers(SettingEmailProperty dataItem)
        {
            _resultData = _services.searchGroupMembers(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty searchAllGroups()
        {
            _resultData = _services.searchAllGroups();
            return _resultData;
        }

        public OutputOnDbProperty searchGroupMatrix(List<SettingEmailProperty> products)
        {
            _resultData = _services.searchGroupMatrix(products);
            return _resultData;
        }

        public OutputOnDbProperty searchGroupUsage(SettingEmailProperty dataItem)
        {
            _resultData = _services.searchGroupUsage(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty searchEmailByAddress(SettingEmailProperty dataItem)
        {
            _resultData = _services.searchEmailByAddress(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty maxEmailId()
        {
            _resultData = _services.maxEmailId();
            return _resultData;
        }

        public OutputOnDbProperty maxGroupId()
        {
            _resultData = _services.maxGroupId();
            return _resultData;
        }

        public OutputOnDbProperty saveChanges(List<string> sqlList)
        {
            _resultData = _services.saveChanges(sqlList);
            return _resultData;
        }
    }
}
