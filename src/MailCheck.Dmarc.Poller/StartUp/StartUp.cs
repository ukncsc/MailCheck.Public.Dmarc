using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Amazon.SimpleNotificationService;
using DnsClient;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Environment.FeatureManagement;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain.Serialization;
using MailCheck.Dmarc.Poller.Config;
using MailCheck.Dmarc.Poller.Dns;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Implicit;
using MailCheck.Dmarc.Poller.OrgDomain;
using MailCheck.Dmarc.Poller.Parsing;
using MailCheck.Dmarc.Poller.Rules;
using MailCheck.Dmarc.Poller.Rules.Record;
using MailCheck.Dmarc.Poller.Rules.Records;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.Dmarc.Poller.StartUp
{
    public class StartUp : IStartUp
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
                .AddTransient<DmarcProcessor>()
                .AddSingleton(CreateLookupClient)
                .AddTransient<IDmarcProcessor, DmarcProcessor>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<IDnsClient, Dns.DnsClient>()
                .AddTransient<IDmarcPollerConfig, DmarcPollerConfig>()
                .AddTransient<IDnsNameServerProvider, LinuxDnsNameServerProvider>()
                .AddTransient<IHandle<DmarcPollPending>, PollHandler>()
                .AddTransient<IOrganisationalDomainProvider, OrganisationDomainProvider>()
                .AddTransient<ITagParser, TagParser>()
                .AddTransient<IImplicitProvider<Tag>, ImplicitProvider<Tag>>()
                .AddTransient<IImplicitProviderStrategy<Tag>, ReportIntervalImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, ReportFormatImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, PercentImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, FailureOptionsImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, AspfImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, AdkimImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, SubDomainPolicyImplicitProvider>()
                .AddTransient<IDmarcRecordParser, DmarcRecordParser>()
                .AddTransient<IDmarcRecordsParser, DmarcRecordsParser>()
                .AddTransient<ITagParserStrategy, VersionParserStrategy>()
                .AddTransient<ITagParserStrategy, AdkimParserStrategy>()
                .AddTransient<ITagParserStrategy, AspfParserStrategy>()
                .AddTransient<ITagParserStrategy, FailureOptionsParserStrategy>()
                .AddTransient<ITagParserStrategy, PolicyParserStrategy>()
                .AddTransient<ITagParserStrategy, PercentParserStrategy>()
                .AddTransient<ITagParserStrategy, ReportFormatParserStrategy>()
                .AddTransient<ITagParserStrategy, ReportIntervalParserStrategy>()
                .AddTransient<ITagParserStrategy, ReportUriAggregateParserStrategy>()
                .AddTransient<ITagParserStrategy, ReportUriForensicParserStrategy>()
                .AddTransient<ITagParserStrategy, SubDomainPolicyParserStrategy>()
                .AddTransient<IUriTagParser, UriTagParser>()
                .AddTransient<IMaxReportSizeParser, MaxReportSizeParser>()
                .AddTransient<IEvaluator<DmarcRecords>, Evaluator<DmarcRecords>>()
                .AddTransient<IRule<DmarcRecords>, TldDmarcRecordBehaviourIsWeaklyDefined>()
                .AddTransient<IEvaluator<DmarcRecord>, Evaluator<DmarcRecord>>()
                .AddTransient<IRule<DmarcRecord>, MaxLengthOf450Characters>()
                .AddTransient<IRule<DmarcRecord>, PolicyTagMustExist>()
                .AddTransient<IRule<DmarcRecord>, RuaTagsShouldBeMailTo>()
                .AddTransient<IRule<DmarcRecord>, RufTagShouldBeMailTo>()
                .AddTransient<IRule<DmarcRecord>, VersionMustBeFirstTag>()
                .AddTransient<IRule<DmarcRecord>, SubDomainPolicyShouldNotBeOnNonOrganisationalDomain>()
                .AddTransient<IDmarcUriParser, DmarcUriParser>()
                .AddConditionally(
                    "MigrationAdvisories",
                    featureActiveRegistrations =>
                    {
                        featureActiveRegistrations.AddTransient<IRule<DmarcRecords>, MigrationOnlyOneDmarcRecord>();
                    },
                    featureInactiveRegistrations =>
                    {
                        featureInactiveRegistrations.AddTransient<IRule<DmarcRecords>, OnlyOneDmarcRecord>();
                    });
        }

        private static ILookupClient CreateLookupClient(IServiceProvider provider)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new LookupClient(NameServer.GooglePublicDns, NameServer.GooglePublicDnsIPv6)
                {
                    Timeout = provider.GetRequiredService<IDmarcPollerConfig>().DnsRecordLookupTimeout
                }
                : new LookupClient(new LookupClientOptions(provider.GetService<IDnsNameServerProvider>()
                    .GetNameServers()
                    .Select(_ => new IPEndPoint(_, 53)).ToArray())
                {
                    ContinueOnEmptyResponse = false,
                    UseCache = false,
                    UseTcpOnly = true,
                    EnableAuditTrail = true,
                    Timeout = provider.GetRequiredService<IDmarcPollerConfig>().DnsRecordLookupTimeout
                });
        }
    }
}
