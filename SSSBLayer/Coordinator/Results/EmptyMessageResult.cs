using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Coordinator.SSSB.Utils;

namespace Coordinator.SSSB
{
    public class EmptyMessageResult : HandleMessageResult
    {
        private readonly IStandardMessageHandlers _standardMessageHandlers;
        private readonly Guid? _conversationHandle;

        public class Args
        {
            public Guid? conversationHandle { get; set; }
        }

        public EmptyMessageResult(IStandardMessageHandlers standardMessageHandlers, Args args)
        {
            _standardMessageHandlers = standardMessageHandlers;
            _conversationHandle = args.conversationHandle;
        }

        public override Task Execute(SqlConnection dbconnection, SSSBMessage message, CancellationToken token)
        {
            Guid conversationHandle = _conversationHandle.HasValue ? _conversationHandle.Value : message.ConversationHandle;
            return _standardMessageHandlers.SendEmptyMessage(dbconnection, conversationHandle);
        }
    }
}
