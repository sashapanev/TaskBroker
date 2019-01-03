using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Settings;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBroker.Services
{
    public class FileWriterService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly ILogger _logger;
        private readonly IOptions<AppSettings> _options;

        public FileWriterService(ILogger<FileWriterService> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _options = options;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(
                (e) => WriteTimeToFile(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        public void WriteTimeToFile()
        {
            string path = _options.Value.TestPath;
            _logger.LogInformation(path);

            using (var sw = File.AppendText(path))
            {
                sw.WriteLine(DateTime.UtcNow.ToString("O"));
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
