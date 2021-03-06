﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskBroker.Services;
using TaskBroker.SSSB.Services;
using Coordinator.SSSB;
using Coordinator.SSSB.EF;

namespace TaskBroker.SSSB
{
    public static class OnDemandTaskServiceExtensions
    {
        public static void AddOnDemandTaskService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SSSBDbContext>((dbOptions) => {
                string connectionString = configuration.GetConnectionString("DBConnectionStringSSSB");
                dbOptions.UseSqlServer(connectionString, (sqlOptions) => {
                    // sqlOptions.UseRowNumberForPaging();
                });
            });
            services.AddSSSBService();
            services.TryAddSingleton<OnDemandTaskSSSBService>();
            services.TryAddTransient<OnDemandTaskManager>();
            services.AddHostedService<OnDemandTaskService>();
        }
    }
}
