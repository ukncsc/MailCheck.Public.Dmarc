using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.AggregateReport.Client;
using MailCheck.Common.Logging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.SSM;
using MailCheck.Common.Util;
using MailCheck.Dkim.Client;
using MailCheck.Dmarc.Contracts.Poller;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Contracts.SharedDomain.Serialization;
using MailCheck.Dmarc.Evaluator.Config;
using MailCheck.Dmarc.Evaluator.Explainers;
using MailCheck.Dmarc.Evaluator.Rules;
using MailCheck.Spf.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.Dmarc.Evaluator.StartUp
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
                .AddTransient<IHandle<DmarcRecordsPolled>, EvaluationHandler>()
                .AddTransient<IDmarcEvaluationProcessor, DmarcEvaluationProcessor>()
                .AddTransient<IExplainer<Tag>, Explainer<Tag>>()
                .AddTransient<IDmarcRecordExplainer, DmarcRecordExplainer>()
                .AddTransient<IExplainerStrategy<Tag>, AdkimTagExplainer>()
                .AddTransient<IExplainerStrategy<Tag>, AspfTagExplainer>()
                .AddTransient<IExplainerStrategy<Tag>, FailureOptionsExplainer>()
                .AddTransient<IExplainerStrategy<Tag>, PercentExplainer>()
                .AddTransient<IExplainerStrategy<Tag>, PolicyExplainer>()
                .AddTransient<IExplainerStrategy<Tag>, ReportFormatExplainer>()
                .AddTransient<IExplainerStrategy<Tag>, ReportIntervalExplainer>()
                .AddTransient<IExplainerStrategy<Tag>, ReportUriAggregateExplainer>()
                .AddTransient<IExplainerStrategy<Tag>, ReportUriForensicExplainer>()
                .AddTransient<IExplainerStrategy<Tag>, SubDomainPolicyExplainer>()
                .AddTransient<IExplainerStrategy<Tag>, VersionExplainer>()
                .AddTransient<IDmarcRecordExplainer, DmarcRecordExplainer>()
                .AddTransient<IEvaluator<DmarcRecord>, Evaluator<DmarcRecord>>()
                .AddTransient<IRule<DmarcRecord>, PctValueShouldBe100>()
                .AddTransient<IRule<DmarcRecord>, PolicyShouldBeQuarantineOrReject>()
                .AddTransient<IRule<DmarcRecord>, NudgeAlongFromPolicyOfNone>()
                .AddTransient<IRule<DmarcRecord>, RuaTagsShouldContainDmarcServiceMailBox>()
                .AddTransient<IRule<DmarcRecord>, RufTagShouldNotContainDmarcServiceMailBox>()
                .AddTransient<IRule<DmarcRecord>, ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<IDmarcEvaluatorConfig, DmarcEvaluatorConfig>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IClock, Clock>()
                .AddSpfApiKeyClient()
                .AddDkimApiKeyClient()
                .AddAggregateReportApiKeyClient()
                .AddSerilogLogging();
        }
    }
}
