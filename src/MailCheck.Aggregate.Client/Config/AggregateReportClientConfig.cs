using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.AggregateReport.Client.Config
{
    public interface IAggregateReportClientConfig
    {
        string AggregateReportApiEndpoint { get; }
    }

    internal class AggregateReportClientConfig : IAggregateReportClientConfig
    {
        public AggregateReportClientConfig(IEnvironment environment)
        {
            AggregateReportApiEndpoint = environment.GetEnvironmentVariable("AggregateReportApiEndpoint");
        }

        public string AggregateReportApiEndpoint { get; }
    }
}