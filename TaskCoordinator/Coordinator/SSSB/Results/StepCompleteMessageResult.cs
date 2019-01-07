using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB.Utils;

namespace TaskCoordinator.SSSB
{
    public class StepCompleteMessageResult : HandleMessageResult
    {
        private readonly IStandardMessageHandlers _standardMessageHandlers;
        private readonly Guid? _conversationHandle;

        public class Args
        {
            public Guid? conversationHandle { get; set; }
        }

        public StepCompleteMessageResult(IStandardMessageHandlers standardMessageHandlers, Args args)
        {
            _standardMessageHandlers = standardMessageHandlers;
            _conversationHandle = args.conversationHandle;
        }

        public override Task Execute(SqlConnection dbconnection, SSSBMessage message, CancellationToken token)
        {
            Guid conversationHandle = _conversationHandle.HasValue ? _conversationHandle.Value : message.ConversationHandle;
            return _standardMessageHandlers.SendStepCompleted(dbconnection, conversationHandle);
        }
    }
}
