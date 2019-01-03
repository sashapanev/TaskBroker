using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Settings;
using System.Threading;
using System.Threading.Tasks;
using TaskBroker.SSSB.Services;

namespace TaskBroker.Services
{
    public class PubSubService : IHostedService
    {
        private readonly IOptions<AppSettings> _options;
        private readonly PubSubSSSBService _sssbService;

        public PubSubService(PubSubSSSBService sssbService, IOptions<AppSettings> options)
        {
            _sssbService = sssbService;
            _options = options;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _sssbService.Start();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _sssbService.Stop();
        }
    }
}
