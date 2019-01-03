using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskCoordinator.SSSB;

namespace TaskBroker.SSSB
{
    public class TaskEndedMessageHandler : BaseMessageHandler<ServiceMessageEventArgs>
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _rootServices;

        public TaskEndedMessageHandler(IServiceProvider rootProvider)
        {
            _logger = rootProvider.GetRequiredService<ILogger<TaskEndedMessageHandler>>();
            _rootServices = rootProvider;
        }

        protected override string GetName()
        {
            return nameof(TaskEndedMessageHandler);
        }

        public override Task<ServiceMessageEventArgs> HandleMessage(ISSSBService sender, ServiceMessageEventArgs args)
        {
            try
            {
                // после выполнения задачи снова включить таймер
                lock (ScheduleTimer.Timers)
                {
                    Guid conversationGroup = args.Message.ConversationGroupID.Value;
                    var timer = (from t in BaseSheduleTimer.Timers.Values
                                 where t.ConversationGroup.HasValue && t.ConversationGroup.Value == conversationGroup
                                 select t).SingleOrDefault();

                    if (timer != null)
                    {
                        timer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorHelper.GetFullMessage(ex));
            }
            finally
            {
                var res = (HandleMessageResult)this._rootServices.GetRequiredService<EndDialogMessageResult>();
                args.TaskCompletionSource.SetResult(res);
            }
            return Task.FromResult(args);
        }
    }
}
