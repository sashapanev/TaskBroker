using System;

namespace TaskCoordinator.SSSB
{
    public class ServiceOptions
    {
        public string Name { get; set; }

        public int MaxReadersCount { get; set; } = 1;

        public bool IsQueueActivationEnabled { get; set; } = false;

        public int MaxReadParallelism { get; set; } = 4;

        public Guid? ConversationGroup { get; set; } = null;
    }
}
