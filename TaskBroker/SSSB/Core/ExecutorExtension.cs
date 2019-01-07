using System;
using TaskBroker.SSSB.Executors;
using TaskCoordinator.SSSB;

namespace TaskBroker.SSSB
{
    public static class ExecutorExtension
    {
        public static HandleMessageResult Noop(this BaseExecutor executor)
        {
            return executor.Services.Noop();
        }

        public static HandleMessageResult EndDialogWithError(this BaseExecutor executor, string error, int? errocode, Guid? conversationHandle = null)
        {
            return executor.Services.EndDialogWithError(error, errocode, conversationHandle);
        }

        public static HandleMessageResult EndDialog(this BaseExecutor executor, Guid? conversationHandle = null)
        {
            return executor.Services.EndDialog(conversationHandle);
        }

        public static HandleMessageResult Defer(this BaseExecutor executor, string fromService, DateTime activationTime, Guid? initiatorConversationGroupID = null, TimeSpan? lifeTime = null)
        {
            return executor.Services.Defer(fromService, activationTime, initiatorConversationGroupID, lifeTime);
        }

        public static HandleMessageResult StepCompleted(this BaseExecutor executor, Guid? conversationHandle = null)
        {
            return executor.Services.StepCompleted(conversationHandle);
        }

        public static HandleMessageResult EmptyMessage(this BaseExecutor executor, Guid? conversationHandle = null)
        {
            return executor.Services.EmptyMessage(conversationHandle);
        }

        public static HandleMessageResult CombinedResult(this BaseExecutor executor, params HandleMessageResult[] resultHandlers)
        {
            return executor.Services.CombinedResult(resultHandlers);
        }
    }
}
