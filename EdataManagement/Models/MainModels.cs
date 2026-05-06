using BusinessData.Property;
using EdataManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.Models
{
    public class MainModels
    {
        OutputOnDbProperty _resultData = new OutputOnDbProperty();
        MainServices _mainServices = new MainServices();

        public OutputOnDbProperty getProduct()
        {
            _resultData = _mainServices.getProduct();
            return _resultData;
        }


    }
}
