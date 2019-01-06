using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Settings;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaskBroker.Services;
using TaskBroker.SSSB;

namespace TaskBroker
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            var builder = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("hostsettings.json", optional: false);
                    configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Directory.GetCurrentDirectory());
                    configApp.AddJsonFile("appsettings.json", optional: false);
                    configApp.AddJsonFile(
                        $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                    configApp.AddEnvironmentVariables(prefix: "PREFIX_");
                    configApp.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        // Development service configuration
                    }
                    else
                    {
                        // Non-development service configuration
                    }

                    services.Configure<HostOptions>(option =>
                    {
                        option.ShutdownTimeout = System.TimeSpan.FromSeconds(30);
                    });

                    var configuration = context.Configuration;

                    services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

                    // services.AddHostedService<FileWriterService>();

                    services.AddOnDemandEventService(configuration);
                    services.AddOnDemandTaskService(configuration);
                    services.AddPubSubService(configuration);
                })
                .ConfigureLogging((context, logBuilder) =>
                {
                    logBuilder.ClearProviders();
                    logBuilder.AddConsole();
                    /*
                    logBuilder.AddFile(opts =>
                    {
                        context.Configuration.GetSection("FileLoggingOptions").Bind(opts);
                    });
                    */
                    /*
                    logBuilder.AddFile(opts =>
                    {
                    opts.FileName = "app-logs-";
                    opts.FileSizeLimit = 4 * 1024 * 1024;
                    opts.RetainedFileCountLimit = 10;
                    opts.BatchSize = 64;
                    opts.FlushPeriod = TimeSpan.FromSeconds(2);
                    });
                    */

                });

            if (isService)
            {
                await builder.RunAsServiceAsync();
            }
            else
            {
                await builder.RunConsoleAsync();
            }
        }
    }
}