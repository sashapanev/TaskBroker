using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskCoordinator.Database;
using TaskCoordinator.SSSB.Utils;

namespace TaskCoordinator.SSSB
{
    public static class AddBaseSSSBServiceExtensions
    {
        public static void AddSSSBService(this IServiceCollection services)
        {
            services.TryAddTransient<IConnectionErrorHandler, ConnectionErrorHandler>();
            services.TryAddTransient(typeof(IDependencyResolver<,>), typeof(DependencyResolver<,>));
            services.TryAddSingleton<IErrorMessages, ErrorMessages>();
            services.TryAddSingleton<IConnectionManager, ConnectionManager>();
            services.TryAddSingleton<ISSSBManager, SSSBManager>();
            services.TryAddSingleton<IServiceBrokerHelper, ServiceBrokerHelper>();
            services.TryAddSingleton<IStandardMessageHandlers, StandardMessageHandlers>();

            services.TryAddSingleton<NoopMessageResult>();
        }
    }
}
