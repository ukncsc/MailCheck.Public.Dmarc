using System;
using Amazon.SimpleSystemsManagement;
using MailCheck.AggregateReport.Client.Config;
using MailCheck.Common.Api.AuthProviders;
using MailCheck.Common.Api.Service;
using MailCheck.Common.Environment;
using MailCheck.Common.SSM;
using MailCheck.Common.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MailCheck.AggregateReport.Client
{
    public static class AggregateReportClientExtensionMethods
    {
        public static IServiceCollection AddAggregateReportClaimsPrincipleClient(this IServiceCollection serviceContainer)
        {
            return serviceContainer
                .AddEnvironment()
                .AddTransient<IAggregateReportClientConfig, AggregateReportClientConfig>()
                .AddTransient<IAggregateReportClient, AggregateReportClient>()
                .AddHttpContextAccessor()
                .AddTransient<IAuthenticationHeaderProvider, ClaimsPrincipalAuthenticationHeaderProvider>();
        }

        public static IServiceCollection AddAggregateReportApiKeyClient(this IServiceCollection serviceContainer)
        {
            return serviceContainer
                .AddEnvironment()
                .AddTransient<IAggregateReportClientConfig, AggregateReportClientConfig>()
                .AddTransient<IAggregateReportClient>(ApiKeyClientFactory)
                .AddTransient<IAggregateReportApiKeyConfig, AggregateReportApiKeyConfig>()
                .TryAddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .TryAddTransient<IApiKeyProvider, ApiKeyProvider>();
        }

        //use factory to prevent ambiguous reference to IAuthenticationHeaderProvider as each client
        //requires a different value for api key name, if multiple of these are reg'd could cause problems
        private static AggregateReportClient ApiKeyClientFactory(IServiceProvider serviceProvider)
        {
            IAggregateReportClientConfig clientConfig = serviceProvider.GetRequiredService<IAggregateReportClientConfig>();
            ILogger<AggregateReportClient> logger = serviceProvider.GetRequiredService<ILogger<AggregateReportClient>>();

            IAggregateReportApiKeyConfig apiKeyConfig = serviceProvider.GetRequiredService<IAggregateReportApiKeyConfig>();
            IApiKeyProvider apiKeyProvider = serviceProvider.GetRequiredService<IApiKeyProvider>();

            ApiKeyAuthenticationHeaderProvider apiKeyAuthenticationHeaderProvider =
                new ApiKeyAuthenticationHeaderProvider(apiKeyConfig.AggregateReportClaimsName, apiKeyProvider);

            return new AggregateReportClient(clientConfig, apiKeyAuthenticationHeaderProvider, logger);
        }
    }
}
