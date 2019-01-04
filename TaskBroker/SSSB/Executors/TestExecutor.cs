using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB;

namespace TaskBroker.SSSB.Executors
{
    public class TestExecutor : BaseExecutor
    {
        public TestExecutor(ExecutorArgs args):
            base(args)
        {
        }

        public override bool IsLongRunning
        {
            get
            {
                return true;
            }
        }

        protected override void BeforeExecuteTask()
        {
            string category = this.Parameters["Category"];
            string batchId = this.Parameters["BatchID"];
            string clientContext = this.Parameters["ClientContext"];
        }

        protected override async Task<HandleMessageResult> DoExecuteTask(CancellationToken token)
        {
            // this.Debug(string.Format("Executing SSSB Task: {0}", this.TaskInfo.OnDemandTaskID.ToString()));
            await Task.Delay(3000);
            this.Debug(string.Format("Executed SSSB Task: {0}", this.TaskInfo.OnDemandTaskID.ToString()));
            return EndDialog();
        }
    }
}
