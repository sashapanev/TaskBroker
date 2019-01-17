using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskBroker.SSSB.Scheduler
{
    public abstract class BaseSheduleTimer : IDisposable
    {
        public const string SHEDULE_EVENT_CONTRACT = "PPS_SheduleEventContract";
        public static Dictionary<int, BaseSheduleTimer> Timers = new Dictionary<int, BaseSheduleTimer>();

        private int _taskID;
        private Guid? _convesationGroup;
        private TimeSpan _lazyWaitTime = TimeSpan.FromSeconds(0);
        private readonly IScheduleManager _scheduleManager;

        public BaseSheduleTimer(IScheduleManager scheduleManager, int taskID)
        {
            this._scheduleManager = scheduleManager;
            // каждый таймер содержит id задачи которую потребуется выполнить когда таймер сработает
            this._taskID = taskID;
        }

        
        protected virtual async Task onTimerElapsed()
        {
            await this.SendEventMessage();
            // останавливаем до окончания выполнения задачи
            this.Stop();
        }

        protected virtual async Task SendEventMessage()
        {
            this._convesationGroup = null;
            this._convesationGroup = await this._scheduleManager.SendTimerEvent(this._taskID);
        }

        public abstract void Start();

        public abstract void Stop();

        public abstract bool Enabled
        {
            get;
            protected set;
        }

        public int TaskID
        {
            get
            {
                return this._taskID;
            }
        }

        public Guid? ConversationGroup
        {
            get
            {
                return this._convesationGroup;
            }
        }

        /// <summary>
        /// Время прошедшее с момента запуска задачи
        /// до получения сообщения об окончании задачи
        /// </summary>
        public TimeSpan LazyWaitTime
        {
            get { return _lazyWaitTime; }
            protected set { _lazyWaitTime = value; }
        }

        protected virtual void OnDispose()
        {
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.OnDispose();
        }

        #endregion
    }
}
