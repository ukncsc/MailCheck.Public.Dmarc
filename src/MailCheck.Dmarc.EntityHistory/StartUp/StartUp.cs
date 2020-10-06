using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.SharedDomain.Serialization;
using MailCheck.Dmarc.EntityHistory.Config;
using MailCheck.Dmarc.EntityHistory.Dao;
using MailCheck.Dmarc.EntityHistory.Entity;
using MailCheck.Dmarc.EntityHistory.Service;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.Dmarc.EntityHistory.StartUp
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
                .AddTransient<IDmarcEntityHistoryConfig, DmarcEntityHistoryConfig>()
                .AddTransient<IDmarcRuaValidator, DmarcRuaValidator>()
                .AddTransient<IDmarcRuaService, DmarcRuaService>()
                .AddTransient<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<IDmarcEntityHistoryDao, DmarcEntityHistoryDao>()
                .AddTransient<DmarcEntityHistory>();
        }
    }
}
