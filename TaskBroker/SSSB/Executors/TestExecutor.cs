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
        private string _clientContextID;

        public TestExecutor(ExecutorArgs args):
            base(args)
        {
        }

        protected override Task BeforeExecuteTask(CancellationToken token)
        {
            _category = this.Parameters["Category"];
            _batchId = this.Parameters["BatchID"];
            _clientContextID = this.Parameters["ClientContext"];
            return Task.CompletedTask;
        }

        protected override async Task<HandleMessageResult> DoExecuteTask(CancellationToken token)
        {
            this.Debug(string.Format("Executing SSSB Task: {0} Batch: {1} ClientContextID {2}", this.TaskInfo.OnDemandTaskID, _batchId, _clientContextID));
            if (_counter < 1)
            {
                Interlocked.Increment(ref _counter);
                this.Debug(string.Format("*** Defer SSSB Task: {0} Batch: {1} ClientContextID {2}", this.TaskInfo.OnDemandTaskID, _batchId, _clientContextID ));
                Guid initiatorConversationGroup = Guid.Parse(_clientContextID);
                // Execute on the same conversation
                // return this.Defer("PPS_OnDemandTaskService", DateTime.Now.AddSeconds(5), initiatorConversationGroup);
                
                // Execute on a new conversation: first execute EndDialog and then Defer
                return this.CombinedResult(this.EndDialog(), this.Defer("PPS_OnDemandTaskService", DateTime.Now.AddSeconds(5), null));
            }
            else
            {
                await Task.Delay(3000);
                this.Debug(string.Format("Executed  SSSB Task: {0} Batch: {1} ClientContextID {2}", this.TaskInfo.OnDemandTaskID, _batchId, _clientContextID));
                return this.EndDialog();
            }
        }

        protected override Task AfterExecuteTask(CancellationToken token)
        {
            //Interlocked.Increment(ref _counter);
            return Task.CompletedTask;
        }
    }
}
