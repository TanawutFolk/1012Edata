using EdataManagement.Property;

using System;
using System.Text.RegularExpressions;

namespace EdataManagement.SQLFactory
{
    /// <summary>
    /// SQL for the Spec Setting page (ucSpecSetting): how every spec of a customer is sent.
    /// One row per spec (KEY_ID = info_cus_condition.CusCondition_ID) in each of the 5 key tables of a product:
    ///   key_send_methods_&lt;p&gt;.SEND_METHODS_ID   email_attach_&lt;p&gt;.FILE_FORMATS_ID   key_group_email_&lt;p&gt;.GROUP_ID
    ///   email_subject_&lt;p&gt;.SUBJECT_ID          email_body_&lt;p&gt;.BODY_ID
    /// Config tables live in the connection database (App.config ConnectionQAOC),
    /// the customer / spec rows in the product database (dataItem.dbProduct, e.g. ddb) - read only.
    /// Recipient groups and members are in SettingEmailSQLFactory.
    /// </summary>
    public class SettingSpecSQLFactory
    {
        private string sql;

        /// <summary>Escape a single quote so a customer name like O'Brien does not break the statement.</summary>
        private string Esc(string value)
        {
            if (value == null) return "";
            return value.Replace("\\", "\\\\").Replace("'", "''");
        }

