using System;
using System.Threading.Tasks;

namespace TaskBroker.SSSB.Scheduler
{
    public interface IScheduleManager
    {
        Task LoadSchedules();
        Task ReloadSchedule(int taskID);
        void UnLoadSchedule(int taskID);
        void UnLoadSchedules();
        Task<Guid> SendTimerEvent(int task_id);
        IServiceProvider RootServices { get; }
    }
}