using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace TaskCoordinator.SSSB.Utils
{
    public interface IServiceBrokerHelper
    {
        Task<Guid> BeginDialogConversation(SqlConnection dbconnection, string fromService, string toService, string contractName, TimeSpan lifetime, bool withEncryption, Guid? relatedConversationHandle, Guid? relatedConversationGroupID);
        Task EndConversation(SqlConnection dbconnection, Guid conversationHandle);
        Task EndConversationWithCleanup(SqlConnection dbconnection, Guid conversationHandle);
        Task EndConversationWithError(SqlConnection dbconnection, Guid conversationHandle, int errorCode, string errorDescription);
        Task<string> GetServiceQueueName(string serviceName);
        Task SendMessage(SqlConnection dbconnection, SSSBMessage message);
        Task<long?> SendPendingMessage(SqlConnection dbconnection, string fromService, SSSBMessage message, TimeSpan lifetime, bool isWithEncryption, Guid? initiatorConversationGroupID, DateTime activationTime, string objectID, bool isOneWay = true);
        Task<int> ProcessPendingMessages(SqlConnection dbconnection, bool processAll = false, string objectID = null);
        Task SendStepCompletedMessage(SqlConnection dbconnection, Guid conversationHandle);
        Task SendEmptyMessage(SqlConnection dbconnection, Guid conversationHandle);
    }
}