using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessData.Property;

namespace BusinessData.Models
{
    public class DatabaseExecuteModels : TransMysqlModels, IDisposable
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();

        protected OutputOnDbProperty SearchBySqlList(List<string> sqlList)
        {
            try
            {
                resultData = base.TransConnection();

                if (resultData.StatusOnDb == true)
                {                    
                    resultData = base.TransSelectCommand(sqlList[0]);

                    string totalCount = resultData.ResultOnDb.Rows[0]["TOTAL_COUNT"].ToString();

                    resultData = base.TransSelectCommand(sqlList[1]);

                    resultData.TotalCountOnDb = totalCount;
                }
            }
            finally
            {
                base.TransClose();
            }

            return resultData;
        }

        protected OutputOnDbProperty SearchBySql(string sql)
        {
            try
            {
                resultData = base.TransConnection();

                if (resultData.StatusOnDb == true)
                {
                    resultData = base.TransSelectCommand(sql);                   
                }
            }
            finally
            {
                base.TransClose();
            }

            return resultData;
        }

        protected OutputOnDbProperty InsertBySql(string sql)
        {
            try
            {
                resultData = base.TransConnection();

                if (resultData.StatusOnDb == true)
                {                  
                    resultData = base.TransExecuteCommand(sql);

                    if (resultData.StatusOnDb == true)
                    {
                        base.TransCommit();
                        resultData.MessageOnDb = "Save Data Success";
                    }
                    else
                    {
                        base.TransRolback();
                    }
                }
            }
            finally
            {
                base.TransClose();
            }
            return resultData;
        }
        protected OutputOnDbProperty InsertBySqlList(List<string> sqlList)
        {
            try
            {
                resultData = base.TransConnection();
                if (resultData.StatusOnDb == true)
                {
                    foreach (string sql in sqlList)
                    {
                        resultData = base.TransExecuteCommand(sql);
                        if (resultData.StatusOnDb == false)
                        {
                            base.TransRolback();
                            return resultData;
                        }
                    }
                    base.TransCommit();
                    resultData.MessageOnDb = "Save Data Success";
                }
            }
            finally
            {
                base.TransClose();
            }
            return resultData;
        }
        protected OutputOnDbProperty UpdateBySql(string sql)
        {
            try
            {
                resultData = base.TransConnection();

                if (resultData.StatusOnDb == true)
                {
                    resultData = base.TransExecuteCommand(sql);

                    if (resultData.StatusOnDb == true)
                    {
                        base.TransCommit();
                        resultData.MessageOnDb = "Update Data Success";
                    }
                    else
                    {
                        base.TransRolback();
                    }
                }
            }
            finally
            {
                base.TransClose();
            }

            return resultData;
        }
        protected OutputOnDbProperty UpdateBySqlList(List<string> sqlList)
        {
            try
            {
                resultData = base.TransConnection();
                if (resultData.StatusOnDb == true)
                {
                    foreach (string sql in sqlList)
                    {
                        resultData = base.TransExecuteCommand(sql);
                        if (resultData.StatusOnDb == false)
                        {
                            base.TransRolback();
                            return resultData;
                        }
                    }
                    base.TransCommit();
                    resultData.MessageOnDb = "Update Data Success";
                }
            }
            finally
            {
                base.TransClose();
            }
            return resultData;
        }

        protected OutputOnDbProperty DeleteBySql(string sql)
        {
            try
            {
                resultData = base.TransConnection();

                if (resultData.StatusOnDb == true)
                {                   
                    resultData = base.TransExecuteCommand(sql);

                    if (resultData.StatusOnDb == true)
                    {
                        base.TransCommit();
                        resultData.MessageOnDb = "Delete Data Success";
                    }
                    else
                    {
                        base.TransRolback();
                    }
                }
            }
            finally
            {
                base.TransClose();
            }

            return resultData;
        }

       # region  Dispose Object

            public void Dispose()
            {
                try
                {
                    base.TransClose();
                }
                finally
                {
                    GC.SuppressFinalize(this);
                }

            }

      #endregion


    }
}