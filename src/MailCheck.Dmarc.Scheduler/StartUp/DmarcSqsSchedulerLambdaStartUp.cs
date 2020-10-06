using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Scheduler.Config;
using MailCheck.Dmarc.Scheduler.Dao;
using MailCheck.Dmarc.Scheduler.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.Dmarc.Scheduler.StartUp
{
    internal class DmarcSqsSchedulerLambdaStartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            DmarcSchedulerCommonStartUp.ConfigureCommonServices(services);

            services
                .AddTransient<IDmarcSchedulerConfig, DmarcSchedulerConfig>()
                .AddTransient<DmarcSchedulerHandler>()
                .AddTransient<IDmarcSchedulerDao, DmarcSchedulerDao>();
        }
    }
}
