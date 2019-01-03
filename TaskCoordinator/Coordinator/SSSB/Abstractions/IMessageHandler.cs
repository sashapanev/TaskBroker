using System.Threading.Tasks;

namespace TaskCoordinator.SSSB
{
    public interface IMessageHandler<T>
    {
        Task<T> HandleMessage(ISSSBService sender, T e);
    }
}
