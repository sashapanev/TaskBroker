using Microsoft.Extensions.DependencyInjection;
using System;

namespace TaskCoordinator.SSSB
{
    public static class ServiceProviderExtension
    {
        public static HandleMessageResult Noop(this IServiceProvider Services)
        {
            var res = (HandleMessageResult)Services.GetRequiredService<NoopMessageResult>();
            return res;
        }

        public static HandleMessageResult EndDialogWithError(this IServiceProvider Services, string error, int? errocode, Guid? conversationHandle = null)
        {
            EndDialogMessageResult.Args args = new EndDialogMessageResult.Args()
            {
                error = error,
                errorCode = errocode,
                conversationHandle = conversationHandle
            };
            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<EndDialogMessageResult>(Services, new object[] { args });
            return res;
        }

        public static HandleMessageResult EndDialog(this IServiceProvider Services, Guid? conversationHandle = null)
        {
            EndDialogMessageResult.Args args = new EndDialogMessageResult.Args()
            {
                conversationHandle = conversationHandle
            };
            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<EndDialogMessageResult>(Services, new object[] { args });
            return res;
        }

        public static HandleMessageResult Defer(this IServiceProvider Services, string fromService, DateTime activationTime, int attemptNumber = 1, TimeSpan? lifeTime = null)
        {
            if (string.IsNullOrEmpty(fromService))
                throw new ArgumentNullException(nameof(fromService));

            DeferMessageResult.Args args = new DeferMessageResult.Args()
            {
                fromService = fromService,
                activationTime = activationTime,
                attemptNumber = attemptNumber,
                lifeTime = lifeTime
            };

            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<DeferMessageResult>(Services, new object[] { args });
            return res;
        }

        public static HandleMessageResult StepCompleted(this IServiceProvider Services, Guid? conversationHandle = null)
        {
            StepCompleteMessageResult.Args args = new StepCompleteMessageResult.Args()
            {
                conversationHandle = conversationHandle
            };
            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<StepCompleteMessageResult>(Services, new object[] { args });
            return res;
        }

        public static HandleMessageResult EmptyMessage(this IServiceProvider Services, Guid? conversationHandle = null)
        {
            EmptyMessageResult.Args args = new EmptyMessageResult.Args()
            {
                conversationHandle = conversationHandle
            };
            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<EmptyMessageResult>(Services, new object[] { args });
            return res;
        }

        public static HandleMessageResult CombinedResult(this IServiceProvider Services, params HandleMessageResult[] resultHandlers)
        {
            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<CombinedMessageResult>(Services, new object[] { resultHandlers });
            return res;
        }
    }
}
