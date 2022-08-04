using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dmarc.Entity.Config
{
    public interface IDmarcEntityConfig
    {
        string SnsTopicArn { get; }
        string WebUrl { get; }
    }

    public class DmarcEntityConfig : IDmarcEntityConfig
    {
        public DmarcEntityConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            WebUrl = environmentVariables.Get("WebUrl");
        }

        public string SnsTopicArn { get; }
        public string WebUrl { get; }
    }
}
