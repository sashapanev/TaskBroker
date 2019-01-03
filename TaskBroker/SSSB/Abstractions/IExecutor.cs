using System;
using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB;

namespace TaskBroker.SSSB
{
    public interface IExecutor
    {
        Guid ID
        {
            get;
        }

        string Name
        {
            get;
        }

        bool IsLongRunning
        {
            get;
        }

        Task<HandleMessageResult> ExecuteTaskAsync(CancellationToken token);
    }
}