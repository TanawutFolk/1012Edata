using BusinessData.Property;
using EdataManagement.Property;
using EdataManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.Models
{
    public class EmailModels
    {
        OutputOnDbProperty _resultData = new OutputOnDbProperty();
        EmailServices _services = new EmailServices();

        public OutputOnDbProperty getSubject(EmailProperty dataItem)
        {
            _resultData = _services.getSubject(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty getBody(EmailProperty dataItem)
        {
            _resultData = _services.getBody(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty getDocumentEmailAddress(string productName, string customerName, int documentId)
        {
            _resultData = _services.getDocumentEmailAddress(productName, customerName, documentId);
            return _resultData;
        }

        public OutputOnDbProperty getGroupEmailAddress(int groupId)
        {
            _resultData = _services.getGroupEmailAddress(groupId);
            return _resultData;
        }

        public OutputOnDbProperty getSignature(EmailProperty dataItem)
        {
            _resultData = _services.getSignature(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty allFormatDate()
        {
            _resultData = _services.allFormatDate();
            return _resultData;
        }

        public OutputOnDbProperty getEmailAddress(EmailProperty dataItem)
        {
            _resultData = _services.getEmailAddress(dataItem);
            return _resultData;
        }

        public OutputOnDbProperty getFileFormats(EmailProperty dataItem)
        {
            _resultData = _services.getFileFormats(dataItem);
            return _resultData;
        }

    }
}
