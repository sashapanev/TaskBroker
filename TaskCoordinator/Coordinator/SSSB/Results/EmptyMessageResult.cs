using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB.Utils;

namespace TaskCoordinator.SSSB
{
    public class EmptyMessageResult : HandleMessageResult
    {
        private readonly IStandardMessageHandlers _standardMessageHandlers;

        public EmptyMessageResult(IStandardMessageHandlers standardMessageHandlers)
        {
            _standardMessageHandlers = standardMessageHandlers;
        }

        public override Task Execute(SqlConnection dbconnection, SSSBMessage message, CancellationToken token)
        {
            return _standardMessageHandlers.SendEmptyMessage(dbconnection, message);
        }
    }
}
