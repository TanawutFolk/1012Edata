using BusinessData.Property;
using EdataManagement.Models;

using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdataManagement.Controllers
{
    public class MainControllers
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        MainModels mainModels = new MainModels();

        public DataTable getProduct()
        {
            DataTable dt = new DataTable();
            try
            {
                resultData = mainModels.getProduct();
                if(resultData.StatusOnDb == true)
                {
                    if(resultData.ResultOnDb.Rows.Count > 0)
                    {
                        dt = resultData.ResultOnDb;
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

            return dt;
        }

        public void updateDataQA()
        {
            // search data production
            //
        }

    }
}
