using System;
using System.Collections.Specialized;
using TaskCoordinator.SSSB;

namespace TaskBroker.SSSB
{
    public class ExecutorArgs
    {
        public ExecutorArgs(OnDemandTaskManager tasksManager, TaskInfo taskInfo, DateTime eventDate, NameValueCollection parameters, bool isMultiStepTask, ServiceMessageEventArgs serviceMessageEventArgs)
        {
            this.TasksManager = tasksManager;
            this.TaskInfo = taskInfo;
            this.EventDate = eventDate;
            this.Parameters = parameters;
            this.IsMultiStepTask = isMultiStepTask;
            this.Message = serviceMessageEventArgs.Message;
        }
        public OnDemandTaskManager TasksManager { get; }
        public TaskInfo TaskInfo { get; }
        public DateTime EventDate { get; }
        public NameValueCollection Parameters { get; }
        public bool IsMultiStepTask { get; }
        public SSSBMessage Message { get; }
    }
}
