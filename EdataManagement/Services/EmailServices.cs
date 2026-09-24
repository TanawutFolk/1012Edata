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
    public class EmailServices : DatabaseAction<EmailProperty>
    {

        private string sql;
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        EmailSQLFactory sqlFactory = new EmailSQLFactory();

        public OutputOnDbProperty getSubject(EmailProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.getSubject(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty getBody(EmailProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.getBody(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty getDocumentEmailAddress(string productName, string customerName, int documentId)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.getDocumentEmailAddress(productName, customerName, documentId);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty getSignature(EmailProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.getSignature(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty allFormatDate()
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.allFormatDate();
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty getEmailAddress(EmailProperty dataItem)
        {

            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.getEmailAddress(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public OutputOnDbProperty getFileFormats(EmailProperty dataItem)
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
            sql = sqlFactory.getFileFormats(dataItem);
            resultData = base.SearchBySql(sql);
            return resultData;
        }


        public override OutputOnDbProperty Delete(EmailProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Insert(EmailProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search(EmailProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search()
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Update(EmailProperty dataItem)
        {
            throw new NotImplementedException();
        }
    }
}
