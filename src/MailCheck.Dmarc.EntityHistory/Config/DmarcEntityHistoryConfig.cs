using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dmarc.EntityHistory.Config
{
    public interface IDmarcEntityHistoryConfig
    {
        string SnsTopicArn { get; }
    }

    public class DmarcEntityHistoryConfig : IDmarcEntityHistoryConfig
    {
        public DmarcEntityHistoryConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
    }
}
