using BusinessData.Property;
using EdataManagement.Property;
using EdataManagement.Services;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.Models
{
    public class ShippingModels
    {
        OutputOnDbProperty _resultData = new OutputOnDbProperty();
        ShippingServices _services = new ShippingServices();

        public OutputOnDbProperty SearchCusPartNo(ShippingProperty dataItem)
        {
            _resultData = _services.SearchCusPartNo(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty searchCustomerShipping(ShippingProperty dataItem)
        {
            _resultData = _services.searchCustomerShipping(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty SearchSerialShipping(ShippingProperty dataItem)
        {
            _resultData = _services.SearchSerialShipping(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty SearchSerialShippingCust(ShippingProperty dataItem)
        {
            _resultData = _services.SearchSerialShippingCust(dataItem);
            return _resultData;
        }


        public OutputOnDbProperty getSendMethods(ShippingProperty dataItem)
        {
            _resultData = _services.getSendMethods(dataItem);
            return _resultData;
        }
    }
}
