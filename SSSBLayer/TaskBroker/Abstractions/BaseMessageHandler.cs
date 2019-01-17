using System.Threading.Tasks;
using Coordinator.SSSB;

namespace TaskBroker.SSSB
{
    public abstract class BaseMessageHandler<T>: IMessageHandler<T>
    {
        protected object SyncRoot = new object();

        protected virtual string GetName()
        {
            return nameof(BaseMessageHandler<T>);
        }

        public abstract Task<T> HandleMessage(ISSSBService sender, T args);
    }
}
