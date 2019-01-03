using System.Data.SqlClient;

namespace TaskCoordinator.SSSB
{
    public interface ISSSBMessageDispatcher: IMessageDispatcher<SSSBMessage, SqlConnection>
    {
        void RegisterMessageHandler(string messageType, IMessageHandler<ServiceMessageEventArgs> handler);
        void RegisterErrorMessageHandler(string messageType, IMessageHandler<ErrorMessageEventArgs> handler);
        void UnregisterMessageHandler(string messageType);
        void UnregisterErrorMessageHandler(string messageType);
    }
}
