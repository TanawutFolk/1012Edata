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
    public class SettingEmailServices : DatabaseAction<SettingEmailProperty>
    {
        private string sql;

        OutputOnDbProperty resultData = new OutputOnDbProperty();
        SettingEmailSQLFactory sqlFactory = new SettingEmailSQLFactory();

        private void UseSettingConnection()
        {
            // qa_oc_test while developing (App.config -> ConnectionQAOC)
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
        }

        public OutputOnDbProperty searchCustomers(SettingEmailProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.searchCustomers(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchGroupOfCustomer(SettingEmailProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.searchGroupOfCustomer(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchDocumentGroup(SettingEmailProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.searchDocumentGroup(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchEdataTypes()
        {
            UseSettingConnection();
            sql = sqlFactory.searchEdataTypes();
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchConfiguredEdataTypes(SettingEmailProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.searchConfiguredEdataTypes(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchCustomersSharingGroup(SettingEmailProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.searchCustomersSharingGroup(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchGroupMembers(SettingEmailProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.searchGroupMembers(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchAllGroups()
        {
            UseSettingConnection();
            sql = sqlFactory.searchAllGroups();
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchGroupMatrix(List<SettingEmailProperty> products)
        {
            UseSettingConnection();
            sql = sqlFactory.searchGroupMatrix(products);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchGroupUsage(SettingEmailProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.searchGroupUsage(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchEmailByAddress(SettingEmailProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.searchEmailByAddress(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty maxEmailId()
        {
            UseSettingConnection();
            sql = sqlFactory.maxEmailId();
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty maxGroupId()
        {
            UseSettingConnection();
            sql = sqlFactory.maxGroupId();
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        /// <summary>Run every staged statement of one Save inside a single transaction.</summary>
        public OutputOnDbProperty saveChanges(List<string> sqlList)
        {
            UseSettingConnection();
            resultData = base.UpdateBySqlList(sqlList);
            return resultData;
        }

        public override OutputOnDbProperty Delete(SettingEmailProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Insert(SettingEmailProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search(SettingEmailProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search()
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Update(SettingEmailProperty dataItem)
        {
            throw new NotImplementedException();
        }
    }
}
