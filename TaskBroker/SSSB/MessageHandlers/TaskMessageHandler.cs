using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Xml.Linq;
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

        public override async Task<ServiceMessageEventArgs> HandleMessage(ISSSBService sender, ServiceMessageEventArgs args)
        {
            int? taskID = null;
            bool isMultiStepTask = false;
            DateTime eventDate = DateTime.Now;
            XElement message_xml = null;
            NameValueCollection parameters = null;

            try
            {
                args.Token.ThrowIfCancellationRequested();
                message_xml = GetMessageXML(args.Message.Body);
                GetMessageAttributes(message_xml, out taskID, out isMultiStepTask, out eventDate, out parameters);
            }
            catch (OperationCanceledException)
            {
                args.TaskCompletionSource.TrySetCanceled(args.Token);
                return args;
            }
            catch (Exception ex)
            {
                args.TaskCompletionSource.TrySetException(ex);
                this._logger.LogError(ErrorHelper.GetFullMessage(ex));
                return args;
            }

            OnDemandTaskManager taskManager = args.Services.GetRequiredService<OnDemandTaskManager>();
            try
            {
                args.Token.ThrowIfCancellationRequested();
                var task = await taskManager.GetTaskInfo(taskID.Value);
                args.TaskID = taskID.Value;
                var execArgs = new ExecutorArgs(taskManager, task, eventDate, parameters, isMultiStepTask);
                await ExecuteTask(execArgs, args);
            }
            catch (OperationCanceledException)
            {
                args.TaskCompletionSource.TrySetCanceled(args.Token);
            }
            catch (Exception ex)
            {
                if (!args.TaskCompletionSource.TrySetException(ex))
                {
                    _logger.LogCritical(ErrorHelper.GetFullMessage(ex));
                }
            }

            return args;
        }

        protected virtual async Task ExecuteTask(ExecutorArgs executorArgs, ServiceMessageEventArgs args)
        {
            try
            {
                var executor = (IExecutor)ActivatorUtilities.CreateInstance(args.Services, executorArgs.TaskInfo.ExecutorType, new object[] { executorArgs });
                executorArgs.TasksManager.CurrentExecutor = executor;
                Task<HandleMessageResult> execResTask = executor.ExecuteTaskAsync(args.Token);
                if (executor.IsLongRunning && !execResTask.IsCompleted)
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
            catch (Exception ex)
            {
                if (!args.TaskCompletionSource.TrySetException(ex))
                {
                    _logger.LogError(ErrorHelper.GetFullMessage(ex));
                }
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
                catch (Exception ex)
                {
                    if (!serviceArgs.TaskCompletionSource.TrySetException(ex))
                    {
                        _logger.LogError(ErrorHelper.GetFullMessage(ex));
                    }
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
