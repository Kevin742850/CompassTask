using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Compass.Digital.DAL
{
    public class DataAccessHelper : IDisposable
    {
        public string _connectionString;
        private SqlCredential _sqlCredential;
        public DataAccessHelper(string connectionString, SqlCredential sqlCredential)
        {
            _connectionString = connectionString;
            _sqlCredential = sqlCredential;
        }
        public DataSet GetData(string sql, IDbDataParameter[] parameters = null, CommandType commandType = CommandType.Text, int commandTimeout = 30)
        {
            DataSet ds = new DataSet();
            using (var conn = new SqlConnection(_connectionString, _sqlCredential))
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandType = commandType;
                    command.CommandTimeout = commandTimeout;
                    command.CommandText = sql;
                    if (parameters != null && parameters.Length > 0)
                    {
                        foreach (var parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(ds);
                    }
                }
            }

            return ds;
        }


        public int ExecuteNonQuery(string sql, IDbDataParameter[] parameters = null, CommandType commandType = CommandType.Text, int commandTimeout = 30)
        {
            int effectedRows = 0;
            using (var conn = new SqlConnection(_connectionString, _sqlCredential))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                using (var command = conn.CreateCommand())
                {
                    command.CommandType = commandType;
                    command.CommandTimeout = commandTimeout;
                    command.CommandText = sql;
                    if (parameters != null && parameters.Length > 0)
                    {
                        foreach (var parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }

                    effectedRows = command.ExecuteNonQuery();
                }
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return effectedRows;
        }


        #region IDisposable
        public void Dispose()
        {
            // If this function is being called the user wants to release the
            // resources. lets call the Dispose which will do this for us.
            Dispose(true);

            // Now since we have done the cleanup already there is nothing left
            // for the Finalizer to do. So lets tell the GC not to call it later.
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                //someone want the deterministic release of all resources
                //Let us release all the managed resources
                DisposeManagedResources();
            }
            else
            {
                // Do nothing, no one asked a dispose, the object went out of
                // scope and finalized is called so lets next round of GC 
                // release these resources
            }

            // Release the unmanaged resource in any case as they will not be 
            // released by GC
            DisposeUnManagedResources();
        }
        private void DisposeManagedResources()
        {

        }
        private void DisposeUnManagedResources()
        {

        }
        #endregion
    }

}
