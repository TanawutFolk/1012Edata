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
    public class ExcelControllers
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        ExcelModels _models = new ExcelModels();

        public ExcelProperty infoPOAllValue(ExcelProperty dataItem)
        {
            // DataTable dt = new DataTable();
            try
            {
                resultData = _models.infoPOallValue(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        dataItem.startRow = int.Parse(resultData.ResultOnDb.Rows[0]["START_ROW"].ToString());
                        dataItem.keyColumn = resultData.ResultOnDb.Rows[0]["KEY_COLUMN"].ToString();
                        dataItem.keyColumnName = resultData.ResultOnDb.Rows[0]["KEY_COLUMN_NAME"].ToString();
                        dataItem.valueColumn = resultData.ResultOnDb.Rows[0]["VALUE_COLUMN"].ToString();
                        dataItem.valueColumnName = resultData.ResultOnDb.Rows[0]["VALUE_COLUMN_NAME"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                // ตรวจสอบว่ามี InnerException หรือไม่
                if (ex.InnerException != null)
                {
                    errorMessage += "\nInner Exception: " + ex.InnerException.Message;
                }

                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dataItem;
        }
    }
}
