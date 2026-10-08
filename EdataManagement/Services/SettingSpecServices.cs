using BusinessData.Interface;
using BusinessData.Property;

using EdataManagement.Property;
using EdataManagement.SQLFactory;

using System;
using System.Configuration;

namespace EdataManagement.Services
{
    public class SettingSpecServices : DatabaseAction<SettingSpecProperty>
    {
        private string sql;

        OutputOnDbProperty resultData = new OutputOnDbProperty();
        SettingSpecSQLFactory sqlFactory = new SettingSpecSQLFactory();

        private void UseSettingConnection()
        {
            // the config database of App.config -> ConnectionQAOC (qa_oc_test_tanawut while developing)
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
        }

        private OutputOnDbProperty RunSearch(string query)
        {
            UseSettingConnection();
            sql = query;
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchCustomers(SettingSpecProperty dataItem)
        {
            return RunSearch(sqlFactory.searchCustomers(dataItem));
        }

        public OutputOnDbProperty searchSpecs(SettingSpecProperty dataItem)
        {
            return RunSearch(sqlFactory.searchSpecs(dataItem));
        }

        public OutputOnDbProperty searchSendMethods()
        {
            return RunSearch(sqlFactory.searchSendMethods());
        }

        public OutputOnDbProperty searchFileFormats()
        {
            return RunSearch(sqlFactory.searchFileFormats());
        }

        public OutputOnDbProperty searchSubjects()
        {
            return RunSearch(sqlFactory.searchSubjects());
        }

        public OutputOnDbProperty searchBodies()
        {
            return RunSearch(sqlFactory.searchBodies());
        }

        public OutputOnDbProperty searchSpecGroups()
        {
            return RunSearch(sqlFactory.searchSpecGroups());
        }

        public OutputOnDbProperty maxGroupId()
        {
            return RunSearch(sqlFactory.maxGroupId());
        }

        public override OutputOnDbProperty Delete(SettingSpecProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Insert(SettingSpecProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search(SettingSpecProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search()
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Update(SettingSpecProperty dataItem)
        {
            throw new NotImplementedException();
        }
    }
}
