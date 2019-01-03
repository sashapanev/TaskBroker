using Microsoft.Extensions.Configuration;
using Shared.Database;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace TaskCoordinator.Database
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        public virtual string GetConnectionString(string connectionName)
        {
            string connstring = _configuration.GetConnectionString(connectionName);
            if (connstring == null)
            {
                throw new Exception(string.Format("Connection string {0} was not found", connectionName));
            }
            return connstring;
        }

        public DbConnection CreateConnection(string connectionName)
        {
            string connectionString = GetConnectionString(connectionName);
            var result = SqlClientFactory.Instance.CreateConnection();
            result.ConnectionString = connectionString;
            return result;
        }
    }
}
