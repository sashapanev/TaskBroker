using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TaskCoordinator.Database;

namespace TaskCoordinator.SSSB
{
    /// <summary>
    /// Вспомогательный класс для работы с SQL Service Broker.
    /// </summary>
    public class ServiceBrokerHelper : IServiceBrokerHelper
    {
        private readonly ILogger _logger;
        private readonly ISSSBManager _manager;

        public ServiceBrokerHelper(ILogger<ServiceBrokerHelper> logger, ISSSBManager manager)
        {
            _logger = logger;
            _manager = manager;
        }

        /// <summary>
        /// Запуск диалога обмена сообщениями.
        /// </summary>
        /// <param name="fromService"></param>
        /// <param name="toService"></param>
        /// <param name="contractName"></param>
        /// <param name="lifetime"></param>
        /// <param name="withEncryption"></param>
        /// <param name="relatedConversationHandle"></param>
        /// <param name="relatedConversationGroupID"></param>
        /// <returns></returns>
        public async Task<Guid> BeginDialogConversation(SqlConnection dbconnection, string fromService, string toService, string contractName, 
			TimeSpan lifetime, bool withEncryption,	Guid? relatedConversationHandle, Guid? relatedConversationGroupID)
		{
			_logger.LogInformation("Выполнение метода BeginDialogConversation(fromService, toService, contractName, lifetime, withEncryption, relatedConversationID, relatedConversationGroupID)");
			try
			{
                Guid? conversationHandle = await _manager.BeginDialogConversation(dbconnection, fromService, toService, contractName,
                    lifetime == TimeSpan.Zero ? (int?)null : (int)lifetime.TotalSeconds,
                    withEncryption, relatedConversationHandle, relatedConversationGroupID);

                return conversationHandle.Value;
            }
            catch (SqlException ex)
            {
                DBWrapperExceptionsHelper.ThrowError(ex, ServiceBrokerResources.BeginDialogConversationErrMsg);
                return Guid.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception(ServiceBrokerResources.BeginDialogConversationErrMsg, ex);
            }
		}
		
		/// <summary>
		/// Завершение диалога
		/// </summary>
		/// <param name="conversationHandle"></param>
		/// <param name="withCleanup"></param>
		/// <param name="errorCode"></param>
		/// <param name="errorDescription"></param>
		private async Task EndConversation(SqlConnection dbconnection, Guid conversationHandle, bool withCleanup, int? errorCode, string errorDescription)
		{
			_logger.LogInformation("Выполнение метода EndConversation(conversationHandle, withCleanup, errorCode, errorDescription)");
			try
			{
                await _manager.EndConversation(dbconnection, conversationHandle, withCleanup, errorCode, errorDescription);
            }
            catch(SqlException ex)
            {
                DBWrapperExceptionsHelper.ThrowError(ex, ServiceBrokerResources.EndConversationErrMsg);
            }
			catch (Exception ex)
			{
				throw new Exception(ServiceBrokerResources.EndConversationErrMsg, ex);
			}
		}

        /// <summary>
        /// Послать ответное сообщение об окончании выполнения шага
        /// </summary>
        /// <param name="conversationHandle"></param>
        public async Task SendStepCompletedMessage(SqlConnection dbconnection, Guid conversationHandle)
        {
            _logger.LogInformation("Выполнение метода SendStepCompletedMessage");
            try
            {

                await _manager.SendMessage(dbconnection, conversationHandle, SSSBMessage.PPS_StepCompleteMessageType, new byte[0]);
            }
            catch (SqlException ex)
            {
                DBWrapperExceptionsHelper.ThrowError(ex, ServiceBrokerResources.SendMessageErrMsg);
            }
            catch (Exception ex)
            {
                throw new Exception(ServiceBrokerResources.SendMessageErrMsg, ex);
            }
        }

