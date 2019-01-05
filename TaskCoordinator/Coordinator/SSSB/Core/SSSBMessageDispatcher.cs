using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using TaskCoordinator.Database;

namespace TaskCoordinator.SSSB
{
    public class SSSBMessageDispatcher : ISSSBMessageDispatcher
    {
        private readonly ILogger _logger;
        private readonly ISSSBService _sssbService;
        private readonly ConcurrentDictionary<string, IMessageHandler<ServiceMessageEventArgs>> _messageHandlers;
        private readonly ConcurrentDictionary<string, IMessageHandler<ErrorMessageEventArgs>> _errorMessageHandlers;
        private readonly IStandardMessageHandlers _standardMessageHandlers;
        private readonly IServiceProvider _services;

        #region  Constants
        public const int MAX_MESSAGE_ERROR_COUNT = 2;
        /// <summary>
        /// The system defined contract name for echo.
        /// </summary>
        private const string EchoContractName = "http://schemas.microsoft.com/SQL/ServiceBroker/ServiceEcho";
        #endregion

        public SSSBMessageDispatcher(ISSSBService sssbService, ILogger<SSSBMessageDispatcher> logger, IStandardMessageHandlers standardMessageHandlers, IServiceProvider services)
        {
            this._logger = logger;
            this._sssbService = sssbService;
            this._standardMessageHandlers = standardMessageHandlers;
            this._services = services;
            this._messageHandlers = new ConcurrentDictionary<string, IMessageHandler<ServiceMessageEventArgs>>();
            this._errorMessageHandlers = new ConcurrentDictionary<string, IMessageHandler<ErrorMessageEventArgs>>();
        }

        protected virtual ServiceMessageEventArgs CreateServiceMessageEventArgs(SSSBMessage message, CancellationToken cancellation)
        {
            ServiceMessageEventArgs args = new ServiceMessageEventArgs(message, this._sssbService, cancellation, _services.CreateScope());
            return args;
        }

        private async Task DispatchErrorMessage(SqlConnection dbconnection, SSSBMessage message, ErrorMessage msgerr, CancellationToken token)
        {
            try
            {
                // для каждого типа сообщения можно добавить нестандартную обработку 
                // которое не может быть обработано
                // например: сохранить тело сообщения в логе
                IMessageHandler<ErrorMessageEventArgs> errorMessageHandler;

                if (_errorMessageHandlers.TryGetValue(message.MessageType, out errorMessageHandler))
                {
                    using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                    {
                        ErrorMessageEventArgs errArgs = new ErrorMessageEventArgs(message, this._sssbService, msgerr.FirstError, token);
                        errArgs = await errorMessageHandler.HandleMessage(this._sssbService, errArgs).ConfigureAwait(continueOnCapturedContext: false);

                        transactionScope.Complete();
                    }
                }

                await _standardMessageHandlers.EndDialogMessageWithErrorHandler(dbconnection, message, msgerr.FirstError.Message, 4);

                string error = string.Format("Message {0} caused MAX Number of errors '{1}'. Dialog aborted!", message.MessageType, msgerr.FirstError.Message);
                _logger.LogError(error);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ErrorHelper.GetFullMessage(ex));
            }
        }

        private async Task<bool> _DispatchMessage(SqlConnection dbconnection, SSSBMessage message, CancellationToken token)
        {
            // возвратить ли сообщение назад в очередь?
            bool rollBack = false;

            IMessageHandler<ServiceMessageEventArgs> messageHandler;

            // if we registered custom handlers for predefined message types
            if (_messageHandlers.TryGetValue(message.MessageType, out messageHandler))
            {
                ServiceMessageEventArgs serviceArgs = this.CreateServiceMessageEventArgs(message, token);
                try
                {
                    bool isSync = true;
                    Task processTask = Task.FromException(new Exception($"The message: {message.MessageType} ConversationHandle: {message.ConversationHandle} is not handled"));
                    try
                    {
                        using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                        {
                            serviceArgs = await messageHandler.HandleMessage(this._sssbService, serviceArgs).ConfigureAwait(continueOnCapturedContext: false);
                            transactionScope.Complete();
                        }
                        isSync = serviceArgs.Completion.IsCompleted;
                    }
                    catch(Exception handleEx)
                    {
                        if (!serviceArgs.TaskCompletionSource.TrySetException(handleEx))
                        {
                            _logger.LogError(ErrorHelper.GetFullMessage(handleEx));
                        }
                    }
                    finally
                    {
                        processTask = this._HandleProcessingResult(dbconnection, message, token, serviceArgs, isSync);
                    }

                    if (isSync)
                    {
                        await processTask;
                    }
                }
                catch (Exception ex)
                {
                    if (!serviceArgs.TaskCompletionSource.TrySetException(ex))
                    {
                        _logger.LogError(ErrorHelper.GetFullMessage(ex));
                    }
                }
            }
            else if (message.MessageType == SSSBMessage.EndDialogMessageType)
            {
                await _standardMessageHandlers.EndDialogMessageHandler(dbconnection, message);
            }
            else if (message.MessageType == SSSBMessage.ErrorMessageType)
            {
                await _standardMessageHandlers.ErrorMessageHandler(dbconnection, message);
            }
            else if (message.MessageType == SSSBMessage.EchoMessageType && message.ContractName == EchoContractName)
            {
                await _standardMessageHandlers.EchoMessageHandler(dbconnection, message);
            }
            else if (message.MessageType == SSSBMessage.PPS_EmptyMessageType)
            {
                await _standardMessageHandlers.EndDialogMessageHandler(dbconnection, message);
            }
            else if (message.MessageType == SSSBMessage.PPS_StepCompleteMessageType)
            {
                //just awake from sleep
            }
            else
            {
                throw new Exception(string.Format(ServiceBrokerResources.UnknownMessageTypeErrMsg, message.MessageType));
            }

            return rollBack;
        }

