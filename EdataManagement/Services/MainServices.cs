using BusinessData.Interface;
using BusinessData.Property;

using EdataManagement.Property;
using EdataManagement.SQLFactory;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.Services
{
    public class MainServices : DatabaseAction<MainProperty>
    {

        private string sql;

        OutputOnDbProperty resultData = new OutputOnDbProperty();
        MainSQLFactory mainSQLFactory = new MainSQLFactory();

        public OutputOnDbProperty getProduct()
        {

            //ConnectionStandAlone คือ xampp Server=127.0.0.1
            //ConnectionQAOC คือ database จริง qa_oc Server=192.168.2.99
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = mainSQLFactory.getProduct();
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public override OutputOnDbProperty Delete(MainProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Insert(MainProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search(MainProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search()
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Update(MainProperty dataItem)
        {
            throw new NotImplementedException();
        }
    }
}
