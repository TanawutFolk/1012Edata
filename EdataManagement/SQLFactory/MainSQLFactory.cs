using EdataManagement.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.SQLFactory
{
    public class MainSQLFactory
    {

        private string sql;
        public string getProduct()
        {

            sql = " SELECT a.PRODUCT_ID , a.PRODUCT_NAME , a.NAME_IN_EMAIL , b.PATH_SHIPPING , c.PATH_PO_ALL , d.DATABASE_NAME ,a.NAME_IN_FileServer " +
                    " from info_product a " +
                    " join info_path_shipping b on (a.PRODUCT_ID = b.PRODUCT_ID) " +
                    // left join: 1480Dual has no Shipping PO all file, its format (13) does not read it
                    " left join info_path_po_all c on (a.PRODUCT_ID = c.PRODUCT_ID) " +
                    //" join info_connection_production c on (a.PRODUCT_ID = c.PRODUCT_ID) " +
                    //" join info_connection_qa d on (a.PRODUCT_ID = d.PRODUCT_ID) " +
                    " join info_db_name d on (a.PRODUCT_ID = d.PRODUCT_ID) " +
                    " where (a.inuse = 1) and ((a.PRODUCT_ID = 1) OR (a.PRODUCT_ID=2) OR (a.PRODUCT_ID=4) OR (a.PRODUCT_ID=9) OR (a.PRODUCT_ID=11)) ";
                    //" where (a.inuse = 1) and ((a.PRODUCT_ID = 1) OR (a.PRODUCT_ID=2) OR (a.PRODUCT_ID=4) OR (a.PRODUCT_ID=9) ) ";

            return sql;

        }

        //public string getCustCondIDProd()
        //{

        //}



    }
}
