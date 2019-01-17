using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Coordinator.SSSB
{
    public class NoopMessageResult : HandleMessageResult
    {
        public NoopMessageResult()
        {
        }

        public override Task Execute(SqlConnection dbconnection, SSSBMessage message, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
