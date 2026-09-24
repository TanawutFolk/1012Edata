using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Web.Script.Serialization;

namespace EdataManagement.Controllers
{
    /// <summary>Result of a Security Center sign in.</summary>
    public class SignInResultProperty
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Security Center API (portal system) - the same call RawMat and 1st Lot use.
    /// No password is stored in this application; the portal password is checked here.
    /// URL comes from App.config key SecurityCenterUrl.
    ///
    /// Currently the call is NOT used: frmLogin has the password step commented out
    /// while the screen is being tested. Turn it back on there when it goes live.
    /// </summary>
    public class SecurityCenterControllers
    {
        private const string DEFAULT_BASE_URL = "http://192.168.14.235:7010";

        private static string BaseUrl
        {
            get
            {
                string url = ConfigurationManager.AppSettings["SecurityCenterUrl"];
                return string.IsNullOrWhiteSpace(url) ? DEFAULT_BASE_URL : url.TrimEnd('/');
            }
        }

        public SignInResultProperty SignIn(string userName, string password)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    JavaScriptSerializer serializer = new JavaScriptSerializer();

                    // the API takes JSON in the query string - serialize + encode so that
                    // a user or password with special characters does not break the request
                    string data = serializer.Serialize(new Dictionary<string, string>
                    {
                        { "USER_NAME", userName },
                        { "USER_PASSWORD", password }
                    });

                    string url = BaseUrl + "/api/security-center/SignIn?data=" + Uri.EscapeDataString(data);

                    HttpResponseMessage response = client.GetAsync(url).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        return new SignInResultProperty
                        {
                            Status = false,
                            Message = response.ReasonPhrase
                        };
                    }

                    string body = response.Content.ReadAsStringAsync().Result;
                    Dictionary<string, object> json = serializer.Deserialize<Dictionary<string, object>>(body);

                    return new SignInResultProperty
                    {
                        Status = ToBool(GetValue(json, "Status")),
                        Message = Convert.ToString(GetValue(json, "Message"))
                    };
                }
            }
            catch (Exception ex)
            {
                return new SignInResultProperty
                {
                    Status = false,
                    Message = ex.GetBaseException().Message
                };
            }
        }

        private static object GetValue(Dictionary<string, object> json, string key)
        {
            if (json == null) return null;

            foreach (KeyValuePair<string, object> item in json)
            {
                if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Value;
                }
            }
            return null;
        }

        /// <summary>The API can answer Status as true, "true", 1 or "1".</summary>
        private static bool ToBool(object value)
        {
            if (value == null) return false;
            if (value is bool) return (bool)value;

            string text = Convert.ToString(value).Trim();
            if (text.Length == 0) return false;

            bool parsed;
            if (bool.TryParse(text, out parsed)) return parsed;

            int number;
            if (int.TryParse(text, out number)) return number != 0;

            return false;
        }
    }
}
