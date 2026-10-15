using EdataManagement.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.SQLFactory
{
    public class SettingSQLFactory
    {
        private string sql;

        // the key tables have audit triggers (DatabaseScripts\audit_key_tables.sql): they read @app_user
        // to fill CREATE_BY / UPDATE_BY and log_setting_edata. Harmless where no trigger exists.
        private const string SET_AUTO_SYNC = "SET @app_user = 'auto-sync'; ";
        private const string CLEAR_APP_USER = "SET @app_user = NULL; ";

        public string maxCusID(SettingProperty dataItem)
        {

            sql = " SELECT IFNULL(MAX(CusCondition_ID), 0) as `max_id_cus` " +
                " FROM " + dataItem.dbProduct + ".`info_cus_condition` ";

            return sql;

        }

        // ส่งยังไง
        public string maxKeyID(SettingProperty dataItem)
        {

            sql = "SELECT " +
                "(SELECT IFNULL(MAX(KEY_ID), 0) FROM email_subject_" + dataItem.productName + ") AS max_email_subject_key_id," +
                "(SELECT IFNULL(MAX(KEY_ID), 0) FROM email_body_" + dataItem.productName + ") AS max_email_body_key_id," +
                "(SELECT IFNULL(MAX(KEY_ID), 0) FROM email_attach_" + dataItem.productName + ") AS max_email_attach_key_id ," +
                "(SELECT IFNULL(MAX(KEY_ID), 0) FROM key_send_methods_" + dataItem.productName + ") AS max_key_send_methods_key_id ," +
                "(SELECT IFNULL(MAX(KEY_ID), 0) FROM key_group_email_" + dataItem.productName + ") AS max_key_group_email_key_id ;";

            return sql;

        }
        public string insertUpdateKeySendMethods(SettingProperty dataItem)
        {
            sql = SET_AUTO_SYNC + "INSERT INTO key_send_methods_" + dataItem.productName + " (key_id, SEND_METHODS_ID) " +
                  "SELECT a.CusCondition_ID, NULL " +
                  "FROM " + dataItem.dbProduct + ".info_cus_condition a " +
                  "LEFT JOIN key_send_methods_" + dataItem.productName + " b ON a.CusCondition_ID = b.key_id " +
                  "WHERE b.key_id IS NULL; " +

                  "UPDATE key_send_methods_" + dataItem.productName + " AS b " +
                        "JOIN " + dataItem.dbProduct + ".info_cus_condition AS a ON b.key_id = a.CusCondition_ID " +
                        "LEFT JOIN ( " +
                     "SELECT a1.customer, " +
                        "CASE " +
                            "WHEN COUNT(DISTINCT CASE WHEN b1.SEND_METHODS_ID != 0 THEN b1.SEND_METHODS_ID END) = 1 " +
                            "THEN MAX(CASE WHEN b1.SEND_METHODS_ID != 0 THEN b1.SEND_METHODS_ID END) " +
                            "WHEN COUNT(DISTINCT CASE WHEN b1.SEND_METHODS_ID != 0 THEN b1.SEND_METHODS_ID END) > 1 " +
                            "THEN 0 " +
                            "ELSE 0 " +
                        "END AS SEND_METHODS_ID " +
                     "FROM  " + dataItem.dbProduct + ".info_cus_condition AS a1 " +
                     "JOIN key_send_methods_" + dataItem.productName + " AS b1 ON a1.CusCondition_ID = b1.key_id " +
                     "WHERE b1.SEND_METHODS_ID != 0 " +
                     "GROUP BY a1.customer " +
                  ") AS subquery ON a.customer = subquery.customer " +
                  "SET b.SEND_METHODS_ID = IFNULL(subquery.SEND_METHODS_ID,0) " +
                  "WHERE b.SEND_METHODS_ID IS NULL; " + CLEAR_APP_USER

            ;

            return sql;
        }

        public string insertUpdateEmailSubject(SettingProperty dataItem)
        {
            sql = SET_AUTO_SYNC + "INSERT INTO email_subject_" + dataItem.productName + " (key_id, SUBJECT_ID) " +
                  "SELECT a.CusCondition_ID, NULL " +
                  "FROM " + dataItem.dbProduct + ".info_cus_condition a " +
                  "LEFT JOIN email_subject_" + dataItem.productName + " b ON a.CusCondition_ID = b.key_id " +
                  "WHERE b.key_id IS NULL; " +

                  "UPDATE email_subject_" + dataItem.productName + " AS b " +
                        "JOIN " + dataItem.dbProduct + ".info_cus_condition AS a ON b.key_id = a.CusCondition_ID " +
                        "LEFT JOIN ( " +
                     "SELECT a1.customer, " +
                        "CASE " +
                            "WHEN COUNT(DISTINCT CASE WHEN b1.SUBJECT_ID != 0 THEN b1.SUBJECT_ID END) = 1 " +
                            "THEN MAX(CASE WHEN b1.SUBJECT_ID != 0 THEN b1.SUBJECT_ID END) " +
                            "WHEN COUNT(DISTINCT CASE WHEN b1.SUBJECT_ID != 0 THEN b1.SUBJECT_ID END) > 1 " +
                            "THEN 0 " +
                            "ELSE 0 " +
                        "END AS SUBJECT_ID " +
                     "FROM  " + dataItem.dbProduct + ".info_cus_condition AS a1 " +
                     "JOIN email_subject_" + dataItem.productName + " AS b1 ON a1.CusCondition_ID = b1.key_id " +
                     "WHERE b1.SUBJECT_ID != 0 " +
                     "GROUP BY a1.customer " +
                  ") AS subquery ON a.customer = subquery.customer " +
                  "SET b.SUBJECT_ID = IFNULL(subquery.SUBJECT_ID,0) " +
                  "WHERE b.SUBJECT_ID IS NULL; " + CLEAR_APP_USER

            ;

            return sql;
        }

        public string insertUpdateEmailBody(SettingProperty dataItem)
        {
            sql = SET_AUTO_SYNC + "INSERT INTO email_body_" + dataItem.productName + " (key_id, BODY_ID) " +
                  "SELECT a.CusCondition_ID, NULL " +
                  "FROM " + dataItem.dbProduct + ".info_cus_condition a " +
                  "LEFT JOIN email_body_" + dataItem.productName + " b ON a.CusCondition_ID = b.key_id " +
                  "WHERE b.key_id IS NULL; " +

                  "UPDATE email_body_" + dataItem.productName + " AS b " +
                        "JOIN " + dataItem.dbProduct + ".info_cus_condition AS a ON b.key_id = a.CusCondition_ID " +
                        "LEFT JOIN ( " +
                     "SELECT a1.customer, " +
                        "CASE " +
                            "WHEN COUNT(DISTINCT CASE WHEN b1.BODY_ID != 0 THEN b1.BODY_ID END) = 1 " +
                            "THEN MAX(CASE WHEN b1.BODY_ID != 0 THEN b1.BODY_ID END) " +
                            "WHEN COUNT(DISTINCT CASE WHEN b1.BODY_ID != 0 THEN b1.BODY_ID END) > 1 " +
                            "THEN 0 " +
                            "ELSE 0 " +
                        "END AS BODY_ID " +
                     "FROM  " + dataItem.dbProduct + ".info_cus_condition AS a1 " +
                     "JOIN email_body_" + dataItem.productName + " AS b1 ON a1.CusCondition_ID = b1.key_id " +
                     "WHERE b1.BODY_ID != 0 " +
                     "GROUP BY a1.customer " +
                  ") AS subquery ON a.customer = subquery.customer " +
                  "SET b.BODY_ID = IFNULL(subquery.BODY_ID,0) " +
                  "WHERE b.BODY_ID IS NULL; " + CLEAR_APP_USER

            ;

            return sql;
        }

        public string insertUpdateEmailAttach(SettingProperty dataItem)
        {
            sql = SET_AUTO_SYNC + "INSERT INTO email_attach_" + dataItem.productName + " (key_id, FILE_FORMATS_ID) " +
                  "SELECT a.CusCondition_ID, NULL " +
                  "FROM " + dataItem.dbProduct + ".info_cus_condition a " +
                  "LEFT JOIN email_attach_" + dataItem.productName + " b ON a.CusCondition_ID = b.key_id " +
                  "WHERE b.key_id IS NULL; " +

                  "UPDATE email_attach_" + dataItem.productName + " AS b " +
                        "JOIN " + dataItem.dbProduct + ".info_cus_condition AS a ON b.key_id = a.CusCondition_ID " +
                        "LEFT JOIN ( " +
                     "SELECT a1.customer, " +
                        "CASE " +
                            "WHEN COUNT(DISTINCT CASE WHEN b1.FILE_FORMATS_ID != 0 THEN b1.FILE_FORMATS_ID END) = 1 " +
                            "THEN MAX(CASE WHEN b1.FILE_FORMATS_ID != 0 THEN b1.FILE_FORMATS_ID END) " +
                            "WHEN COUNT(DISTINCT CASE WHEN b1.FILE_FORMATS_ID != 0 THEN b1.FILE_FORMATS_ID END) > 1 " +
                            "THEN 0 " +
                            "ELSE 0 " +
                        "END AS FILE_FORMATS_ID " +
                     "FROM  " + dataItem.dbProduct + ".info_cus_condition AS a1 " +
                     "JOIN email_attach_" + dataItem.productName + " AS b1 ON a1.CusCondition_ID = b1.key_id " +
                     "WHERE b1.FILE_FORMATS_ID != 0 " +
                     "GROUP BY a1.customer " +
                  ") AS subquery ON a.customer = subquery.customer " +
                  "SET b.FILE_FORMATS_ID = IFNULL(subquery.FILE_FORMATS_ID,0) " +
                  "WHERE b.FILE_FORMATS_ID IS NULL; " + CLEAR_APP_USER

            ;

            return sql;
        }


        public string insertUpdateKeyGroupEmail(SettingProperty dataItem)
        {
            sql = SET_AUTO_SYNC + "INSERT INTO key_group_email_" + dataItem.productName + " (key_id, GROUP_ID) " +
                  "SELECT a.CusCondition_ID, NULL " +
                  "FROM " + dataItem.dbProduct + ".info_cus_condition a " +
                  "LEFT JOIN key_group_email_" + dataItem.productName + " b ON a.CusCondition_ID = b.key_id " +
                  "WHERE b.key_id IS NULL; " +

                  "UPDATE key_group_email_" + dataItem.productName + " AS b " +
                        "JOIN " + dataItem.dbProduct + ".info_cus_condition AS a ON b.key_id = a.CusCondition_ID " +
                        "LEFT JOIN ( " +
                     "SELECT a1.customer, " +
                        "CASE " +
                            "WHEN COUNT(DISTINCT CASE WHEN b1.GROUP_ID != 0 THEN b1.GROUP_ID END) = 1 " +
                            "THEN MAX(CASE WHEN b1.GROUP_ID != 0 THEN b1.GROUP_ID END) " +
                            "WHEN COUNT(DISTINCT CASE WHEN b1.GROUP_ID != 0 THEN b1.GROUP_ID END) > 1 " +
                            "THEN 0 " +
                            "ELSE 0 " +
                        "END AS GROUP_ID " +
                     "FROM  " + dataItem.dbProduct + ".info_cus_condition AS a1 " +
                     "JOIN key_group_email_" + dataItem.productName + " AS b1 ON a1.CusCondition_ID = b1.key_id " +
                     "WHERE b1.GROUP_ID != 0 " +
                     "GROUP BY a1.customer " +
                  ") AS subquery ON a.customer = subquery.customer " +
                  "SET b.GROUP_ID = IFNULL(subquery.GROUP_ID,0) " +
                  "WHERE b.GROUP_ID IS NULL; " + CLEAR_APP_USER

            ;

            return sql;
        }

        //public string infoPOallValue(SettingProperty dataItem)
        //{
        //    sql = "select `START_ROW` , `KEY_COLUMN` , `VALUE_COLUMN` , `KEY_COLUMN_NAME` , `VALUE_COLUMN_NAME` , `VARIABLE`" +
        //        " from info_po_all " +
        //        " where product_id = " + dataItem.productID +
        //        " and VARIABLE = '" + dataItem.VARIABLE;

        //    return sql;
        //}
    }
}
