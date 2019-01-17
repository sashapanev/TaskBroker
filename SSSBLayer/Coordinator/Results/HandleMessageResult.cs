using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Coordinator.SSSB
{
    public abstract class HandleMessageResult
    {
        public HandleMessageResult()
        {
        }

        public abstract Task Execute(SqlConnection dbconnection, SSSBMessage message, CancellationToken token);
    }
}
