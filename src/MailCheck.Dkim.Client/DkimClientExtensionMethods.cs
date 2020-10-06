using System;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Api.AuthProviders;
using MailCheck.Common.Api.Service;
using MailCheck.Common.Environment;
using MailCheck.Common.SSM;
using MailCheck.Common.Util;
using MailCheck.Dkim.Client.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dkim.Client
{
    public static class DkimClientExtensionMethods
    {
        public static IServiceCollection AddDkimClaimsPrincipleClient(this IServiceCollection serviceContainer)
        {
            return serviceContainer
                .AddEnvironment()
                .AddTransient<IDkimClientConfig, DkimClientConfig>()
                .AddTransient<IDkimClient, DkimClient>()
                .AddHttpContextAccessor()
                .AddTransient<IAuthenticationHeaderProvider, ClaimsPrincipalAuthenticationHeaderProvider>();
        }

        public static IServiceCollection AddDkimApiKeyClient(this IServiceCollection serviceContainer)
        {
            return serviceContainer
                .AddEnvironment()
                .AddTransient<IDkimClientConfig, DkimClientConfig>()
                .AddTransient<IDkimClient>(ApiKeyClientFactory)
                .AddTransient<IDkimApiKeyConfig, DkimApiKeyConfig>()
                .TryAddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .TryAddTransient<IApiKeyProvider, ApiKeyProvider>();
        }

        //use factory to prevent ambiguous reference to IAuthenticationHeaderProvider as each client
        //requires a different value for api key name, if multiple of these are reg'd could cause problems
        private static DkimClient ApiKeyClientFactory(IServiceProvider serviceProvider)
        {
            IDkimClientConfig clientConfig = serviceProvider.GetRequiredService<IDkimClientConfig>();
            ILogger<DkimClient> logger = serviceProvider.GetRequiredService<ILogger<DkimClient>>();

            IDkimApiKeyConfig apiKeyConfig = serviceProvider.GetRequiredService<IDkimApiKeyConfig>();
            IApiKeyProvider apiKeyProvider = serviceProvider.GetRequiredService<IApiKeyProvider>();

            ApiKeyAuthenticationHeaderProvider apiKeyAuthenticationHeaderProvider =
                new ApiKeyAuthenticationHeaderProvider(apiKeyConfig.DkimClaimsName, apiKeyProvider);

            return new DkimClient(clientConfig, apiKeyAuthenticationHeaderProvider, logger);
        }
    }
}