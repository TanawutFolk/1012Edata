using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EdataManagement.Property;

namespace EdataManagement.SQLFactory
{
    public class ShippingSQLFactory
    {
        private string sql;
        public string SearchCustPartNoAll(ShippingProperty dataItem)
        {

            sql = "select B.CusCondition_ID as `KEY_ID` ,B.Customer as `customerName` , B.Part_No , b.Specification , A.FECOrderNo as `manuFactureNo` , A.TestReportNo , B.Customer_Part_No as `customerPN` , COUNT(*) AS `Qty` " +
                "from " + dataItem.dbProduct +".shipping A " +
                "INNER JOIN "+dataItem.dbProduct + ".info_cus_condition B on (A.CusCond_ID = B.CusCondition_ID) " +
                "where A.Ship_Date = '" + dataItem.shipdate + "' GROUP BY TestReportNo ";

            return sql;

        }

        public string SearchCustPartNo(ShippingProperty dataItem)
        {

            sql = "select B.CusCondition_ID as `KEY_ID` ,B.Customer as `customerName` , B.Part_No , b.Specification , A.FECOrderNo as `manuFactureNo` , A.TestReportNo , B.Customer_Part_No as `customerPN` ,C.SEND_METHODS_ID ,COUNT(*) AS `Qty` , E.SubFolder as `subFolder` " +
                "from " + dataItem.dbProduct + ".shipping A " +
                "INNER JOIN " + dataItem.dbProduct + ".info_cus_condition B on (A.CusCond_ID = B.CusCondition_ID) " +
                "INNER JOIN " + dataItem.dbProduct + ".db_check_sheet E on (A.TestReportNo = E.TestReportNo and E.Inuse = 1) " +
                "INNER JOIN key_send_methods_"  + dataItem.productName + " C ON(A.CusCond_ID = C.KEY_ID) " +
                "where A.Ship_Date = '" + dataItem.shipdate + "' and B.Customer = '" + dataItem.customerName + "' and C.SEND_METHODS_ID != 0 GROUP BY TestReportNo ORDER BY manuFactureNo ,subfolder ASC ";

            return sql;
        }

        public string searchCustomerShipping(ShippingProperty dataItem)
        {
            sql = "select B.CusCondition_ID as `KEY_ID` ,B.Customer as `customerName` , B.Part_No , b.Specification , A.FECOrderNo as `manuFactureNo` , A.TestReportNo , B.Customer_Part_No as `customerPN` ,C.SEND_METHODS_ID,D.FILE_FORMATS_ID  , COUNT(*) AS `Qty` " +
               "from " + dataItem.dbProduct + ".shipping A " +
               "INNER JOIN " + dataItem.dbProduct + ".info_cus_condition B on (A.CusCond_ID = B.CusCondition_ID) " +
               "INNER JOIN key_send_methods_" + dataItem.productName + " C ON(A.CusCond_ID = C.KEY_ID) " +
               "INNER JOIN email_attach_" + dataItem.productName + " D ON(A.CusCond_ID = D.KEY_ID) " +
               "where A.Ship_Date = '" + dataItem.shipdate + "' and C.SEND_METHODS_ID != 0 GROUP BY customerName ";

            return sql;
        }

        public string SearchSerialShipping(ShippingProperty dataItem)
        {
            sql = "select  A.Serial " +
                "from " + dataItem.dbProduct +".shipping A " +
                "INNER JOIN  "+dataItem.dbProduct + ".info_cus_condition B on (A.CusCond_ID = B.CusCondition_ID) " +
                "where a.FECOrderNo = '" + dataItem.fecOrderNo + "' and A.TestReportNo = '" + dataItem.testReportNo + "'";
            // "where A.Ship_Date = '" +dataItem.shipdate + "' and a.FECOrderNo = '"+dataItem.fecOrderNo+"' and A.TestReportNo = '"+dataItem.testReportNo +"'";
            return sql;
        }

        public string SearchSerialShippingCust(ShippingProperty dataItem)
        {
            sql = "select  A.Serial " +
                "from " + dataItem.dbProduct + ".shipping A " +
                "INNER JOIN  " + dataItem.dbProduct + ".info_cus_condition B on (A.CusCond_ID = B.CusCondition_ID) " +
                "where A.Ship_Date = '"+ dataItem.shipdate +"' and B.Customer = '"+ dataItem.customerName +"' and A.FECOrderNo = '"+ dataItem.fecOrderNo +"'";
            // "where A.Ship_Date = '" +dataItem.shipdate + "' and a.FECOrderNo = '"+dataItem.fecOrderNo+"' and A.TestReportNo = '"+dataItem.testReportNo +"'";
            return sql;
        }

        public string getSendMethods(ShippingProperty dataItem)
        {
            sql = "SELECT SEND_METHODS_ID" +
                " from key_send_methods_" + dataItem.productName + 
                " where KEY_ID = " + dataItem.key + "";
            return sql;
        }

        //public string SearchCustPartNo(ShippingProperty dataItem)
        //{

        //    sql = "select B.Customer , B.Part_No , b.Specification , A.FECOrderNo , A.TestReportNo , B.Customer_Part_No " +
        //        "from shipping A " +
        //        "INNER JOIN info_cus_condition B on (A.CusCond_ID = B.CusCondition_ID) " +
        //        "where A.Ship_Date = '" + dataItem.shipdate + "' and B.E_Data_Content like 'Y-3ZD2001%' GROUP BY TestReportNo ";

        //    return sql;

        //}

    }
}
