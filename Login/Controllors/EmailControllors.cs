using BusinessData.Property;

using Login.Property;
using Login.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Login.Controllors
{
    public class EmailControllors
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();

        private string server;
        private string host;
        private string emailuser;
        private string pass;

        public EmailControllors()
        {
            IniFile IniFile = new IniFile("System.Settings.ini");
            server = IniFile.Read("smtserver", "SendEmail");
            host = IniFile.Read("smtport", "SendEmail");
            emailuser = IniFile.Read("emailUser", "SendEmail");
            pass = IniFile.Read("emailpass", "SendEmail");
        }

        public bool SentEmail(string sendTo,string subJect,string body)
        {
            bool _result = false;
            try
            {
                if (CheckInput(sendTo, subJect, body))
                {
                    SmtpClient client = new SmtpClient
                    {
                        Port = Int32.Parse(host),
                        Host = server,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new System.Net.NetworkCredential(emailuser, pass)
                    };

                    MailMessage objeto_mail = new MailMessage
                    {
                        From = new MailAddress(emailuser),
                        Subject = subJect,
                        Body = body,
                        IsBodyHtml = true
                    };

                    objeto_mail.To.Add(new MailAddress(sendTo));
                    client.Send(objeto_mail);
                    client.Dispose();
                    objeto_mail.Dispose();

                    _result = true;
                }
                else
                {
                    _result = false;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _result = false;
            }
            return _result; 
        }

        public bool CheckInput(string sendTo, string subJect, string body)
        {
            if (sendTo == "")
            {
                MessageBox.Show("", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
            if (subJect == "")
            {
                MessageBox.Show("", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            if (body == "")
            {
                MessageBox.Show("", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            return true;
        }
    }
}