		/// <summary>
		/// Завершение диалога.
		/// </summary>
		/// <param name="conversationHandle"></param>
		public Task EndConversation(SqlConnection dbconnection, Guid conversationHandle)
		{
			return EndConversation(dbconnection, conversationHandle, false, null, null);
		}

		/// <summary>
		/// Завершение диалога с опцией CLEANUP.
		/// </summary>
		/// <param name="conversationHandle"></param>
		public Task EndConversationWithCleanup(SqlConnection dbconnection, Guid conversationHandle)
		{
			return EndConversation(dbconnection, conversationHandle, true, null, null);
		}

		/// <summary>
		/// Завершение диалога с возвратом сообщения об ошибке.
		/// </summary>
		/// <param name="conversationHandle"></param>
		/// <param name="errorCode"></param>
		/// <param name="errorDescription"></param>
		public Task EndConversationWithError(SqlConnection dbconnection, Guid conversationHandle, int? errorCode, string errorDescription)
		{
			return EndConversation(dbconnection, conversationHandle, false, errorCode, errorDescription);
		}

		/// <summary>
		/// Отправка сообщения. Сообщение содержит информацию о типе, идентификатор диалога и пр. необходимую для отправки информацию.
		/// </summary>
		/// <param name="message"></param>
		public async Task SendMessage(SqlConnection dbconnection, SSSBMessage message)
		{
			_logger.LogInformation("Выполнение метода SendMessage(message)");
			try
			{

                await _manager.SendMessage(dbconnection, message.ConversationHandle, message.MessageType, message.Body);
			}
            catch (SqlException ex)
            {
                DBWrapperExceptionsHelper.ThrowError(ex, ServiceBrokerResources.SendMessageErrMsg);
            }
            catch (Exception ex)
            {
                throw new Exception(ServiceBrokerResources.SendMessageErrMsg, ex);
            }
		}


        /// <summary>
        /// Отправка отложенного сообщения. Сообщение содержит информацию о типе, идентификатор диалога и пр. необходимую для отправки информацию.
        /// </summary>
        /// <param name="fromService"></param>
        /// <param name="message"></param>
        /// <param name="lifetime"></param>
        /// <param name="isWithEncryption"></param>
        /// <param name="activationTime"></param>
        /// <param name="objectID"></param>
        public async Task<long> SendPendingMessage(SqlConnection dbconnection, string fromService, SSSBMessage message, TimeSpan lifetime, bool isWithEncryption, Guid? initiatorConversationGroupID, DateTime activationTime, string objectID)
		{
			_logger.LogInformation("Выполнение метода SendPendingMessage(..)");
			try
			{
                long? pendingMessageID = await _manager.SendPendingMessage(dbconnection, objectID, activationTime, fromService, message.ServiceName, message.ContractName, (int)lifetime.TotalSeconds, isWithEncryption, message.ConversationGroupID, message.ConversationHandle, message.Body, message.MessageType, initiatorConversationGroupID);
                return pendingMessageID.Value;
            }
            catch (SqlException ex)
            {
                DBWrapperExceptionsHelper.ThrowError(ex, ServiceBrokerResources.PendingMessageErrMsg);
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ServiceBrokerResources.PendingMessageErrMsg, ex);
            }
		}
		
		/// <summary>
		/// Возвращает название очереди сообщений для сервиса
		/// </summary>
		/// <param name="serviceName"></param>
		/// <returns></returns>
		public async Task<string> GetServiceQueueName(string serviceName)
		{
			_logger.LogInformation("Выполнение метода GetServiceQueueName(serviceName)");
			try
			{
                return await _manager.GetServiceQueueName(serviceName).ConfigureAwait(false);
            }
            catch (SqlException ex)
            {
                DBWrapperExceptionsHelper.ThrowError(ex, ServiceBrokerResources.GetServiceQueueNameErrMsg);
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ServiceBrokerResources.GetServiceQueueNameErrMsg, ex);
            }
		}
	}
}
