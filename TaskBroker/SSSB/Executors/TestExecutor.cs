using System;
using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB;

namespace TaskBroker.SSSB.Executors
{
    public class TestExecutor : BaseExecutor
    {
        private static int _counter = 0;
        private string _batchId;
        private string _category;
        private string _clientContext;

        public TestExecutor(ExecutorArgs args):
            base(args)
        {
        }

        public override bool IsAsyncProcessing
        {
            get
            {
                return false;
            }
        }

        protected override void BeforeExecuteTask()
        {
            _category = this.Parameters["Category"];
            _batchId = this.Parameters["BatchID"];
            _clientContext = this.Parameters["ClientContext"];
        }

        protected override async Task<HandleMessageResult> DoExecuteTask(CancellationToken token)
        {
            this.Debug(string.Format("Executing SSSB Task: {0} Batch: {1} Guid: {2}", this.TaskInfo.OnDemandTaskID, _batchId, _clientContext));
            if (_counter < 1)
            {
                Interlocked.Increment(ref _counter);
                this.Debug(string.Format("*** Defer SSSB Task: {0} Batch: {1} Guid: {2}", this.TaskInfo.OnDemandTaskID, _batchId, _clientContext ));
                Guid initiatorConversationGroup = Guid.Parse(_clientContext);
                return Defer("PPS_OnDemandTaskService", DateTime.Now.AddSeconds(5), initiatorConversationGroup);
            }
            else
            {
                await Task.Delay(3000);
                this.Debug(string.Format("Executed  SSSB Task: {0} Batch: {1} Guid: {2}", this.TaskInfo.OnDemandTaskID, _batchId, _clientContext));
                return EndDialog();
            }
        }

        protected override void AfterExecuteTask()
        {
            //Interlocked.Increment(ref _counter);
        }
    }
}
