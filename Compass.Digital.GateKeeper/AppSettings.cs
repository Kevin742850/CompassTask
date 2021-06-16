using MMS.Encryption;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Compass.Digital.GateKeeper
{
    public class AppSettings
    {
        public string DBConnectionString { get; set; }
        public string DBUserName { get; set; }
        public string DBUserPassword { get; set; }

        public string TestConnectionServiceUrl { get; set; }
        public double CacheTimeout { get; set; }
        public string JwtSecret { get; set; }
        public string RequestExpiresIn { get; set; }
        public string DeploymentMode { get; set; }
        static SqlCredential _dbCredential;
        public SqlCredential DBCredential
        {
            get
            {
                if (_dbCredential == null)
                {
                    _dbCredential = new SqlCredential(GetDecryptedDBUserName(), GetDecryptedDBPassword());
                }

                return _dbCredential;
            }
        }
        private SecureString GetDecryptedDBPassword()
        {
            SecureString _dbPassword;
            if (!string.IsNullOrWhiteSpace(DBUserPassword))
            {
                string decUserPassword = string.Empty;
                _dbPassword = new SecureString();
                bool result = NativeEncryption.DecryptText(DBUserPassword, ref decUserPassword);
                if (result)
                {
                    foreach (var chr in decUserPassword.ToCharArray())
                    {
                        _dbPassword.AppendChar(chr);
                    }
                    _dbPassword.MakeReadOnly();
                }
                else
                {
                    throw new MissingFieldException("Failed to decrypt database password.");
                }
            }
            else
            {
                throw new MissingFieldException("Missing Database User Password.");
            }

            return _dbPassword;
        }
        private string GetDecryptedDBUserName()
        {
            string decDBUserName = string.Empty;
            if (!string.IsNullOrWhiteSpace(DBUserName))
            {
                bool result = NativeEncryption.DecryptText(DBUserName, ref decDBUserName);
                if (!result)
                {
                    throw new MissingFieldException("Failed to decrypt database username.");
                }
            }
            else
            {
                throw new MissingFieldException("Missing Database User Name.");
            }
            return decDBUserName;
        }

    }

}