        protected virtual async Task HandleAsyncProcessingResult(SSSBMessage message, CancellationToken token, Task<HandleMessageResult> completionTask)
        {
            token.ThrowIfCancellationRequested();
            var connectionManager = _services.GetRequiredService<IConnectionManager>();

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
            using (var dbconnection = await connectionManager.CreateSSSBConnectionAsync(token))
            {
                await HandleSyncProcessingResult(dbconnection, message, token, completionTask);

                transactionScope.Complete();
            }
        }

        protected virtual async Task HandleSyncProcessingResult(SqlConnection dbconnection, SSSBMessage message, CancellationToken token, Task<HandleMessageResult> completionTask)
        {
            HandleMessageResult handleMessageResult = null;
            try
            {
                handleMessageResult = await completionTask;

                await handleMessageResult.Execute(dbconnection, message, token);
            }
            catch (OperationCanceledException)
            {
                await _standardMessageHandlers.EndDialogMessageWithErrorHandler(dbconnection, message, $"Operation on Service: '{message.ServiceName}', MessageType: '{message.MessageType}', ConversationHandle: '{message.ConversationHandle}', is Cancelled", 1);
            }
            catch (PPSException ex)
            {
                await _standardMessageHandlers.EndDialogMessageWithErrorHandler(dbconnection, message,  $"Operation on Service: '{message.ServiceName}', MessageType: '{message.MessageType}', ConversationHandle: '{message.ConversationHandle}', ended with Error: {ex.Message}", 2);
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(new EventId(0, message.ServiceName), ErrorHelper.GetFullMessage(ex));
                }
                finally
                {
                    await _standardMessageHandlers.EndDialogMessageWithErrorHandler(dbconnection, message, $"Operation on Service: '{message.ServiceName}', MessageType: '{message.MessageType}', ConversationHandle: '{message.ConversationHandle}', ended with Error: {ex.Message}", 3);
                }
            }
        }

        Task _HandleProcessingResult(SqlConnection dbconnection, SSSBMessage message, CancellationToken token, ServiceMessageEventArgs serviceArgs, bool isSync)
        {
            Task processTask = serviceArgs.Completion.ContinueWith(async (antecedent) =>
            {
                try
                {
                    if (isSync)
                    {
                        await this.HandleSyncProcessingResult(dbconnection, message, token, antecedent);
                    }
                    else
                    {
                        await this.HandleAsyncProcessingResult(message, token, antecedent);
                    }
                }
                catch (OperationCanceledException)
                {
                    // NOOP
                }
                catch (PPSException)
                {
                    // Already Logged
                }
                catch (Exception ex)
                {
                    _logger.LogError(ErrorHelper.GetFullMessage(ex));
                }
            }, isSync? TaskContinuationOptions.ExecuteSynchronously: TaskContinuationOptions.None).Unwrap();

            var disposeTask = processTask.ContinueWith((antecedent) =>
            {
                try
                {
                    serviceArgs.Dispose();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ErrorHelper.GetFullMessage(ex));
                }
            }, TaskContinuationOptions.ExecuteSynchronously);

            return processTask;
        }

        async Task<MessageProcessingResult> IMessageDispatcher<SSSBMessage, SqlConnection>.DispatchMessage(SSSBMessage message, long taskId, CancellationToken token, SqlConnection dbconnection)
        {
            bool rollBack = false;

            ErrorMessage msgerr = null;
            bool end_dialog_with_error = false;
            //определяем сообщение по ConversationHandle
            if (message.ConversationHandle.HasValue)
            {
                // оканчивалась ли ранее обработка этого сообщения с ошибкой?
                msgerr = _sssbService.GetError(message.ConversationHandle.Value);
                if (msgerr != null)
                    end_dialog_with_error = msgerr.ErrorCount >= MAX_MESSAGE_ERROR_COUNT;
            }
            if (end_dialog_with_error)
                await this.DispatchErrorMessage(dbconnection, message, msgerr, token).ConfigureAwait(continueOnCapturedContext: false);
            else
                rollBack = await this._DispatchMessage(dbconnection, message, token).ConfigureAwait(continueOnCapturedContext: false);

            return new MessageProcessingResult() { isRollBack = rollBack };
        }

        public void RegisterMessageHandler(string messageType, IMessageHandler<ServiceMessageEventArgs> handler)
        {
            _messageHandlers[messageType] = handler;
        }

        public void RegisterErrorMessageHandler(string messageType, IMessageHandler<ErrorMessageEventArgs> handler)
        {
            _errorMessageHandlers[messageType] = handler;
        }

        public void UnregisterMessageHandler(string messageType)
        {
            _messageHandlers.TryRemove(messageType, out var _);
        }

        public void UnregisterErrorMessageHandler(string messageType)
        {
            _errorMessageHandlers.TryRemove(messageType, out var _);
        }
    }
}
