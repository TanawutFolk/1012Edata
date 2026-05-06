using BusinessData.Property;

using Login.Property;
using Login.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.Models
{
    public class LoginModels
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        LoginServices _services = new LoginServices();

        public OutputOnDbProperty SearchEmpCompany(EmpProperty dataItem)
        {
            resultData = _services.SearchEmpCompany(dataItem);
            return resultData;
        }

        public OutputOnDbProperty SearchEmpFitelGroup(EmpProperty dataItem)
        {
            resultData = _services.SearchEmpFitelGroup(dataItem);
            return resultData;
        }
        public OutputOnDbProperty LoginFitelGroup(EmpProperty dataItem)
        {
            resultData = _services.LoginFitelGroup(dataItem);
            return resultData;
        }

        public OutputOnDbProperty InsertLoginFitelGroup(EmpProperty dataItem)
        {
            resultData = _services.InsertLoginFitelGroup(dataItem);
            return resultData;
        }

        public OutputOnDbProperty UpdatePassLoginFitelGroup(EmpProperty dataItem)
        {
            resultData = _services.UpdatePassLoginFitelGroup(dataItem);
            return resultData;
        }
        

        public OutputOnDbProperty InsertOTP(EmpProperty dataItem, string OTP)
        {
            resultData = _services.InsertOTP(dataItem, OTP);
            return resultData;
        }

        public OutputOnDbProperty SearchOTP(EmpProperty dataItem, string OTP)
        {
            resultData = _services.SearchOTP(dataItem, OTP);
            return resultData;
        }

        public OutputOnDbProperty UpdateOTPUsed(EmpProperty dataItem, string OTP)
        {
            resultData = _services.UpdateOTPUsed(dataItem, OTP);
            return resultData;
        }

        public OutputOnDbProperty GetDatatime()
        {
            resultData = _services.GetDatatime();
            return resultData;
        }

    }
}
