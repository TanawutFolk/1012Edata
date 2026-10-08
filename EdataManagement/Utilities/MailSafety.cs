using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Net;

namespace EdataManagement.Utilities
{
    /// <summary>
    /// Guard against test mail reaching a customer.
    ///
    /// Every mail of 1012 leaves through EmailCls.SendEmail. When the program is NOT on the
    /// production config database (ConnectionQAOC -> qa_oc), SendEmail does not send at all:
    /// the mail is saved to the Outlook Drafts folder with NO recipient, the subject starts
    /// with TEST_SUBJECT_PREFIX and the recipients it would have gone to are listed at the top
    /// of the body for checking. Even pressing Send by mistake in Outlook cannot reach anyone.
    ///
    /// Fail-safe: when the database name cannot be read, it is treated as test mode.
    ///
    /// Test group (App.config appSetting "TestMailGroupId", e.g. 1000): in test mode a mail IS sent
    /// when every one of its recipients is a member of that info_group_email group and on
    /// INTERNAL_DOMAIN - i.e. the spec was pointed at the test group on purpose. That mail goes out
    /// exactly like a real one (same subject and body, nothing added). A mail with any other
    /// recipient stays a draft as above. Not set = never send. Ignored on qa_oc.
    /// </summary>
    public static class MailSafety
    {
        public const string PRODUCTION_DATABASE = "qa_oc";
        public const string TEST_SUBJECT_PREFIX = "[TEST - NOT SENT] ";
        public const string INTERNAL_DOMAIN = "@furukawaelectric.com";

        /// <summary>Database of ConnectionQAOC, empty when it cannot be read.</summary>
        public static string ConfigDatabase
        {
            get
            {
                try
                {
                    string connection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
                    DbConnectionStringBuilder builder = new DbConnectionStringBuilder { ConnectionString = connection };
                    return builder.ContainsKey("Database") ? builder["Database"].ToString().Trim() : "";
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>True unless the program is on the production config database.</summary>
        public static bool IsTestMode
        {
            get { return !string.Equals(ConfigDatabase, PRODUCTION_DATABASE, StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>App.config "TestMailGroupId", null when missing or not a number (= never send in test mode).</summary>
        public static int? TestMailGroupId
        {
            get
            {
                int groupId;
                string value = ConfigurationManager.AppSettings["TestMailGroupId"];
                return int.TryParse(value?.Trim(), out groupId) ? groupId : (int?)null;
            }
        }

        /// <summary>
        /// Test mode: may this mail really go out? Only when it has at least one recipient and every
        /// recipient is in testGroupAddresses and on INTERNAL_DOMAIN.
        /// </summary>
        public static bool CanSendTestMail(IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> testGroupAddresses)
        {
            HashSet<string> allowed = new HashSet<string>(
                (testGroupAddresses ?? Enumerable.Empty<string>())
                    .Select(a => a.Trim())
                    .Where(a => a.EndsWith(INTERNAL_DOMAIN, StringComparison.OrdinalIgnoreCase)),
                StringComparer.OrdinalIgnoreCase);

            List<string> recipients = (to ?? Enumerable.Empty<string>())
                .Concat(cc ?? Enumerable.Empty<string>())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => a.Trim())
                .ToList();

            return recipients.Count > 0 && recipients.All(allowed.Contains);
        }

        /// <summary>HTML block put at the top of a test mail: who it would have gone to.</summary>
        public static string IntendedRecipientsHtml(IEnumerable<string> to, IEnumerable<string> cc)
        {
            string toText = string.Join(", ", (to ?? Enumerable.Empty<string>()).Where(a => !string.IsNullOrWhiteSpace(a)).ToArray());
            string ccText = string.Join(", ", (cc ?? Enumerable.Empty<string>()).Where(a => !string.IsNullOrWhiteSpace(a)).ToArray());

            return "<div style=\"border:2px solid #b91c1c;padding:8px;margin-bottom:12px;color:#b91c1c;font-family:Segoe UI,Arial\">" +
                   "<b>TEST MODE (DB: " + WebUtility.HtmlEncode(ConfigDatabase) + ") - this mail was NOT sent and has no recipient.</b><br>" +
                   "It would have gone to:<br>" +
                   "<b>TO:</b> " + WebUtility.HtmlEncode(toText.Length == 0 ? "-" : toText) + "<br>" +
                   "<b>CC:</b> " + WebUtility.HtmlEncode(ccText.Length == 0 ? "-" : ccText) +
                   "</div>";
        }
    }
}
