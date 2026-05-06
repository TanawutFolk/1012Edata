using BusinessData.Property;
using EdataManagement.Models;
using EdataManagement.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdataManagement.Controllers
{
    public class SettingControllers
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        SettingModels models = new SettingModels();


        public bool updateLastData(SettingProperty dataItem)

        {

            int maxCus = 0;
            int max_key_send_methods_key_id = 0;
            int max_email_subject_key_id = 0;
            int max_email_body_key_id = 0;
            int max_email_attach_key_id = 0;
            int max_key_group_email_key_id = 0;

            try
            {
                resultData = models.maxCusID(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        maxCus = Convert.ToInt32(resultData.ResultOnDb.Rows[0]["max_id_cus"]);

                    }
                }

                resultData = models.maxKeyID(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        max_key_send_methods_key_id = Convert.ToInt32(resultData.ResultOnDb.Rows[0]["max_key_send_methods_key_id"]);
                        max_email_subject_key_id = Convert.ToInt32(resultData.ResultOnDb.Rows[0]["max_email_subject_key_id"]);
                        max_email_body_key_id = Convert.ToInt32(resultData.ResultOnDb.Rows[0]["max_email_body_key_id"]);
                        max_email_attach_key_id = Convert.ToInt32(resultData.ResultOnDb.Rows[0]["max_email_attach_key_id"]);
                        max_key_group_email_key_id = Convert.ToInt32(resultData.ResultOnDb.Rows[0]["max_key_group_email_key_id"]);
                    }
                }

                if (maxCus > max_key_send_methods_key_id)
                {

                    resultData = models.insertUpdateKeySendMethods(dataItem);


                }

                if (maxCus > max_email_subject_key_id)
                {

                    resultData = models.insertUpdateEmailSubject(dataItem);

                }

                if (maxCus > max_email_body_key_id)
                {

                    resultData = models.insertUpdateEmailBody(dataItem);

                }

                if (maxCus > max_email_attach_key_id)
                {

                    resultData = models.insertUpdateEmailAttach(dataItem);

                }

                if (maxCus > max_key_group_email_key_id)
                {

                    resultData = models.insertUpdateKeyGroupEmail(dataItem);

                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", (MessageBoxButtons)MessageBoxButtons.OK, (MessageBoxIcon)MessageBoxIcon.Error);
            }

            return resultData.StatusOnDb;

        }
    }
}
