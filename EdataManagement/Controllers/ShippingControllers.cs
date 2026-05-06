using BusinessData.Property;

using EdataManagement.Property;
using EdataManagement.Models;
using EdataManagement.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Data;
using MessageBox = System.Windows.Forms.MessageBox;

namespace EdataManagement.Controllers
{
    public class ShippingControllers
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        ShippingModels models = new ShippingModels();

        public DataTable SearchDataShipping(ShippingProperty dataItem)
        {
            DataTable _result = new DataTable();
            //ShippingProperty _resultShipping = new ShippingProperty();

            //_resultShipping.shipdate = dataItem.shipdate;
            try
            {
                resultData = models.SearchCusPartNo(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        _result = resultData.ResultOnDb;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }
            return _result;
        }


        public DataTable searchCustomerShipping(ShippingProperty dataItem)
        {
            DataTable _result = new DataTable();
            try
            {
                resultData = models.searchCustomerShipping(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        _result = resultData.ResultOnDb;

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
            return _result;
        }

        public DataTable SearchSerialShipping(ShippingProperty dataItem)
        {
            DataTable _result = new DataTable();
            //ShippingProperty _resultShipping = new ShippingProperty();

            //_resultShipping.shipdate = dataItem.shipdate;
            try
            {
                resultData = models.SearchSerialShipping(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        _result = resultData.ResultOnDb;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }
            return _result;
        }

        public DataTable SearchSerialShippingCust(ShippingProperty dataItem)
        {
            DataTable _result = new DataTable();
            //ShippingProperty _resultShipping = new ShippingProperty();

            //_resultShipping.shipdate = dataItem.shipdate;
            try
            {
                resultData = models.SearchSerialShippingCust(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        _result = resultData.ResultOnDb;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }
            return _result;
        }

        public string getSendMethods(ShippingProperty dataItem)
        {
            //DataTable _result = new DataTable();
            //ShippingProperty _resultShipping = new ShippingProperty();

            //_resultShipping.shipdate = dataItem.shipdate;

            string _getDelv = "";

            try
            {
                resultData = models.getSendMethods(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        _getDelv = resultData.ResultOnDb.Rows[0]["SEND_METHODS_ID"].ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }
            return _getDelv;
        }

    }
}
