using System;
using System.Collections.Specialized;

namespace TaskBroker.SSSB
{
    public class ExecutorArgs
    {
        public ExecutorArgs(OnDemandTaskManager tasksManager, TaskInfo taskInfo, DateTime eventDate, NameValueCollection parameters, bool isMultiStepTask)
        {
            this.TasksManager = tasksManager;
            this.TaskInfo = taskInfo;
            this.EventDate = eventDate;
            this.Parameters = parameters;
            this.IsMultiStepTask = isMultiStepTask;
        }
        public OnDemandTaskManager TasksManager { get; }
        public TaskInfo TaskInfo { get; }
        public DateTime EventDate { get; }
        public NameValueCollection Parameters { get; }
        public bool IsMultiStepTask { get; }
    }
}
