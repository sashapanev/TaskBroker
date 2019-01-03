using System.Threading.Tasks;

namespace TaskCoordinator
{
    public interface ICallbackProxy<T>
    {
        BatchInfo BatchInfo { get; }
        Task TaskCompleted(T message, string error);
    }
}
