using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Coordinator.Database
{
    public class DbNameConnectionFactory : DbConnectionFactory
    {
        private string _defaultConnectionName;

        public DbNameConnectionFactory(IConfiguration configuration, string defaultConnectionName):
            base(configuration)
        {
            this._defaultConnectionName = defaultConnectionName;
        }

        public override string GetConnectionString(string connectionName)
        {
            return GetConnectionStringByDbName(connectionName);
        }

        public string GetConnectionStringByDbName(string dbname)
        {
            string connStr = base.GetConnectionString(this._defaultConnectionName);
            SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder(connStr);
            scsb.InitialCatalog = dbname;
            connStr = scsb.ToString();
            return connStr;
        }
    }
}
