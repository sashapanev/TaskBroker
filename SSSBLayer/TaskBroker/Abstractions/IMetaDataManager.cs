using System.Threading;
using System.Threading.Tasks;
using Coordinator.SSSB.EF;

namespace TaskBroker.SSSB
{
    public interface IMetaDataManager
    {
        int MetaDataID { get; }
        Task<MetaData> GetMetaData(CancellationToken token = default(CancellationToken));

        CompletionResult IsAllTasksCompleted(MetaData metaData);
        Task<CompletionResult> SetCancelled();
        Task<CompletionResult> SetCompleted();
        Task<CompletionResult> SetCompletedWithError(string error);
        Task<CompletionResult> SetCompletedWithResult(string result);
    }
}