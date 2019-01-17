using Microsoft.Extensions.Logging;

namespace Coordinator.SSSB
{
    public class SSSBTasksCoordinator: BaseTasksCoordinator
    {
        public SSSBTasksCoordinator(ILoggerFactory loggerFactory, IMessageReaderFactory messageReaderFactory,
             int maxReadersCount, bool isQueueActivationEnabled = false, int maxReadParallelism = 4) :
             base(loggerFactory, messageReaderFactory, maxReadersCount, isQueueActivationEnabled, maxReadParallelism)
        {
        }
    }
}
