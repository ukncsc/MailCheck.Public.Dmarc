using System.Collections.Generic;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.SSM;
using MailCheck.Dmarc.Contracts.SharedDomain.Serialization;
using MailCheck.Dmarc.Entity.Config;
using MailCheck.Dmarc.Entity.Dao;
using MailCheck.Dmarc.Entity.Entity;
using MailCheck.Dmarc.Entity.Entity.DomainStatus;
using MailCheck.Dmarc.Entity.Entity.Notifiers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.Dmarc.Entity.StartUp
{
    internal class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings serializerSetting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };

                serializerSetting.Converters.Add(new StringEnumConverter());
                serializerSetting.Converters.Add(new TagConverter());

                return serializerSetting;
            };

            services
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddTransient<IEnvironment, EnvironmentWrapper>()
                .AddTransient<IEnvironmentVariables, EnvironmentVariables>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>() 
                .AddTransient<IDmarcEntityDao, DmarcEntityDao>()
                .AddTransient<IDmarcEntityConfig, DmarcEntityConfig>()
                .AddTransient<IChangeNotifiersComposite, ChangeNotifiersComposite>()
                .AddTransient<IChangeNotifier, RecordChangedNotifier>()
                .AddTransient<IChangeNotifier, AdvisoryChangedNotifier>()
                .AddTransient<IEqualityComparer<Contracts.SharedDomain.Message>, MessageEqualityComparer>()
                .AddTransient<IDomainStatusPublisher, DomainStatusPublisher>()
                .AddTransient<IDomainStatusEvaluator, DomainStatusEvaluator>()
                .AddTransient<DmarcEntity>();
        }
    }
}