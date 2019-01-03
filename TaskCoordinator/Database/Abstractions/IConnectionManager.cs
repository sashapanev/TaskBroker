using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCoordinator.Database
{
    public interface IConnectionManager
    {
        Task<SqlConnection> GetConnectionByDbNameAsync(string dbname);
        Task<SqlConnection> GetConnectionByNameAsync(string connectionName);
        Task<SqlConnection> CreateConnectionByDbNameAsync(string dbname);
        Task<SqlConnection> CreateConnectionByNameAsync(string connectionName);
        string GetSSSBConnectionString();
        Task<bool> CheckSSSBConnectionAsync();
        Task<SqlConnection> GetSSSBConnectionAsync();
        Task<SqlConnection> CreateSSSBConnectionAsync(CancellationToken token);
        Task<bool> IsSSSBConnectionOKAsync();
    }
}