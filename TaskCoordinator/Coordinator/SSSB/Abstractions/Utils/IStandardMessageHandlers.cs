using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace TaskCoordinator.SSSB.Utils
{
    public interface IStandardMessageHandlers
    {
        Task EchoMessageHandler(SqlConnection dbconnection, SSSBMessage receivedMessage);
        Task ErrorMessageHandler(SqlConnection dbconnection, SSSBMessage receivedMessage);
        Task EndDialogMessageHandler(SqlConnection dbconnection, SSSBMessage receivedMessage);
        Task EndDialogMessageWithErrorHandler(SqlConnection dbconnection, SSSBMessage receivedMessage, string message, int errorNumber);
        Task SendStepCompleted(SqlConnection dbconnection, Guid conversationHandle);
        Task SendEmptyMessage(SqlConnection dbconnection, Guid conversationHandle);
    }
}