        /// <summary>Product / database names become part of table names, so only plain names get through.</summary>
        private static string Name(string value)
        {
            if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, "^[A-Za-z0-9_]+$"))
            {
                throw new ArgumentException("Invalid product / database name: '" + value + "'");
            }
            return value;
        }

        /// <summary>Key table of one field (the grid column names of ucSpecSetting).</summary>
        public static string KeyTableOf(string field)
        {
            switch (field)
            {
                case "SEND_METHODS_ID": return "key_send_methods_";
                case "FILE_FORMATS_ID": return "email_attach_";
                case "GROUP_ID": return "key_group_email_";
                case "SUBJECT_ID": return "email_subject_";
                case "BODY_ID": return "email_body_";
                default: throw new ArgumentException("Unknown spec field: " + field);
            }
        }

        /// <summary>Every customer of a product with an in-use spec, and how many of those go by Email.</summary>
        public string searchCustomers(SettingSpecProperty dataItem)
        {
            sql = "SELECT ic.Customer AS `CUSTOMER_NAME` " +
                  ", COUNT(*) AS `TOTAL_SPECS` " +
                  ", SUM(CASE WHEN k.SEND_METHODS_ID = 3 THEN 1 ELSE 0 END) AS `EMAIL_SPECS` " +
                  "FROM `" + Name(dataItem.dbProduct) + "`.info_cus_condition ic " +
                  "LEFT JOIN key_send_methods_" + Name(dataItem.productName) + " k ON (k.KEY_ID = ic.CusCondition_ID) " +
                  "WHERE ic.Inuse = 1 " +
                  "GROUP BY ic.Customer " +
                  "ORDER BY ic.Customer ASC ";

            return sql;
        }

        /// <summary>
        /// The in-use specs of one customer with the value of each key table.
        /// A spec without a row yet (added after the start-up sync) reads 0; Save inserts the row.
        /// </summary>
        public string searchSpecs(SettingSpecProperty dataItem)
        {
            string p = Name(dataItem.productName);

            sql = "SELECT ic.CusCondition_ID AS `KEY_ID` " +
                  ", ic.Specification AS `SPECIFICATION` " +
                  ", ic.Part_No AS `PART_NO` " +
                  ", IFNULL(ic.Customer_Part_No, '') AS `CUSTOMER_PN` " +
                  ", IFNULL(s.SEND_METHODS_ID, 0) AS `SEND_METHODS_ID` " +
                  ", IFNULL(a.FILE_FORMATS_ID, 0) AS `FILE_FORMATS_ID` " +
                  ", IFNULL(g.GROUP_ID, 0) AS `GROUP_ID` " +
                  ", IFNULL(sj.SUBJECT_ID, 0) AS `SUBJECT_ID` " +
                  ", IFNULL(b.BODY_ID, 0) AS `BODY_ID` " +
                  "FROM `" + Name(dataItem.dbProduct) + "`.info_cus_condition ic " +
                  "LEFT JOIN key_send_methods_" + p + " s ON (s.KEY_ID = ic.CusCondition_ID) " +
                  "LEFT JOIN email_attach_" + p + " a ON (a.KEY_ID = ic.CusCondition_ID) " +
                  "LEFT JOIN key_group_email_" + p + " g ON (g.KEY_ID = ic.CusCondition_ID) " +
                  "LEFT JOIN email_subject_" + p + " sj ON (sj.KEY_ID = ic.CusCondition_ID) " +
                  "LEFT JOIN email_body_" + p + " b ON (b.KEY_ID = ic.CusCondition_ID) " +
                  "WHERE ic.Inuse = 1 AND ic.Customer = '" + Esc(dataItem.customerName) + "' " +
                  "ORDER BY ic.Specification ASC, ic.Part_No ASC ";

            return sql;
        }

        // ---- the lists of the editor ----

        public string searchSendMethods()
        {
            sql = "SELECT ID, SEND_METHODS_NAME AS `NAME` FROM info_send_methods ORDER BY ID ASC ";
            return sql;
        }

        public string searchFileFormats()
        {
            sql = "SELECT ID, FILE_FORMATS AS `NAME`, DETAILS FROM info_file_formats ORDER BY ID ASC ";
            return sql;
        }

        public string searchSubjects()
        {
            sql = "SELECT SUBJECT_ID AS `ID`, SUBJECT_DETAIL AS `NAME`, INUSE FROM info_email_subject ORDER BY SUBJECT_ID ASC ";
            return sql;
        }

        public string searchBodies()
        {
            sql = "SELECT BODY_ID AS `ID`, BODY_NAME AS `NAME`, INUSE FROM info_email_body ORDER BY BODY_ID ASC ";
            return sql;
        }

        /// <summary>Recipient groups a spec can use - not the Serial List / Wafer groups of key_group_email_document.</summary>
        public string searchSpecGroups()
        {
            sql = "SELECT ge.GROUP_ID AS `ID`, COUNT(*) AS `MEMBERS` " +
                  "FROM info_group_email ge " +
                  "WHERE ge.GROUP_ID NOT IN (SELECT d.GROUP_ID FROM key_group_email_document d) " +
                  "GROUP BY ge.GROUP_ID " +
                  "ORDER BY ge.GROUP_ID ASC ";

            return sql;
        }

        /// <summary>
        /// Highest group id in use - members (info_group_email) or a Serial List / Wafer group
        /// that has no member yet (key_group_email_document). A new group takes the next one.
        /// </summary>
        public string maxGroupId()
        {
            sql = "SELECT GREATEST( " +
                  "IFNULL((SELECT MAX(GROUP_ID) FROM info_group_email), -1), " +
                  "IFNULL((SELECT MAX(GROUP_ID) FROM key_group_email_document), -1) ) AS `MAX_GROUP_ID` ";

            return sql;
        }

        // ---------------------------------------------------------------
        // write statements - run in one transaction with SET @app_user first,
        // the audit triggers of the key tables fill UPDATE_BY / CREATE_BY and log_setting_edata
        // ---------------------------------------------------------------

        /// <summary>Set one field of one spec; inserts the row when the spec has none yet.</summary>
        public string saveSpecValue(string productName, string field, int keyId, int value)
        {
            sql = "INSERT INTO " + KeyTableOf(field) + Name(productName) + " (KEY_ID, " + field + ") " +
                  "VALUES (" + keyId + ", " + value + ") " +
                  "ON DUPLICATE KEY UPDATE " + field + " = VALUES(" + field + ") ";

            return sql;
        }

        /// <summary>
        /// info_group_email has no trigger (other programs write it too), so recipient changes are logged here:
        /// KEY_ID = group, SPEC = address, OLD / NEW = 1 TO, 2 CC, NULL = not in the group.
        /// </summary>
        public string logRecipientChange(string productName, int groupId, string customerName, string address,
            int? oldType, int? newType, string user)
        {
            sql = "INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_DATE) " +
                  "VALUES ('" + Esc(productName) + "', 'info_group_email', " + groupId + ", LEFT('" + Esc(customerName) + "', 100), " +
                  "LEFT('" + Esc(address) + "', 50), " + (oldType.HasValue ? oldType.Value.ToString() : "NULL") + ", " +
                  (newType.HasValue ? newType.Value.ToString() : "NULL") + ", LEFT('" + Esc(user) + "', 20), NOW()) ";

            return sql;
        }

        /// <summary>A group made by New Group (empty at first; each address it gets has its own log row).</summary>
        public string logNewGroup(string productName, int newGroupId, string customerName, string user)
        {
            sql = "INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_DATE) " +
                  "VALUES ('" + Esc(productName) + "', 'info_group_email', " + newGroupId + ", LEFT('" + Esc(customerName) + "', 100), " +
                  "'new group', NULL, " + newGroupId + ", LEFT('" + Esc(user) + "', 20), NOW()) ";

            return sql;
        }
    }
}
