using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB.EF;

namespace TaskBroker.SSSB
{
    public interface IMetaDataManager
    {
        int MetaDataID { get; }
        MetaData MetaData { get; }
        Task<MetaData> GetMetaData(CancellationToken token = default(CancellationToken));

        Task<CompletionResult> IsAllTasksCompleted(CancellationToken token = default(CancellationToken));
        Task<bool> IsCanceled(CancellationToken token = default(CancellationToken));
        Task SetCancelled();
        Task SetCompleted();
        Task SetCompletedWithError(string error);
        Task SetCompletedWithResult(string result);
    }
}