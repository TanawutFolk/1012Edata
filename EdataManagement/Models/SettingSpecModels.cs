using BusinessData.Property;

using EdataManagement.Property;
using EdataManagement.Services;

namespace EdataManagement.Models
{
    public class SettingSpecModels
    {
        OutputOnDbProperty _resultData = new OutputOnDbProperty();
        SettingSpecServices _services = new SettingSpecServices();

        public OutputOnDbProperty searchCustomers(SettingSpecProperty dataItem)
        {
            _resultData = _services.searchCustomers(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty searchSpecs(SettingSpecProperty dataItem)
        {
            _resultData = _services.searchSpecs(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty searchSendMethods()
        {
            _resultData = _services.searchSendMethods();
            return _resultData;
        }

        public OutputOnDbProperty searchFileFormats()
        {
            _resultData = _services.searchFileFormats();
            return _resultData;
        }

        public OutputOnDbProperty searchSubjects()
        {
            _resultData = _services.searchSubjects();
            return _resultData;
        }

        public OutputOnDbProperty searchBodies()
        {
            _resultData = _services.searchBodies();
            return _resultData;
        }

        public OutputOnDbProperty searchSpecGroups()
        {
            _resultData = _services.searchSpecGroups();
            return _resultData;
        }

        public OutputOnDbProperty searchGroupNames()
        {
            _resultData = _services.searchGroupNames();
            return _resultData;
        }

        public OutputOnDbProperty maxGroupId()
        {
            _resultData = _services.maxGroupId();
            return _resultData;
        }
    }
}
