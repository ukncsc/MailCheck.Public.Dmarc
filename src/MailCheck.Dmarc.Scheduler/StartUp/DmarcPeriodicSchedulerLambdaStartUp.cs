using MailCheck.Common.Data;
using MailCheck.Common.Environment.FeatureManagement;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Sns;
using MailCheck.Common.Util;
using MailCheck.Dmarc.Scheduler.Config;
using MailCheck.Dmarc.Scheduler.Dao;
using MailCheck.Dmarc.Scheduler.Processor;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.Dmarc.Scheduler.StartUp
{
    internal class DmarcPeriodicSchedulerLambdaStartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            DmarcSchedulerCommonStartUp.ConfigureCommonServices(services);

            services
                .AddSingleton<IDatabase, DefaultDatabase<MySqlProvider>>()
                .AddTransient<IDmarcPeriodicSchedulerConfig, DmarcPeriodicSchedulerConfig>()
                .AddTransient<IProcess, DmarcPollSchedulerProcessor>()
                .AddTransient<IMessagePublisher, SnsMessagePublisher>()
                .AddTransient<IClock, Clock>()
                .AddTransient<IDmarcPeriodicSchedulerDao, DmarcPeriodicSchedulerDao>();
        }
    }
}
