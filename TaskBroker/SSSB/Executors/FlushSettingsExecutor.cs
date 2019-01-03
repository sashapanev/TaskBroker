using System;
using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB;
using Microsoft.Extensions.Logging;

namespace TaskBroker.SSSB.Executors
{
    public class FlushSettingsExecutor : BaseExecutor
    {
        private readonly IScheduleManager _scheduleManager;
        private SettingsType _settingsType;
        private int _settingsID;

        public FlushSettingsExecutor(ExecutorArgs args, IScheduleManager scheduleManager) :
            base(args)
        {
            _scheduleManager = scheduleManager;
        }

        public override bool IsLongRunning
        {
            get
            {
                return false;
            }
        }

        protected override void BeforeExecuteTask()
        {
            _settingsType = (SettingsType)Enum.Parse(typeof(SettingsType), this.Parameters["settingsType"]);
            _settingsID = int.Parse(this.Parameters["settingsID"]);
        }

        protected override async Task<HandleMessageResult> DoExecuteTask(CancellationToken token)
        {
            OnDemandTaskManager.FlushTaskInfos();

            switch (_settingsType)
            {
                case SettingsType.None:
                    break;
                case SettingsType.OnDemandTaskSettings:
                    if (_settingsID < 0)
                        BaseExecutor.FlushStaticSettings();
                    else
                        BaseExecutor.FlushStaticSettings(_settingsID);
                    break;
                case SettingsType.Shedule:
                    if (_settingsID < 0)
                    {
                        _scheduleManager.UnLoadSchedules();
                        await _scheduleManager.LoadSchedules();
                    }
                    else
                    {
                        await _scheduleManager.ReloadSchedule(_settingsID);
                    }
                    break;
                default:
                    Logger.LogError(string.Format("UnKnown settingsType: {0}", _settingsType));
                    break;
            }

            return Noop();
        }
    }
}
