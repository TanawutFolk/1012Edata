using BusinessData.Interface;
using BusinessData.Property;
using BusinessData.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace BusinessData.Network
{
    public class Network
    {
        public List<NetworkProperty> _listDB = new List<NetworkProperty>();
        public Network()
        {
            this.Check_ConnectStatus();
        }

        public void Check_ConnectStatus()
        {
            NetworkControllors _controllors = new NetworkControllors();
            List<string> strConnection = new List<string>();

            strConnection.Add(ConfigurationManager.ConnectionStrings["ConnectionStrMysql"].ConnectionString);

            _listDB = new List<NetworkProperty>();
            foreach (string ip in strConnection)
            {
                NetworkProperty _property = new NetworkProperty();
                _property.IP = Get_IP_Name(ip);
                _property = _controllors.CheckNewtwork(_property);
                _listDB.Add(_property);
            }


        }

        private string Get_IP_Name(string strConnect)
        {
            string _result = "";
            if (strConnect != "")
            {
                string[] _array = strConnect.Split(';');
                if (_array.Length > 0)
                {
                    string[] _dbName = _array[0].Split('=');
                    string _name = "Database_Name : " + _dbName[1];

                    string[] _dbIP = _array[1].Split('=');
                    string _ip =  _dbIP[1];

                    _result = _name + " | " + _ip;
                }
            }
            return _result;
        }
    }
    class NetworkControllors
    {
        NetworkModels _models = new NetworkModels();
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        NetworkProperty _conn = new NetworkProperty();

        public NetworkProperty CheckNewtwork(NetworkProperty dataItem)
        {
            _conn.IP = dataItem.IP;
            _conn.STATUS = false;

            resultData = _models.GetDatatime();
            if (resultData.StatusOnDb == true)
            {
                _conn.STATUS = true;
            }
            return _conn;
        }
    }
    class NetworkModels
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        NetworkServices _services = new NetworkServices();

        public OutputOnDbProperty GetDatatime()
        {
            resultData = _services.GetDatatime();
            return resultData;
        }
    }
    class NetworkServices : DatabaseAction<NetworkProperty>
    {
        OutputOnDbProperty resultData = new OutputOnDbProperty();
        NetworkSQL _sql = new NetworkSQL();

        private string sql;
        public OutputOnDbProperty GetDatatime()
        {
            sql = _sql.GetDatatime();
            resultData = base.SearchBySql(sql);
            return resultData;
        }

        public override OutputOnDbProperty Delete(NetworkProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Insert(NetworkProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search(NetworkProperty dataItem)
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Search()
        {
            throw new NotImplementedException();
        }

        public override OutputOnDbProperty Update(NetworkProperty dataItem)
        {
            throw new NotImplementedException();
        }

    }
    public class NetworkProperty
    {
        public string IP { get; set; }
        public bool STATUS { get; set; }
    }
    class NetworkSQL
    {
        private string sql;
        public string GetDatatime()
        {
            sql = @"SELECT NOW() AS DATETIME_NOW;";
            return sql;
        }
    }
    
}

