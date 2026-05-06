using BusinessData.Property;

using Login.Property;
using Login.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Login.Controllors
{
    public class LoginControllors
    {
        LoginModels _models = new LoginModels();
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        EmpProperty _empProperty = new EmpProperty();
        EmailControllors _emailControllors = new EmailControllors();

        public bool EnterOTP(EmpProperty dataItem, string OTP)
        {
            bool _result = false;
            try
            {
                resultData = _models.SearchOTP(dataItem, OTP);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        //Check EXP Date.
                        string EXP_OTP = resultData.ResultOnDb.Rows[0]["EXP_OTP"].ToString();
                        DateTime oEXP_OTP = DateTime.Parse(EXP_OTP);
                        string dateTimeNow = this.GetDateTime();
                        if (dateTimeNow != "")
                        {
                            DateTime odateTimeNow = DateTime.Parse(dateTimeNow);
                            if (oEXP_OTP >= odateTimeNow)
                            {
                                if (this.UpdateOTPUsed(dataItem, OTP))
                                { _result = true; }
                            }
                            else
                            { MessageBox.Show("OTP has expired, please click 'Send again.'", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);}
                        }
                    }
                    else
                    {MessageBox.Show("You have entered an incorrect code. Please try again.", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);}
                }
                else
                {MessageBox.Show(resultData.MessageOnDb, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return _result;
        }

        public EmpProperty GetEmpProperty(EmpProperty dataItem)
        {
            EmpProperty _result = new EmpProperty();
            try
            {
                resultData = _models.SearchEmpCompany(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        _result = new EmpProperty
                        {
                            code = resultData.ResultOnDb.Rows[0]["EMP_ID"].ToString(),
                            name = resultData.ResultOnDb.Rows[0]["EMP_NAME"].ToString(),
                            sername = resultData.ResultOnDb.Rows[0]["EMP_SURNAME"].ToString(),
                            email = resultData.ResultOnDb.Rows[0]["EMP_EMAIL"].ToString(),
                            startwork = resultData.ResultOnDb.Rows[0]["START_WORK"].ToString(),
                            depertment = resultData.ResultOnDb.Rows[0]["DEPARTMENT"].ToString(),
                            position = resultData.ResultOnDb.Rows[0]["POSITION"].ToString()
                        };
                    }
                }
                else
                {
                    MessageBox.Show(resultData.MessageOnDb, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return _result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return _result;
            }
        }
        public string GetDateTime()
        {
            string _result = "";
            try
            {
                resultData = _models.GetDatatime();
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        string dateTimeNow = resultData.ResultOnDb.Rows[0]["DATETIME_NOW"].ToString();
                        _result = dateTimeNow;
                    }
                }
                else
                {
                    MessageBox.Show(resultData.MessageOnDb, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return _result;
        }
        public string GetOTP()
        {
            char[] chars = "1234567890".ToCharArray();
            byte[] data = new byte[6];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(6);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
        public string GetEXP_OTP(EmpProperty dataItem, string OTP)
        {
            string _result = "";
            try
            {
                resultData = _models.SearchOTP(dataItem, OTP);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        string EXP_OTP = resultData.ResultOnDb.Rows[0]["EXP_OTP"].ToString();
                        _result = EXP_OTP;
                    }
                }
                else
                {
                    MessageBox.Show(resultData.MessageOnDb, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return _result;
        }
        public string GetUniqueKey(int size)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public bool CheckEmpCompanyExist(EmpProperty dataItem)
        {
            bool _result = false;
            try
            {
                resultData = _models.SearchEmpCompany(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {
                        if (resultData.ResultOnDb.Rows[0]["EMP_STATUS"].ToString() == "1")
                        { _result = true; }
                    }
                    else
                    {MessageBox.Show("Couldn't find Employee ID", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);}
                }
                else
                {MessageBox.Show(resultData.MessageOnDb, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}

            return _result;
        }
        public bool CheckEmpFitelGroupExist(EmpProperty dataItem)
        {
            bool _result = false;
            try
            {
                resultData = _models.SearchEmpFitelGroup(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {_result = true;}
                }
                else
                { MessageBox.Show(resultData.MessageOnDb, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}
            }
            catch (Exception ex)
            {MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            return _result;
        }
        public bool CheckForEmail(string email)
        {

            bool IsValid = false;
            Regex r = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            if (r.IsMatch(email))
            {
                IsValid = true;
            }
            return IsValid;
        }
      

        public bool LoginFitelGroup(EmpProperty dataItem)
        {
            bool _retult = false;
            try
            {
                resultData = _models.LoginFitelGroup(dataItem);
                if (resultData.StatusOnDb == true)
                {
                    if (resultData.ResultOnDb.Rows.Count > 0)
                    {_retult = true;}
                }
                else
                {MessageBox.Show(resultData.MessageOnDb, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}
            }
            catch (Exception ex)
            {MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}

            return _retult;
        }

        public bool UpdateOTPUsed(EmpProperty dataItem, string OTP)
        {
            bool _result = false;
            try
            {
                resultData = _models.UpdateOTPUsed(dataItem, OTP);
                if (resultData.StatusOnDb == true)
                {_result = true; }
                else
                { MessageBox.Show(resultData.MessageOnDb, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}
            }
            catch (Exception ex)
            {MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}
            return _result;
        }

        public bool InsertLoginFitelGroup(EmpProperty dataItem)
        {
            bool _result = false;
            try
            {
                resultData = _models.InsertLoginFitelGroup(dataItem);
                if (resultData.StatusOnDb == true)
                {_result = true; }
                else {MessageBox.Show(resultData.MessageOnDb, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}
            }
            catch (Exception ex)
            {MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}
            return _result;
        }
        public bool InsertOTP(EmpProperty emp, string OTP)
        {
            bool _result = false;
            try
            {
                resultData = _models.InsertOTP(emp, OTP);
                if (resultData.StatusOnDb == true)
                {_result = true;}
                else
                {MessageBox.Show(resultData.MessageOnDb, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            return _result;
        }
        public bool SendOTP(EmpProperty dataItem)
        {
            bool _result = false;
            try
            {
                string OTP = this.GetOTP();
                if (this.InsertOTP(dataItem, OTP))
                {
                    string EXP = this.GetEXP_OTP(dataItem, OTP);
                    if (EXP != "")
                    {
                        string sendTo = dataItem.email;
                        string Subject = @"Program : Login";
                        string Body = @"OTP for Program Login is " + OTP + @" 
                                <br>EXP : " + EXP + @"
                                <br>Do not open this code to other people.";

                        if (_emailControllors.SentEmail(sendTo, Subject, Body))
                        {_result = true;}
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return _result;
        }

        public bool SendNewPassword(EmpProperty dataItem)
        {
            bool _result = false;
            try
            {
                string sendTo = dataItem.email;
                string Subject = @"Program : Login";
                string Body = "Your new password is : " + dataItem.pass;

                if (_emailControllors.SentEmail(sendTo, Subject, Body))
                { _result = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return _result;
        }

        public bool UpdatePassLoginFitelGroup(EmpProperty dataItem)
        {
            bool _result = false;
            try
            {
                resultData = _models.UpdatePassLoginFitelGroup(dataItem);
                if (resultData.StatusOnDb == true)
                {_result = true;}
                else
                { MessageBox.Show(resultData.MessageOnDb, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return _result;
        }

        




    }
}
