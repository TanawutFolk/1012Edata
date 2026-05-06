using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using System.Data;
using BusinessData.Property;
using MySql.Data.MySqlClient;

namespace BusinessData.Models
{
    public class TransMysqlModels
    {
        private MySqlConnection conTrans;
        private MySqlCommand userCommand = new MySqlCommand();
        private MySqlTransaction trans;

        public static string strConnection;
        private OutputOnDbProperty dataOutPut;

        protected TransMysqlModels()
        {
            strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;            
        }

        protected OutputOnDbProperty TransConnection()
        {
            try
            {
                conTrans = new MySqlConnection(strConnection);
                conTrans.Open();
                trans = conTrans.BeginTransaction(IsolationLevel.ReadCommitted);     //*** Start  
                userCommand.Connection = conTrans;
                userCommand.Transaction = trans;  //*** Command & Transaction ***// 

                dataOutPut = new OutputOnDbProperty
                {
                    StatusOnDb = true,
                    MessageOnDb = "Connect Database Success",
                    ClassOnDb = "Class Name: TransOracleModels",
                    MethodOnDb = "Method Name: TransConnection",
                    ResultOnDb = null
                };
            }
            catch (Exception er)
            {
                dataOutPut = new OutputOnDbProperty
                {
                    StatusOnDb = false,
                    ClassOnDb = "Class Name: TransOracleModels",
                    MethodOnDb = "Method Name: TransConnection",
                    MessageOnDb = "Connect Database Error ==> " + er.Message,
                    ResultOnDb = null
                };
            }

            return dataOutPut;
        }

        protected OutputOnDbProperty TransSelectCommand(string sql)
        {
             MySqlConnection  connecter = new MySqlConnection(strConnection);

             DataTable tableResult = new DataTable();            
            
            try
            {               
                connecter.Open();

                MySqlDataAdapter userDataAdaptor = new MySqlDataAdapter(sql, connecter);

                userDataAdaptor.Fill(tableResult);

                connecter.Close();
                connecter.Dispose();

                dataOutPut = new OutputOnDbProperty
                {
                    StatusOnDb = true,
                    ClassOnDb = "Class Name: TransOracleModels",
                    MethodOnDb = "Method Name: TransSelectCommand",
                    MessageOnDb = "",
                    ResultOnDb = tableResult
                };

            }
            catch (Exception er)
            {
                dataOutPut = new OutputOnDbProperty
                {
                    StatusOnDb = false,
                    ClassOnDb = "Class Name: TransOracleModels",
                    MethodOnDb = "Method Name: TransSelectCommand",
                    MessageOnDb = "Select SQL Error ==> " + er.Message + "<br>" + sql,
                    ResultOnDb = null
                };
            }

            return dataOutPut;
        }

    

        protected OutputOnDbProperty TransExecuteCommand(string sql)
        {           
            try
            {
                userCommand.CommandText = sql;
                userCommand.CommandType = CommandType.Text;
                userCommand.ExecuteNonQuery();

                dataOutPut = new OutputOnDbProperty
                {
                    StatusOnDb = true,
                    ClassOnDb = "Class Name: TransOracleModels",
                    MethodOnDb = "Method Name: TransExecuteCommand",
                    MessageOnDb = "",
                    ResultOnDb = null
                };
            }
            catch (Exception er)
            {
                dataOutPut = new OutputOnDbProperty
                {
                    StatusOnDb = false,
                    ClassOnDb = "Class Name: TransOracleModels",
                    MethodOnDb = "Method Name: TransExecuteCommand",
                    MessageOnDb = "Execute SQL Command Error ==> " + er.Message,
                    ResultOnDb = null
                };
            }

            return dataOutPut;
        }

        protected void TransCommit()
        {
            trans.Commit();
            conTrans.Close();
            conTrans.Dispose();
        }

        protected void TransRolback()
        {
            trans.Rollback();
            conTrans.Close();
            conTrans.Dispose();
        }

        protected void TransClose()
        {    
           
            conTrans.Close();
            conTrans.Dispose();
        }

        protected string GetDBName()
        {
            string result = "";
            if (conTrans != null)
            {
                result = conTrans.DataSource;
            }
            return result;
        }

    }
}