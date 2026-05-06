using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessData.Models;
using BusinessData.Property;


namespace BusinessData.Interface
{
    public abstract class DatabaseAction<T> : DatabaseExecuteModels
    {
       public abstract OutputOnDbProperty Search(T dataItem);
       public abstract OutputOnDbProperty Search();
       public abstract OutputOnDbProperty Insert(T dataItem);
       public abstract OutputOnDbProperty Update(T dataItem);
       public abstract OutputOnDbProperty Delete(T dataItem);
    
    }
}