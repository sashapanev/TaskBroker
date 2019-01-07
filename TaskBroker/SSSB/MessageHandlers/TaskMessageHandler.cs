using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Xml.Linq;
using TaskBroker.SSSB.Executors;
using TaskCoordinator.SSSB;

namespace TaskBroker.SSSB
{
    public class TaskMessageHandler : BaseMessageHandler<ServiceMessageEventArgs>
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _rootServices;

        public TaskMessageHandler(IServiceProvider rootProvider)
        {
            _rootServices = rootProvider;
            _logger = rootProvider.GetRequiredService<ILogger<TaskMessageHandler>>();
        }

        protected override string GetName()
        {
            return nameof(TaskMessageHandler);
        }

        public override async Task<ServiceMessageEventArgs> HandleMessage(ISSSBService sender, ServiceMessageEventArgs serviceMessageArgs)
        {
            int? taskID = null;
            bool isMultiStepTask = false;
            DateTime eventDate = DateTime.Now;
            XElement message_xml = null;
            NameValueCollection parameters = null;

            try
            {
                serviceMessageArgs.Token.ThrowIfCancellationRequested();
                message_xml = GetMessageXML(serviceMessageArgs.Message.Body);
                GetMessageAttributes(message_xml, out taskID, out isMultiStepTask, out eventDate, out parameters);
            }
            catch (OperationCanceledException)
            {
                serviceMessageArgs.TaskCompletionSource.TrySetCanceled(serviceMessageArgs.Token);
                return serviceMessageArgs;
            }
            catch (PPSException ex)
            {
                serviceMessageArgs.TaskCompletionSource.TrySetException(ex);
                return serviceMessageArgs;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ErrorHelper.GetFullMessage(ex));
                serviceMessageArgs.TaskCompletionSource.TrySetException(new PPSException(ex));
                return serviceMessageArgs;
            }

            OnDemandTaskManager taskManager = serviceMessageArgs.Services.GetRequiredService<OnDemandTaskManager>();
            try
            {
                serviceMessageArgs.Token.ThrowIfCancellationRequested();
                var task = await taskManager.GetTaskInfo(taskID.Value);
                serviceMessageArgs.TaskID = taskID.Value;
                var executorArgs = new ExecutorArgs(taskManager, task, eventDate, parameters, isMultiStepTask, serviceMessageArgs);
                await ExecuteTask(executorArgs, serviceMessageArgs);
            }
            catch (OperationCanceledException)
            {
                serviceMessageArgs.TaskCompletionSource.TrySetCanceled(serviceMessageArgs.Token);
            }
            catch (PPSException ex)
            {
                serviceMessageArgs.TaskCompletionSource.TrySetException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ErrorHelper.GetFullMessage(ex));
                serviceMessageArgs.TaskCompletionSource.TrySetException(new PPSException(ex));
            }

            return serviceMessageArgs;
        }

        protected virtual async Task ExecuteTask(ExecutorArgs executorArgs, ServiceMessageEventArgs args)
        {
            try
            {
                var executor = (IExecutor)ActivatorUtilities.CreateInstance(args.Services, executorArgs.TaskInfo.ExecutorType, new object[] { executorArgs });
                executorArgs.TasksManager.CurrentExecutor = executor;
                Task<HandleMessageResult> execResTask = executor.ExecuteTaskAsync(args.Token);
                if (executor.IsAsyncProcessing && !execResTask.IsCompleted)
                {
                    this.ExecuteLongRun(execResTask, executorArgs, args);
                }
                else
                {
                    var res = await execResTask;
                    args.TaskCompletionSource.TrySetResult(res);
                }
            }
            catch (OperationCanceledException)
            {
                args.TaskCompletionSource.TrySetCanceled(args.Token);
            }
            catch (PPSException ex)
            {
                args.TaskCompletionSource.TrySetException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorHelper.GetFullMessage(ex));
                args.TaskCompletionSource.TrySetException(new PPSException(ex));
            }
        }

        protected virtual void ExecuteLongRun(Task<HandleMessageResult> execResTask, ExecutorArgs executorArgs, ServiceMessageEventArgs serviceArgs)
        {
            var continuationTask = execResTask.ContinueWith((antecedent) =>
            {
                try
                {
                    if (antecedent.IsFaulted)
                    {
                        antecedent.Exception.Flatten().Handle((err) =>
                        {
                            serviceArgs.TaskCompletionSource.TrySetException(err);
                            return true;
                        });
                    }
                    else if (antecedent.IsCanceled)
                    {
                        serviceArgs.TaskCompletionSource.TrySetCanceled(serviceArgs.Token);
                    }
                    else
                    {
                        serviceArgs.TaskCompletionSource.TrySetResult(antecedent.Result);
                    }
                }
                catch (OperationCanceledException)
                {
                    serviceArgs.TaskCompletionSource.TrySetCanceled(serviceArgs.Token);
                }
                catch (PPSException ex)
                {
                    serviceArgs.TaskCompletionSource.TrySetException(ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ErrorHelper.GetFullMessage(ex));
                    serviceArgs.TaskCompletionSource.TrySetException(new PPSException(ex));
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
