using BusinessData.Interface;
using BusinessData.Property;
using BusinessData.Models;

using Login.Property;
using Login.SQLFactory;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Login.Services
{
    public class LoginServices : DatabaseAction<LoginProperty>
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        LoginSQLFactory _sqlFactory = new LoginSQLFactory();

        private string sql;

        public  OutputOnDbProperty SearchEmpCompany(EmpProperty dataItem)
        {
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionLogin"].ConnectionString;
            sql = _sqlFactory.SearchEmpCompany(dataItem);
            resultData = base.SearchBySql(sql);
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionStrMysql"].ConnectionString;
            return resultData;
        }
        public OutputOnDbProperty SearchEmpFitelGroup(EmpProperty dataItem)
        {
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionPersonnel"].ConnectionString;
            sql = _sqlFactory.SearchEmpFitelGroup(dataItem);
            resultData = base.SearchBySql(sql);
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionStrMysql"].ConnectionString;
            return resultData;
        }
        public OutputOnDbProperty LoginFitelGroup(EmpProperty dataItem)
        {
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionPersonnel"].ConnectionString;
            sql = _sqlFactory.LoginFitelGroup(dataItem);
            resultData = base.SearchBySql(sql);
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionStrMysql"].ConnectionString;
            return resultData;
        }

        public OutputOnDbProperty InsertLoginFitelGroup(EmpProperty dataItem)
        {
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionPersonnel"].ConnectionString;
            sql = _sqlFactory.InsertLoginFitelGroup(dataItem);
            resultData = base.InsertBySql(sql);
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionStrMysql"].ConnectionString;
            return resultData;
        }

        public OutputOnDbProperty UpdatePassLoginFitelGroup(EmpProperty dataItem)
        {
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionPersonnel"].ConnectionString;
            sql = _sqlFactory.UpdatePassLoginFitelGroup(dataItem);
            resultData = base.UpdateBySql(sql);
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionStrMysql"].ConnectionString;
            return resultData;


        }

        

        public OutputOnDbProperty DeleteOTP(EmpProperty dataItem)
        {
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionPersonnel"].ConnectionString;
            sql = _sqlFactory.DeleteOTP(dataItem);
            resultData = base.DeleteBySql(sql);
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionStrMysql"].ConnectionString;
            return resultData;
        }

        public OutputOnDbProperty InsertOTP(EmpProperty dataItem, string OTP)
        {
            //Delete OTP Hittory 
            OutputOnDbProperty _resultData = this.DeleteOTP(dataItem);

            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionPersonnel"].ConnectionString;
            sql = _sqlFactory.InsertOTP(dataItem, OTP);
            resultData = base.InsertBySql(sql);
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionStrMysql"].ConnectionString;
            return resultData;
        }

        public OutputOnDbProperty SearchOTP(EmpProperty dataItem, string OTP)
        {
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionPersonnel"].ConnectionString;
            sql = _sqlFactory.SearchOTP(dataItem, OTP);
            resultData = base.SearchBySql(sql);
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionStrMysql"].ConnectionString;
            return resultData;
        }

        public OutputOnDbProperty UpdateOTPUsed(EmpProperty dataItem, string OTP)
        {
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionPersonnel"].ConnectionString;
            sql = _sqlFactory.UpdateOTPUsed(dataItem, OTP);
            resultData = base.UpdateBySql(sql);
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionStrMysql"].ConnectionString;
            return resultData;
        }

        public OutputOnDbProperty GetDatatime()
        {
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionPersonnel"].ConnectionString;
            sql = _sqlFactory.GetDatatime();
            resultData = base.SearchBySql(sql);
            TransMysqlModels.strConnection = ConfigurationManager.ConnectionStrings["ConnectionStrMysql"].ConnectionString;
            return resultData;
        }



        public override OutputOnDbProperty Delete(LoginProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Insert(LoginProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search(LoginProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search()
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Update(LoginProperty dataItem)
        {
            throw new NotImplementedException();
        }
    }
}
