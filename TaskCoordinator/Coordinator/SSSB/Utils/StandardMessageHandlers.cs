using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace TaskCoordinator.SSSB.Utils
{
    public class StandardMessageHandlers : IStandardMessageHandlers
    {
        private readonly ILogger _logger;
        private readonly IServiceBrokerHelper _serviceBrokerHelper;

        public StandardMessageHandlers(ILogger<StandardMessageHandlers> logger, IServiceBrokerHelper serviceBrokerHelper)
        {
            _logger = logger;
            _serviceBrokerHelper = serviceBrokerHelper;
        }

        #region Standard MessageHandlers
        /// <summary>
		/// Стандартная обработка ECHO сообщения
		/// </summary>
		/// <param name="receivedMessage"></param>
		public Task EchoMessageHandler(SqlConnection dbconnection, SSSBMessage receivedMessage)
        {
            return _serviceBrokerHelper.SendMessage(dbconnection, receivedMessage);
        }

        /// <summary>
        /// Стандартная обработка сообщения об ошибке
        /// </summary>
        /// <param name="receivedMessage"></param>
        public async Task ErrorMessageHandler(SqlConnection dbconnection, SSSBMessage receivedMessage)
        {
            if (receivedMessage.ConversationHandle.HasValue)
            {
                await _serviceBrokerHelper.EndConversation(dbconnection, receivedMessage.ConversationHandle.Value);
                _logger.LogError(string.Format(ServiceBrokerResources.ErrorMessageReceivedErrMsg, receivedMessage.ConversationHandle.Value, Encoding.Unicode.GetString(receivedMessage.Body)));
            }
        }

        /// <summary>
        /// Стандартная обработка сообщения о завершении диалога
        /// </summary>
        /// <param name="receivedMessage"></param>
        public async Task EndDialogMessageHandler(SqlConnection dbconnection, SSSBMessage receivedMessage)
        {
            if (receivedMessage.ConversationHandle.HasValue)
               await _serviceBrokerHelper.EndConversation(dbconnection, receivedMessage.ConversationHandle.Value);
        }

        /// <summary>
        /// Отправка ответного сообщения о завершении задачи
        /// </summary>
        /// <param name="receivedMessage"></param>
        public async Task SendStepCompleted(SqlConnection dbconnection, SSSBMessage receivedMessage)
        {
            if (receivedMessage.ConversationHandle.HasValue)
                await _serviceBrokerHelper.SendStepCompletedMessage(dbconnection, receivedMessage.ConversationHandle.Value);
        }

        /// <summary>
        /// Отправка пустого сообщения
        /// </summary>
        /// <param name="receivedMessage"></param>
        public async Task SendEmptyMessage(SqlConnection dbconnection, SSSBMessage receivedMessage)
        {
            if (receivedMessage.ConversationHandle.HasValue)
                await _serviceBrokerHelper.SendEmptyMessage(dbconnection, receivedMessage.ConversationHandle.Value);
        }

        /// <summary>
        /// Завершение диалога с отправкой сообщения об ошибке
        /// </summary>
        /// <param name="receivedMessage"></param>
        public Task EndDialogMessageWithErrorHandler(SqlConnection dbconnection, SSSBMessage receivedMessage, string message, int errorNumber)
        {
            return _serviceBrokerHelper.EndConversationWithError(dbconnection, receivedMessage.ConversationHandle.Value, errorNumber, message);
        }
        #endregion
    }
}
