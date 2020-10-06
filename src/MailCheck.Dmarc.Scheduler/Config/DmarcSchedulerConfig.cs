using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dmarc.Scheduler.Config
{
    public interface IDmarcSchedulerConfig
    {
        string PublisherConnectionString { get; }
    }

    public class DmarcSchedulerConfig : IDmarcSchedulerConfig
    {
        public DmarcSchedulerConfig(IEnvironmentVariables environmentVariables)
        {
            PublisherConnectionString = environmentVariables.Get("SnsTopicArn");
        }

        public string PublisherConnectionString { get; }
    }
}
