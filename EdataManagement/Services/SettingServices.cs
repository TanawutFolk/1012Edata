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
    public class SettingServices : DatabaseAction<SettingProperty>
    {

        private string sql;

        OutputOnDbProperty resultData = new OutputOnDbProperty();
        SettingSQLFactory sqlFactory = new SettingSQLFactory();

        public OutputOnDbProperty maxCusID(SettingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.maxCusID(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty maxKeyID(SettingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.maxKeyID(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty insertUpdateKeySendMethods(SettingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.insertUpdateKeySendMethods(dataItem);
            resultData = base.InsertBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty insertUpdateEmailSubject(SettingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.insertUpdateEmailSubject(dataItem);
            resultData = base.InsertBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty insertUpdateEmailBody(SettingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.insertUpdateEmailBody(dataItem);
            resultData = base.InsertBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty insertUpdateEmailAttach(SettingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.insertUpdateEmailAttach(dataItem);
            resultData = base.InsertBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty insertUpdateKeyGroupEmail(SettingProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.insertUpdateKeyGroupEmail(dataItem);
            resultData = base.InsertBySql(sql);
            return resultData;
        }



        public override OutputOnDbProperty Delete(SettingProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Insert(SettingProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search(SettingProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search()
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Update(SettingProperty dataItem)
        {
            throw new NotImplementedException();
        }
    }
}
