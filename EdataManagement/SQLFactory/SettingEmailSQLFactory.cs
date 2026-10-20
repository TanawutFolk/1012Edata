using EdataManagement.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.SQLFactory
{
    /// <summary>
    /// SQL for the Setting screen (frmSetting) - email recipient / group management.
    /// Config tables live in the connection database (qa_oc_test).
    /// Customer / spec rows live in the product database (dataItem.dbProduct, e.g. ddb).
    /// </summary>
    public class SettingEmailSQLFactory
    {
        private string sql;

        /// <summary>Escape a single quote so a customer name like O'Brien does not break the statement.</summary>
        private string Esc(string value)
        {
            if (value == null) return "";
            return value.Replace("\\", "\\\\").Replace("'", "''");
        }

        /// <summary>
        /// Customers of one product that really send e-data by email
        /// (at least one spec with SEND_METHODS_ID = 3).
        /// The table also holds old customers that are all Don't Send - 299 rows on 1480
        /// against 18 that matter here - so they are filtered out by the HAVING clause.
        /// </summary>
        public string searchCustomers(SettingEmailProperty dataItem)
        {
            sql = "SELECT ic.Customer AS `CUSTOMER_NAME` " +
                  ", COUNT(*) AS `TOTAL_SPECS` " +
                  ", SUM(CASE WHEN k.SEND_METHODS_ID = 3 THEN 1 ELSE 0 END) AS `EMAIL_SPECS` " +
                  "FROM " + dataItem.dbProduct + ".info_cus_condition ic " +
                  "INNER JOIN key_send_methods_" + dataItem.productName + " k ON (k.KEY_ID = ic.CusCondition_ID) " +
                  "WHERE ic.Inuse = 1 " +
                  "GROUP BY ic.Customer " +
                  "HAVING `EMAIL_SPECS` > 0 " +
                  "ORDER BY `EMAIL_SPECS` DESC, ic.Customer ASC ";

            return sql;
        }

        /// <summary>
        /// Group(s) used by the Email specs of one customer.
        /// More than one row back = specs of this customer point at different groups.
        /// </summary>
        public string searchGroupOfCustomer(SettingEmailProperty dataItem)
        {
            sql = "SELECT g.GROUP_ID, COUNT(*) AS `SPEC_COUNT` " +
                  "FROM " + dataItem.dbProduct + ".info_cus_condition ic " +
                  "INNER JOIN key_send_methods_" + dataItem.productName + " k ON (k.KEY_ID = ic.CusCondition_ID) " +
                  "INNER JOIN key_group_email_" + dataItem.productName + " g ON (g.KEY_ID = ic.CusCondition_ID) " +
                  "WHERE ic.Inuse = 1 AND ic.Customer = '" + Esc(dataItem.customerName) + "' " +
                  "AND k.SEND_METHODS_ID = 3 " +
                  "GROUP BY g.GROUP_ID " +
                  "ORDER BY `SPEC_COUNT` DESC ";

            return sql;
        }

        /// <summary>
        /// E-data types for cbEdataType: LD data (Edata) / Serial List / Wafer / ... from info_document.
        /// Every row is listed - INUSE is stale there (Wafer is 0 but is still sent).
        /// </summary>
        public string searchEdataTypes()
        {
            sql = "SELECT DOCUMENT_ID AS `ID`, DOCUMENT_NAME AS `NAME`, IN_EMAIL_NAME, INUSE " +
                  "FROM info_document " +
                  "ORDER BY DOCUMENT_ID ASC ";

            return sql;
        }

        /// <summary>
        /// E-data types configured for one product + customer. Edata (ID 1) is available
        /// through the customer's Email specs; other types need a bound document group.
        /// </summary>
        public string searchConfiguredEdataTypes(SettingEmailProperty dataItem)
        {
            sql = "SELECT d.DOCUMENT_ID AS `ID`, d.DOCUMENT_NAME AS `NAME`, d.IN_EMAIL_NAME, d.INUSE " +
                  "FROM info_document d " +
                  "WHERE d.DOCUMENT_ID = 1 " +
                  "OR EXISTS (" +
                  "SELECT 1 FROM key_group_email_document g " +
                  "WHERE g.PRODUCT_NAME = '" + Esc(dataItem.productName) + "' " +
                  "AND g.CUSTOMER = '" + Esc(dataItem.customerName) + "' " +
                  "AND g.DOCUMENT_ID = d.DOCUMENT_ID) " +
                  "ORDER BY d.DOCUMENT_ID ASC ";

            return sql;
        }

        /// <summary>
        /// Group of a non-LD E-data type (Serial List / Wafer) of one product + customer.
        /// No row = that type has no recipients in the database yet.
        /// </summary>
        public string searchDocumentGroup(SettingEmailProperty dataItem)
        {
            sql = "SELECT GROUP_ID, UPDATE_USER, UPDATE_DATE " +
                  "FROM key_group_email_document " +
                  "WHERE PRODUCT_NAME = '" + Esc(dataItem.productName) + "' " +
                  "AND CUSTOMER = '" + Esc(dataItem.customerName) + "' " +
                  "AND DOCUMENT_ID = " + dataItem.documentId + " ";

            return sql;
        }

        /// <summary>Bind a new (empty) group to a product + customer + E-data type, with its log row.</summary>
        public string insertDocumentGroup(SettingEmailProperty dataItem)
        {
            sql = "INSERT INTO key_group_email_document (PRODUCT_NAME, CUSTOMER, DOCUMENT_ID, GROUP_ID, UPDATE_USER, UPDATE_DATE) " +
                  "VALUES ('" + Esc(dataItem.productName) + "', '" + Esc(dataItem.customerName) + "', " + dataItem.documentId + ", " +
                  dataItem.newGroupId + ", '" + Esc(dataItem.createUser) + "', NOW()); " +

                  "INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_DATE) " +
                  "VALUES ('" + Esc(dataItem.productName) + "', 'key_group_email_document', " + dataItem.documentId + ", '" +
                  Esc(dataItem.customerName) + "', 'new group', NULL, " + dataItem.newGroupId + ", '" + Esc(dataItem.createUser) + "', NOW()) ";

            return sql;
        }

        /// <summary>
        /// Everyone else whose mail comes from the same GROUP_ID (the reason for the alert line):
        /// the Email specs of every product in dataItem.products (key tables are per product, customers live in
        /// each product's own database) plus the Serial List / Wafer groups in key_group_email_document.
        /// The mail being edited (dataItem product + customer + documentId) is left out.
        /// Strings are converted to one collation because the product databases do not share one.
        /// </summary>
        public string searchCustomersSharingGroup(SettingEmailProperty dataItem)
        {
            List<string> parts = new List<string>();
            List<SettingEmailProperty> products = dataItem.products ?? new List<SettingEmailProperty> { dataItem };

            foreach (SettingEmailProperty product in products)
            {
                if (string.IsNullOrEmpty(product.productName) || string.IsNullOrEmpty(product.dbProduct)) continue;

                parts.Add(
                    "SELECT CONVERT('" + Esc(product.productName) + "' USING utf8) COLLATE utf8_general_ci AS `PRODUCT` " +
                    ", CONVERT(ic.Customer USING utf8) COLLATE utf8_general_ci AS `CUSTOMER_NAME` " +
                    ", 1 AS `DOCUMENT_ID`, CONVERT('' USING utf8) COLLATE utf8_general_ci AS `DOCUMENT_NAME` " +
                    "FROM `" + product.dbProduct + "`.info_cus_condition ic " +
                    "INNER JOIN key_send_methods_" + product.productName + " k ON (k.KEY_ID = ic.CusCondition_ID) " +
                    "INNER JOIN key_group_email_" + product.productName + " g ON (g.KEY_ID = ic.CusCondition_ID) " +
                    "WHERE ic.Inuse = 1 AND k.SEND_METHODS_ID = 3 AND g.GROUP_ID = " + dataItem.groupId + " " +
                    "GROUP BY ic.Customer ");
            }

            parts.Add(
                "SELECT CONVERT(d.PRODUCT_NAME USING utf8) COLLATE utf8_general_ci AS `PRODUCT` " +
                ", CONVERT(d.CUSTOMER USING utf8) COLLATE utf8_general_ci AS `CUSTOMER_NAME` " +
                ", d.DOCUMENT_ID AS `DOCUMENT_ID` " +
                ", CONVERT(IFNULL(doc.DOCUMENT_NAME, CONCAT('Document ', d.DOCUMENT_ID)) USING utf8) COLLATE utf8_general_ci AS `DOCUMENT_NAME` " +
                "FROM key_group_email_document d " +
                "LEFT JOIN info_document doc ON (doc.DOCUMENT_ID = d.DOCUMENT_ID) " +
                "WHERE d.GROUP_ID = " + dataItem.groupId + " ");

            sql = "SELECT x.`PRODUCT`, x.`CUSTOMER_NAME`, x.`DOCUMENT_ID`, x.`DOCUMENT_NAME` " +
                  "FROM ( " + string.Join(" UNION ALL ", parts.ToArray()) + " ) x " +
                  "WHERE NOT (x.`PRODUCT` = '" + Esc(dataItem.productName) + "' " +
                  "AND x.`CUSTOMER_NAME` = '" + Esc(dataItem.customerName) + "' " +
                  "AND x.`DOCUMENT_ID` = " + dataItem.documentId + ") " +
                  "ORDER BY (x.`PRODUCT` = '" + Esc(dataItem.productName) + "') DESC, x.`PRODUCT` ASC, " +
                  "x.`DOCUMENT_ID` ASC, x.`CUSTOMER_NAME` ASC ";

            return sql;
        }

        /// <summary>Members of one group - what the grid shows.</summary>
        public string searchGroupMembers(SettingEmailProperty dataItem)
        {
            sql = "SELECT ge.EMAIL_ID, e.EMAIL_ADDRESS, ge.RECIPIENT_ID " +
                  ", CASE WHEN ge.RECIPIENT_ID = 1 THEN 'TO' WHEN ge.RECIPIENT_ID = 2 THEN 'CC' ELSE 'UNKNOWN' END AS `RECIPIENT_TYPE` " +
                  ", e.INUSE " +
                  "FROM info_group_email ge " +
                  "INNER JOIN info_email e ON (e.EMAIL_ID = ge.EMAIL_ID) " +
                  "WHERE ge.GROUP_ID = " + dataItem.groupId + " " +
                  "ORDER BY ge.RECIPIENT_ID ASC, e.EMAIL_ADDRESS ASC ";

            return sql;
        }

        /// <summary>All groups with member counts - for the "All Group Details" button.</summary>
        public string searchAllGroups()
        {
            sql = "SELECT ge.GROUP_ID " +
                  ", COUNT(*) AS `MEMBER_COUNT` " +
                  ", SUM(CASE WHEN ge.RECIPIENT_ID = 1 THEN 1 ELSE 0 END) AS `TO_COUNT` " +
                  ", SUM(CASE WHEN ge.RECIPIENT_ID = 2 THEN 1 ELSE 0 END) AS `CC_COUNT` " +
                  "FROM info_group_email ge " +
                  "GROUP BY ge.GROUP_ID " +
                  "ORDER BY ge.GROUP_ID ASC ";

            return sql;
        }

        /// <summary>Customers bound to every group (joined in memory with searchAllGroups).</summary>
        public string searchGroupUsage(SettingEmailProperty dataItem)
        {
            sql = "SELECT g.GROUP_ID, ic.Customer AS `CUSTOMER_NAME`, COUNT(*) AS `SPEC_COUNT` " +
                  "FROM " + dataItem.dbProduct + ".info_cus_condition ic " +
                  "INNER JOIN key_send_methods_" + dataItem.productName + " k ON (k.KEY_ID = ic.CusCondition_ID) " +
                  "INNER JOIN key_group_email_" + dataItem.productName + " g ON (g.KEY_ID = ic.CusCondition_ID) " +
                  "WHERE ic.Inuse = 1 AND k.SEND_METHODS_ID = 3 " +
                  "GROUP BY g.GROUP_ID, ic.Customer " +
                  "ORDER BY g.GROUP_ID ASC, ic.Customer ASC ";

            return sql;
        }

        /// <summary>
        /// One row per product + customer: which group it uses and who is on it.
        /// The key tables are per product (key_group_email_&lt;product&gt;) and the customer rows
        /// live in each product's own database, so the products are stitched together with UNION ALL.
        /// </summary>
        public string searchGroupMatrix(List<SettingEmailProperty> products)
        {
            List<string> parts = new List<string>();

            foreach (SettingEmailProperty product in products)
            {
                if (string.IsNullOrEmpty(product.productName) || string.IsNullOrEmpty(product.dbProduct))
                {
                    continue;
                }

                parts.Add(
                    "SELECT '" + Esc(product.productName) + "' AS `PRODUCT` " +
                    ", ic.Customer AS `CUSTOMER_NAME` " +
                    ", g.GROUP_ID " +
                    ", COUNT(*) AS `SPEC_COUNT` " +
                    "FROM `" + product.dbProduct + "`.info_cus_condition ic " +
                    "INNER JOIN key_send_methods_" + product.productName + " k ON (k.KEY_ID = ic.CusCondition_ID) " +
                    "INNER JOIN key_group_email_" + product.productName + " g ON (g.KEY_ID = ic.CusCondition_ID) " +
                    "WHERE ic.Inuse = 1 AND k.SEND_METHODS_ID = 3 " +
                    "GROUP BY ic.Customer, g.GROUP_ID ");
            }

            if (parts.Count == 0)
            {
                return "SELECT '' AS `PRODUCT`, '' AS `CUSTOMER_NAME`, 0 AS `GROUP_ID`, 0 AS `SPEC_COUNT` " +
                       ", '' AS `TO_LIST`, '' AS `CC_LIST`, 0 AS `TO_COUNT`, 0 AS `CC_COUNT` FROM DUAL WHERE 1 = 0 ";
            }

            sql = "SELECT x.`PRODUCT`, x.`CUSTOMER_NAME`, x.GROUP_ID, x.`SPEC_COUNT` " +
                  ", IFNULL(m.`TO_COUNT`, 0) AS `TO_COUNT` " +
                  ", IFNULL(m.`CC_COUNT`, 0) AS `CC_COUNT` " +
                  ", IFNULL(m.`TO_LIST`, '-') AS `TO_LIST` " +
                  ", IFNULL(m.`CC_LIST`, '-') AS `CC_LIST` " +
                  "FROM ( " + string.Join(" UNION ALL ", parts.ToArray()) + " ) x " +
                  "LEFT JOIN ( " +
                  "   SELECT ge.GROUP_ID " +
                  "   , GROUP_CONCAT(CASE WHEN ge.RECIPIENT_ID = 1 THEN e.EMAIL_ADDRESS END ORDER BY e.EMAIL_ADDRESS SEPARATOR ', ') AS `TO_LIST` " +
                  "   , GROUP_CONCAT(CASE WHEN ge.RECIPIENT_ID = 2 THEN e.EMAIL_ADDRESS END ORDER BY e.EMAIL_ADDRESS SEPARATOR ', ') AS `CC_LIST` " +
                  "   , SUM(CASE WHEN ge.RECIPIENT_ID = 1 THEN 1 ELSE 0 END) AS `TO_COUNT` " +
                  "   , SUM(CASE WHEN ge.RECIPIENT_ID = 2 THEN 1 ELSE 0 END) AS `CC_COUNT` " +
                  "   FROM info_group_email ge " +
                  "   INNER JOIN info_email e ON (e.EMAIL_ID = ge.EMAIL_ID) " +
                  "   GROUP BY ge.GROUP_ID " +
                  ") m ON (m.GROUP_ID = x.GROUP_ID) " +
                  "ORDER BY x.`PRODUCT` ASC, x.`SPEC_COUNT` DESC, x.`CUSTOMER_NAME` ASC ";

            return sql;
        }

        /// <summary>Look up an address in the master list before inserting a new one.</summary>
        public string searchEmailByAddress(SettingEmailProperty dataItem)
        {
            sql = "SELECT EMAIL_ID, EMAIL_ADDRESS, INUSE " +
                  "FROM info_email " +
                  "WHERE EMAIL_ADDRESS = '" + Esc(dataItem.emailAddress) + "' ";

            return sql;
        }

        public string maxEmailId()
        {
            sql = "SELECT IFNULL(MAX(EMAIL_ID), -1) AS `MAX_EMAIL_ID` FROM info_email ";
            return sql;
        }

        public string maxGroupId()
        {
            sql = "SELECT IFNULL(MAX(GROUP_ID), -1) AS `MAX_GROUP_ID` FROM info_group_email ";
            return sql;
        }

        // ---------------------------------------------------------------
        // write statements - run through InsertBySqlList / UpdateBySqlList
        // so every change of one Save is committed in a single transaction
        // ---------------------------------------------------------------

        public string insertEmail(SettingEmailProperty dataItem)
        {
            sql = "INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) " +
                  "VALUES (" + dataItem.emailId + ", '" + Esc(dataItem.emailAddress) + "', '" + Esc(dataItem.createUser) + "', NOW(), 1) ";

            return sql;
        }

        public string insertGroupMember(SettingEmailProperty dataItem)
        {
            sql = "INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) " +
                  "VALUES (" + dataItem.groupId + ", " + dataItem.recipientId + ", " + dataItem.emailId + ") ";

            return sql;
        }

        public string deleteGroupMember(SettingEmailProperty dataItem)
        {
            sql = "DELETE FROM info_group_email " +
                  "WHERE GROUP_ID = " + dataItem.groupId + " AND EMAIL_ID = " + dataItem.emailId + " ";

            return sql;
        }

        /// <summary>RECIPIENT_ID is part of the primary key, so TO/CC is a delete + insert.</summary>
        public string updateRecipientType(SettingEmailProperty dataItem)
        {
            sql = "DELETE FROM info_group_email " +
                  "WHERE GROUP_ID = " + dataItem.groupId + " AND EMAIL_ID = " + dataItem.emailId + "; " +
                  "INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) " +
                  "VALUES (" + dataItem.groupId + ", " + dataItem.recipientId + ", " + dataItem.emailId + "); ";

            return sql;
        }

        /// <summary>
        /// Who is saving: the audit triggers of the key tables (DatabaseScripts\audit_key_tables.sql)
        /// read @app_user for CREATE_BY / UPDATE_BY and log_setting_edata. Run first in the same Save.
        /// </summary>
        public string setAppUser(string empCode)
        {
            return "SET @app_user = '" + Esc(empCode) + "'";
        }

        /// <summary>Run last in the Save, so a pooled connection does not carry the name to the next command.</summary>
        public string clearAppUser()
        {
            return "SET @app_user = NULL";
        }
    }
}
