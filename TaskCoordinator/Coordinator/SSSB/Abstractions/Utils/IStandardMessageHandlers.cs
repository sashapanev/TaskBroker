using System.Data.SqlClient;
using System.Threading.Tasks;

namespace TaskCoordinator.SSSB.Utils
{
    public interface IStandardMessageHandlers
    {
        Task EchoMessageHandler(SqlConnection dbconnection, SSSBMessage receivedMessage);
        Task EndDialogMessageHandler(SqlConnection dbconnection, SSSBMessage receivedMessage);
        Task EndDialogMessageWithErrorHandler(SqlConnection dbconnection, SSSBMessage receivedMessage, string message, int errorNumber);
        Task ErrorMessageHandler(SqlConnection dbconnection, SSSBMessage receivedMessage);
        Task SendStepCompleted(SqlConnection dbconnection, SSSBMessage receivedMessage);
        Task SendEmptyMessage(SqlConnection dbconnection, SSSBMessage receivedMessage);
    }
}