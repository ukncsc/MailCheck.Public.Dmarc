using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dmarc.Entity.Config
{
    public interface IDmarcEntityConfig
    {
        string SnsTopicArn { get; }
    }

    public class DmarcEntityConfig : IDmarcEntityConfig
    {
        public DmarcEntityConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
    }
}
