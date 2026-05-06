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
    public class ExcelServices : DatabaseAction<ExcelProperty>
    {

        private string sql;
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        ExcelSQLFactory sqlFactory = new ExcelSQLFactory();

        public OutputOnDbProperty infoPOallValue(ExcelProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.infoPOallValue(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public override OutputOnDbProperty Delete(ExcelProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Insert(ExcelProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search(ExcelProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search()
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Update(ExcelProperty dataItem)
        {
            throw new NotImplementedException();
        }
    }
}
