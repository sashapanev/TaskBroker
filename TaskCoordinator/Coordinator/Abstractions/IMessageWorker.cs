using System.Threading.Tasks;

namespace Coordinator
{
    public interface IMessageWorker<in M>
    {
        bool OnBeforeDoWork(IMessageReader reader);
        Task<MessageProcessingResult> OnDoWork(M message, object state, int taskId);
        void OnAfterDoWork(IMessageReader reader);
    }
}
