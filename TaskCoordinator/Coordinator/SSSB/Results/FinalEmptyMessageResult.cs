using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCoordinator.SSSB
{
    public class FinalEmptyMessageResult : HandleMessageResult
    {
        private readonly IStandardMessageHandlers _standardMessageHandlers;

        public FinalEmptyMessageResult(IStandardMessageHandlers standardMessageHandlers)
        {
            _standardMessageHandlers = standardMessageHandlers;
        }

        public override async Task Execute(SqlConnection dbconnection, SSSBMessage message, CancellationToken token)
        {
            await _standardMessageHandlers.SendEmptyMessage(dbconnection, message);
            await _standardMessageHandlers.EndDialogMessageHandler(dbconnection, message);
        }
    }
}
