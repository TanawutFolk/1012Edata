using EdataManagement.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.SQLFactory
{
    public class EmailSQLFactory
    {
        string sql = "";

        public string allFormatDate()
        {

            sql = "select DATE_FORMAT " +
                "from info_date_format";

            return sql;
        }

        public string getSubject(EmailProperty dataItem)
        {

            sql = "select b.SUBJECT_DETAIL " +
                "from email_subject_" + dataItem.productName + " a " +
                "join info_email_subject b on (a.SUBJECT_ID = b.SUBJECT_ID) " +
                "where a.KEY_ID = '" + dataItem.key + "'";

            return sql;

        }

        public string getBody(EmailProperty dataItem)
        {

            sql = "select b.BODY_NAME as `body` " +
                "from email_body_" + dataItem.productName + " a " +
                "join info_email_body b on (a.BODY_ID = b.Body_ID) " +
                "where a.KEY_ID = '" + dataItem.key + "'";

            return sql;

        }

        public string getEmailAddress(EmailProperty dataItem)
        {
            sql = @"
                SELECT
                    e.EMAIL_ID,
                    e.EMAIL_ADDRESS,
                    er.GROUP_ID,
                    er.RECIPIENT_ID,
                    CASE
                        WHEN er.recipient_id = 1 THEN 'to'
                        WHEN er.recipient_id = 2 THEN 'cc'
                        ELSE 'unknown'
                    END AS recipient_type
                FROM
                    info_group_email er
                JOIN
                    info_email e ON er.EMAIL_ID = e.EMAIL_ID
                JOIN
                    key_group_email_" +dataItem.productName + " f on er.GROUP_ID = f.GROUP_ID "
                    //+ "WHERE er.GROUP_ID = " + dataItem.GroupId + ";" ;
                    + " WHERE f.KEY_ID = " + dataItem.key + ";";
            

            return sql; 
        }

        public string getSignature(EmailProperty dataItem)
        {
            sql = @"SELECT c.SIGNATURE_DETAIL as `Signature`
                FROM info_sender a 
                join info_email b on (a.EMAIL_ID = b.EMAIL_ID)
                join `info_email_signature` c on (a.SIGNATURE_ID = c.signature_ID)
                Where b.EMAIL_ADDRESS = '" + dataItem.emailSender +"';";


            return sql;

        }

        public string getFileFormats(EmailProperty dataItem)
        {
            sql = "select FILE_FORMATS_ID " +
                 "from email_attach_" + dataItem.productName + " " +
                 "where KEY_ID = '" + dataItem.key + "'";

            return sql;
        }

    }
}
