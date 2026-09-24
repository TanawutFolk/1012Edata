using BusinessData.Property;

using EdataManagement.Models;
using EdataManagement.Property;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdataManagement.Controllers
{
    public class SettingEmailControllers
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        SettingEmailModels models = new SettingEmailModels();

        /// <summary>Message of the last failed call, so the form can show it once.</summary>
        public string lastError { get; private set; }

        private DataTable Run(Func<OutputOnDbProperty> call)
        {
            DataTable _result = new DataTable();
            lastError = "";

            try
            {
                resultData = call();

                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb != null)
                    {
                        _result = resultData.ResultOnDb;
                    }
                }
                else
                {
                    lastError = resultData.MessageOnDb;
                }
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                if (ex.InnerException != null)
                {
                    lastError += Environment.NewLine + "Inner: " + ex.InnerException.Message;
                }
            }

            return _result;
        }

        public DataTable searchCustomers(SettingEmailProperty dataItem)
        {
            return Run(() => models.searchCustomers(dataItem));
        }

        public DataTable searchGroupOfCustomer(SettingEmailProperty dataItem)
        {
            return Run(() => models.searchGroupOfCustomer(dataItem));
        }

        public DataTable searchDocumentGroup(SettingEmailProperty dataItem)
        {
            return Run(() => models.searchDocumentGroup(dataItem));
        }

        public DataTable searchEdataTypes()
        {
            return Run(() => models.searchEdataTypes());
        }

        public DataTable searchCustomersSharingGroup(SettingEmailProperty dataItem)
        {
            return Run(() => models.searchCustomersSharingGroup(dataItem));
        }

        public DataTable searchGroupMembers(SettingEmailProperty dataItem)
        {
            return Run(() => models.searchGroupMembers(dataItem));
        }

        public DataTable searchAllGroups()
        {
            return Run(() => models.searchAllGroups());
        }

        public DataTable searchGroupMatrix(List<SettingEmailProperty> products)
        {
            return Run(() => models.searchGroupMatrix(products));
        }

        public DataTable searchGroupUsage(SettingEmailProperty dataItem)
        {
            return Run(() => models.searchGroupUsage(dataItem));
        }

        public DataTable searchEmailByAddress(SettingEmailProperty dataItem)
        {
            return Run(() => models.searchEmailByAddress(dataItem));
        }

        public int maxEmailId()
        {
            DataTable dt = Run(() => models.maxEmailId());
            if (dt.Rows.Count == 0) return -1;
            return Convert.ToInt32(dt.Rows[0]["MAX_EMAIL_ID"]);
        }

        public int maxGroupId()
        {
            DataTable dt = Run(() => models.maxGroupId());
            if (dt.Rows.Count == 0) return -1;
            return Convert.ToInt32(dt.Rows[0]["MAX_GROUP_ID"]);
        }

        /// <summary>True when every staged statement was committed.</summary>
        public bool saveChanges(List<string> sqlList)
        {
            lastError = "";

            try
            {
                resultData = models.saveChanges(sqlList);

                if (resultData.StatusOnDb == false)
                {
                    lastError = resultData.MessageOnDb;
                }

                return resultData.StatusOnDb;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }
    }
}
