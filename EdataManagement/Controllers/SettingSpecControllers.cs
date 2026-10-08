using BusinessData.Property;

using EdataManagement.Models;
using EdataManagement.Property;

using System;
using System.Data;

namespace EdataManagement.Controllers
{
    /// <summary>Reads of the Spec Setting page. Writes go through SettingEmailControllers.saveChanges (one transaction).</summary>
    public class SettingSpecControllers
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        SettingSpecModels models = new SettingSpecModels();

        /// <summary>Message of the last failed call, so the page can show it once.</summary>
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

        public DataTable searchCustomers(SettingSpecProperty dataItem)
        {
            return Run(() => models.searchCustomers(dataItem));
        }

        public DataTable searchSpecs(SettingSpecProperty dataItem)
        {
            return Run(() => models.searchSpecs(dataItem));
        }

        public DataTable searchSendMethods()
        {
            return Run(() => models.searchSendMethods());
        }

        public DataTable searchFileFormats()
        {
            return Run(() => models.searchFileFormats());
        }

        public DataTable searchSubjects()
        {
            return Run(() => models.searchSubjects());
        }

        public DataTable searchBodies()
        {
            return Run(() => models.searchBodies());
        }

        public DataTable searchSpecGroups()
        {
            return Run(() => models.searchSpecGroups());
        }

        /// <summary>-1 when nothing is there yet; check lastError for a failed read.</summary>
        public int maxGroupId()
        {
            DataTable dt = Run(() => models.maxGroupId());
            if (dt.Rows.Count == 0) return -1;
            return Convert.ToInt32(dt.Rows[0]["MAX_GROUP_ID"]);
        }
    }
}
