using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB.Utils;

namespace TaskCoordinator.SSSB
{
    public class EndDialogMessageResult : HandleMessageResult
    {
        private readonly IStandardMessageHandlers _standardMessageHandlers;

        public EndDialogMessageResult(IStandardMessageHandlers standardMessageHandlers)
        {
            _standardMessageHandlers = standardMessageHandlers;
        }

        public override Task Execute(SqlConnection dbconnection, SSSBMessage message, CancellationToken token)
        {
            return _standardMessageHandlers.EndDialogMessageHandler(dbconnection, message);
        }
    }
}
