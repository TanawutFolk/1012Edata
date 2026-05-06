using EdataManagement.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.SQLFactory
{
    public class ExcelSQLFactory
    {
        string sql = "";

        public string infoPOallValue(ExcelProperty dataItem)
        {
            sql = "select `START_ROW` , `KEY_COLUMN` , `VALUE_COLUMN` , `KEY_COLUMN_NAME` , `VALUE_COLUMN_NAME` , `VARIABLE`" +
                " from info_po_all " +
                " where product_id = " + dataItem.productID +
                " and VARIABLE = '" + dataItem.VARIABLE + "'";

            return sql;
        }
    }
}
