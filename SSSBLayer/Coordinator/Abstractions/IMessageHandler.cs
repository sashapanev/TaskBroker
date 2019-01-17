using System.Threading.Tasks;

namespace Coordinator.SSSB
{
    public interface IMessageHandler<T>
    {
        Task<T> HandleMessage(ISSSBService sender, T e);
    }
}
