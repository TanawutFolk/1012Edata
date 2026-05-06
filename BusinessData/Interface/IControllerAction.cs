using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessData.Models;
using BusinessData.Property;
using System.Web.Mvc;

namespace BusinessData.Interface
{
    public interface  IControllerAction<T>
    {
         JsonResult Search(T dataItem);
         JsonResult Insert(T dataItem);
         JsonResult Update(T dataItem);
         JsonResult Delete(T dataItem);
    }
}