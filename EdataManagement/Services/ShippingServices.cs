using BusinessData.Interface;
using BusinessData.Property;

using EdataManagement.Property;
using EdataManagement.SQLFactory;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.Services
{
    public class ShippingServices : DatabaseAction<ShippingProperty>
    {
        private string sql;

        OutputOnDbProperty resultData = new OutputOnDbProperty();
        ShippingSQLFactory sqlFactory = new ShippingSQLFactory();

        public OutputOnDbProperty SearchCusPartNo(ShippingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.SearchCustPartNo(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchCustomerShipping(ShippingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.searchCustomerShipping(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty SearchSerialShipping(ShippingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.SearchSerialShipping(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty SearchSerialShippingCust(ShippingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.SearchSerialShippingCust(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty getSendMethods(ShippingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.getSendMethods(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }
        public override OutputOnDbProperty Delete(ShippingProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Insert(ShippingProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search(ShippingProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search()
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Update(ShippingProperty dataItem)
        {
            throw new NotImplementedException();
        }
    }
}
