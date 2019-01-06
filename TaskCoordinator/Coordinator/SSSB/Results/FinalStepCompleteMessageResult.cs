using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB.Utils;

namespace TaskCoordinator.SSSB
{
    public class FinalStepCompleteMessageResult : HandleMessageResult
    {
        private readonly IStandardMessageHandlers _standardMessageHandlers;

        public FinalStepCompleteMessageResult(IStandardMessageHandlers standardMessageHandlers)
        {
            _standardMessageHandlers = standardMessageHandlers;
        }

        public override async Task Execute(SqlConnection dbconnection, SSSBMessage message, CancellationToken token)
        {
            await _standardMessageHandlers.SendStepCompleted(dbconnection, message);
            await _standardMessageHandlers.EndDialogMessageHandler(dbconnection, message);
        }
    }
}
