using System.Threading;
using System.Threading.Tasks;

namespace Coordinator
{
    public interface ITaskCoordinator
    {
        bool Start(CancellationToken token);
        Task Stop();
        bool IsPaused { get; set; }
        int MaxTasksCount { get; set; }
        int TasksCount { get; }
        CancellationToken Token { get; }
        bool IsQueueActivationEnabled { get; }
    }
}
