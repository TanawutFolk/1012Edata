using BusinessData.Property;
using EdataManagement.Models;
using EdataManagement.Property;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdataManagement.Controllers
{
    public class EmailControllers
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        EmailModels models = new EmailModels();
        
        public string getSubject(EmailProperty dataItem)
        {
            string _result = "";
            try
            {
                resultData = models.getSubject(dataItem);

                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        _result = resultData.ResultOnDb.Rows[0]["SUBJECT_DETAIL"].ToString();

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }

            return _result;
        }

        public string getBody(EmailProperty dataItem)
        {
            string _result = "";
            try
            {
                resultData = models.getBody(dataItem);

                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        _result = resultData.ResultOnDb.Rows[0]["body"].ToString();

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }

            return _result;
        }

        public int getFileFormats(EmailProperty dataItem)
        {
            int _result = 0;
            try
            {
                resultData = models.getFileFormats(dataItem);

                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        _result = Convert.ToInt32(resultData.ResultOnDb.Rows[0]["FILE_FORMATS_ID"]);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }

            return _result;
        }

        public string getSignature(EmailProperty dataItem)
        {
            string _result = "";
            try
            {
                resultData = models.getSignature(dataItem);

                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        _result = resultData.ResultOnDb.Rows[0]["Signature"].ToString();

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }

            return _result;
        }


        public DataTable allFormatDate()
        {
            DataTable dt =new DataTable();
            try
            {
                resultData = models.allFormatDate();

                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        dt = resultData.ResultOnDb;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }

            return dt;
        }
        
        public EmailProperty getEmailAddress(EmailProperty dataItem)
        {
            //DataTable dt = new DataTable();
            dataItem.toEmailAddresses = new List<string>();
            dataItem.ccEmailAddresses = new List<string>();
            try
            {
                resultData = models.getEmailAddress(dataItem);

                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        for(int i =0;i< resultData.ResultOnDb.Rows.Count;i++)
                        {          
                            if (resultData.ResultOnDb.Rows[i]["recipient_type"].ToString() == "to")
                            {
                                dataItem.toEmailAddresses.Add(resultData.ResultOnDb.Rows[i]["EMAIL_ADDRESS"].ToString());
                            }
                            else if (resultData.ResultOnDb.Rows[i]["recipient_type"].ToString() == "cc")
                            {    
                                dataItem.ccEmailAddresses.Add(resultData.ResultOnDb.Rows[i]["EMAIL_ADDRESS"].ToString());
                            }
                            else
                            { 
                                dataItem = new EmailProperty();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }
            return dataItem;
        }

        /// <summary>
        /// TO or CC addresses of one E-data type (2 Serial List, 3 Wafer) of one product + customer.
        /// Returns null when the database has no recipient for it yet, so the caller can still
        /// fall back to Setting\EmailSetting.ini.
        /// </summary>
        public List<string> getDocumentEmailAddresses(string productName, string customerName, int documentId, string recipientType)
        {
            try
            {
                resultData = models.getDocumentEmailAddress(productName, customerName, documentId);

                if (resultData.StatusOnDb != true || resultData.ResultOnDb == null || resultData.ResultOnDb.Rows.Count == 0)
                {
                    return null;
                }

                List<string> addresses = new List<string>();
                foreach (DataRow row in resultData.ResultOnDb.Rows)
                {
                    if (string.Equals(row["recipient_type"].ToString(), recipientType, StringComparison.OrdinalIgnoreCase))
                    {
                        addresses.Add(row["EMAIL_ADDRESS"].ToString());
                    }
                }
                return addresses;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
                return null;
            }
        }

    }
}
