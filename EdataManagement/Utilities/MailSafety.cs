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
    /// Test groups are configured in App.config "TestMailGroupIds" (for example "0,1000").
    /// Group 1000 remains internal-only; Group 0 is explicitly allowed to include external
    /// recipients because it is the selected QA test group. Any recipient outside these groups
    /// stays a draft. Ignored on qa_oc.
    ///
    /// SFTP in test mode goes to the real server but only into TEST_SFTP_FOLDER (the old "for test"
    /// folder), never into a customer folder: ftpUserControl refuses any other folder.
    /// </summary>
    public static class MailSafety
    {
        public const string PRODUCTION_DATABASE = "qa_oc";
        public const string TEST_SUBJECT_PREFIX = "[TEST - NOT SENT] ";
        public const string INTERNAL_DOMAIN = "@furukawaelectric.com";
        public const string TEST_SFTP_FOLDER = "fftTest";

        /// <summary>A test-mode SFTP read / upload may only touch TEST_SFTP_FOLDER; production may go anywhere.</summary>
        public static bool SftpFolderAllowed(string remoteFolderPath)
        {
            return !IsTestMode ||
                   string.Equals((remoteFolderPath ?? "").Trim().Trim('/'), TEST_SFTP_FOLDER, StringComparison.OrdinalIgnoreCase);
        }

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

        /// <summary>Legacy single-group setting. Prefer TestMailGroupIds for new deployments.</summary>
        public static int? TestMailGroupId
        {
            get
            {
                int groupId;
                string value = ConfigurationManager.AppSettings["TestMailGroupId"];
                return int.TryParse(value?.Trim(), out groupId) ? groupId : (int?)null;
            }
        }

        public static IEnumerable<int> TestMailGroupIds
        {
            get
            {
                string value = ConfigurationManager.AppSettings["TestMailGroupIds"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    int? legacy = TestMailGroupId;
                    return legacy.HasValue ? new[] { legacy.Value } : Enumerable.Empty<int>();
                }

                return value.Split(',')
                    .Select(item =>
                    {
                        int groupId;
                        return int.TryParse(item.Trim(), out groupId) ? (int?)groupId : null;
                    })
                    .Where(item => item.HasValue)
                    .Select(item => item.Value)
                    .Distinct()
                    .ToArray();
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

        public static bool CanSendTestMail(IEnumerable<string> to, IEnumerable<string> cc,
                                           IEnumerable<string> internalGroupAddresses,
                                           IEnumerable<string> externalGroupAddresses)
        {
            List<string> recipients = (to ?? Enumerable.Empty<string>())
                .Concat(cc ?? Enumerable.Empty<string>())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => a.Trim())
                .ToList();

            HashSet<string> externalAllowed = new HashSet<string>(
                externalGroupAddresses ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            if (recipients.Count > 0 && recipients.All(externalAllowed.Contains))
            {
                return true;
            }

            return CanSendTestMail(recipients, Enumerable.Empty<string>(), internalGroupAddresses);
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
