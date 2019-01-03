using System.Threading;
using System.Threading.Tasks;

namespace TaskCoordinator
{
    public interface IMessageDispatcher<TMessage, TState>
    {
        Task<MessageProcessingResult> DispatchMessage(TMessage message, long taskId, CancellationToken token, TState state);
    }
}
