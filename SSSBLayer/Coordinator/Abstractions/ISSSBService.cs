using Shared.Services;
using System;

namespace Coordinator.SSSB
{
    public interface ISSSBService : ITaskService
    {
        string QueueName
        {
            get;
        }

        int MaxReadersCount
        {
            get;
        }

        int MaxReadParallelism
        {
            get;
        }

        Guid? ConversationGroup
        {
            get;
        }

        ErrorMessage GetError(Guid messageID);
        int AddError(Guid messageID, Exception err);
    }
}
