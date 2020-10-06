using System;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Api.AuthProviders;
using MailCheck.Common.Api.Service;
using MailCheck.Common.Environment;
using MailCheck.Common.SSM;
using MailCheck.Common.Util;
using MailCheck.Spf.Client.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MailCheck.Spf.Client
{
    public static class SpfClientExtensionMethods
    {
        public static IServiceCollection AddSpfClaimsPrincipleClient(this IServiceCollection serviceContainer)
        {
            return serviceContainer
                .AddEnvironment()
                .AddTransient<ISpfClientConfig, SpfClientConfig>()
                .AddTransient<ISpfClient, SpfClient>()
                .AddHttpContextAccessor()
                .AddTransient<IAuthenticationHeaderProvider, ClaimsPrincipalAuthenticationHeaderProvider>();
        }

        public static IServiceCollection AddSpfApiKeyClient(this IServiceCollection serviceContainer)
        {
            return serviceContainer
                .AddEnvironment()
                .AddTransient<ISpfClientConfig, SpfClientConfig>()
                .AddTransient<ISpfClient>(ApiKeyClientFactory)
                .AddTransient<ISpfApiKeyConfig, SpfApiKeyConfig>()
                .TryAddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .TryAddTransient<IApiKeyProvider, ApiKeyProvider>();
        }

        //use factory to prevent ambiguous reference to IAuthenticationHeaderProvider as each client
        //requires a different value for api key name, if multiple of these are reg'd could cause problems
        private static SpfClient ApiKeyClientFactory(IServiceProvider serviceProvider)
        {
            ISpfClientConfig clientConfig = serviceProvider.GetRequiredService<ISpfClientConfig>();
            ILogger<SpfClient> logger = serviceProvider.GetRequiredService<ILogger<SpfClient>>();

            ISpfApiKeyConfig apiKeyConfig = serviceProvider.GetRequiredService<ISpfApiKeyConfig>();
            IApiKeyProvider apiKeyProvider = serviceProvider.GetRequiredService<IApiKeyProvider>();

            ApiKeyAuthenticationHeaderProvider apiKeyAuthenticationHeaderProvider =
                new ApiKeyAuthenticationHeaderProvider(apiKeyConfig.SpfClaimsName, apiKeyProvider);

            return new SpfClient(clientConfig, apiKeyAuthenticationHeaderProvider, logger);
        }
    }
}