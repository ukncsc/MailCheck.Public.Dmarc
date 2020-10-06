using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dmarc.Api.Config
{
    public interface IDmarcApiConfig
    {
        int RecheckMinPeriodInSeconds { get; }
        string MicroserviceOutputSnsTopicArn { get; }
        string SnsTopicArn { get; }
    }

    public class DmarcApiConfig : IDmarcApiConfig
    {
        public DmarcApiConfig(IEnvironmentVariables environmentVariables)
        {
            MicroserviceOutputSnsTopicArn = environmentVariables.Get("MicroserviceOutputSnsTopicArn");
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            RecheckMinPeriodInSeconds = environmentVariables.GetAsInt("RecheckMinPeriodInSeconds");
        }

        public int RecheckMinPeriodInSeconds { get; }
        public string MicroserviceOutputSnsTopicArn { get; }
        public string SnsTopicArn { get; }
    }
}
