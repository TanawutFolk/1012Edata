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
    public class SettingUserServices : DatabaseAction<SettingUserProperty>
    {
        private string sql;

        OutputOnDbProperty resultData = new OutputOnDbProperty();
        SettingUserSQLFactory sqlFactory = new SettingUserSQLFactory();

        private void UseSettingConnection()
        {
            // qa_oc_test while developing (App.config -> ConnectionQAOC)
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
        }

        public OutputOnDbProperty searchUserForLogin(SettingUserProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.searchUserForLogin(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchAllUsers()
        {
            UseSettingConnection();
            sql = sqlFactory.searchAllUsers();
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchEmployee(SettingUserProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.searchEmployee(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty searchUserRow(SettingUserProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.searchUserRow(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty countActiveAdmins(SettingUserProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.countActiveAdmins(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty insertUser(SettingUserProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.insertUser(dataItem);
            resultData = base.InsertBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty updateUserRole(SettingUserProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.updateUserRole(dataItem);
            resultData = base.UpdateBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty updateUserInuse(SettingUserProperty dataItem)
        {
            UseSettingConnection();
            sql = sqlFactory.updateUserInuse(dataItem);
            resultData = base.UpdateBySql(sql);
            return resultData;
        }

        public override OutputOnDbProperty Delete(SettingUserProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Insert(SettingUserProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search(SettingUserProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search()
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Update(SettingUserProperty dataItem)
        {
            throw new NotImplementedException();
        }
    }
}
