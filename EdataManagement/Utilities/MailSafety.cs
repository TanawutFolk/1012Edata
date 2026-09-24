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
    /// </summary>
    public static class MailSafety
    {
        public const string PRODUCTION_DATABASE = "qa_oc";
        public const string TEST_SUBJECT_PREFIX = "[TEST - NOT SENT] ";

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
