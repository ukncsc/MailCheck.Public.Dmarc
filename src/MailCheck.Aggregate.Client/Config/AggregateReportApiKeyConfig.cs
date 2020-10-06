using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.AggregateReport.Client.Config
{
    public interface IAggregateReportApiKeyConfig
    {
        string AggregateReportClaimsName { get; }
    }

    internal class AggregateReportApiKeyConfig : IAggregateReportApiKeyConfig
    {
        public AggregateReportApiKeyConfig(IEnvironment environment)
        {
            AggregateReportClaimsName = environment.GetEnvironmentVariable("AggregateReportClaimsName");
        }

        public string AggregateReportClaimsName { get; }
    }
}
