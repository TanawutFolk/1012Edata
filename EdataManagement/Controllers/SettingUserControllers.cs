using BusinessData.Property;

using EdataManagement.Models;
using EdataManagement.Property;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.Controllers
{
    public class SettingUserControllers
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        SettingUserModels models = new SettingUserModels();

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

        private bool RunWrite(Func<OutputOnDbProperty> call)
        {
            lastError = "";

            try
            {
                resultData = call();

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

        /// <summary>
        /// Login lookup. Returns null when the employee code does not exist at all.
        /// A returned object with role = "" means the employee is not on the whitelist.
        /// </summary>
        public SettingUserProperty searchUserForLogin(SettingUserProperty dataItem)
        {
            DataTable dt = Run(() => models.searchUserForLogin(dataItem));

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];

            return new SettingUserProperty
            {
                empCode = row["EMPCODE"].ToString(),
                empName = row["EMP_NAME"].ToString(),
                empSurname = row["EMP_SURNAME"].ToString(),
                empDept = row["EMP_DEPT"].ToString(),
                empSection = row["EMP_SECTION"].ToString(),
                empEmail = row["EMP_EMAIL"].ToString(),
                empStatus = row["EMP_STATUS"].ToString(),
                role = row["ROLE"].ToString().Trim().ToUpper(),
                inuse = Convert.ToInt32(row["INUSE"])
            };
        }

        public DataTable searchAllUsers()
        {
            return Run(() => models.searchAllUsers());
        }

        /// <summary>Employee master lookup, null when the code is unknown.</summary>
        public SettingUserProperty searchEmployee(SettingUserProperty dataItem)
        {
            DataTable dt = Run(() => models.searchEmployee(dataItem));

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];

            return new SettingUserProperty
            {
                empCode = row["EMPCODE"].ToString(),
                empName = row["EMP_NAME"].ToString(),
                empSurname = row["EMP_SURNAME"].ToString(),
                empDept = row["EMP_DEPT"].ToString(),
                empSection = row["EMP_SECTION"].ToString(),
                empStatus = row["EMP_STATUS"].ToString()
            };
        }

        public int countActiveAdmins(SettingUserProperty dataItem)
        {
            DataTable dt = Run(() => models.countActiveAdmins(dataItem));
            if (dt.Rows.Count == 0) return 0;
            return Convert.ToInt32(dt.Rows[0]["ADMIN_COUNT"]);
        }

        public bool insertUser(SettingUserProperty dataItem)
        {
            return RunWrite(() => models.insertUser(dataItem));
        }

        public bool updateUserRole(SettingUserProperty dataItem)
        {
            return RunWrite(() => models.updateUserRole(dataItem));
        }

        public bool updateUserInuse(SettingUserProperty dataItem)
        {
            return RunWrite(() => models.updateUserInuse(dataItem));
        }
    }
}
