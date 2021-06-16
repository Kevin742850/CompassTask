using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Compass.Digital.DAL
{
    public abstract class DBManager
    {
        private string _connectionString;
        private SqlCredential _sqlCredential;
        protected string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }
        protected SqlCredential Credential
        {
            get
            {
                return _sqlCredential;
            }
        }
        public DBManager(string connectionString, SqlCredential sqlCredential)
        {
            _connectionString = connectionString;
            _sqlCredential = sqlCredential;
        }

    }

}
