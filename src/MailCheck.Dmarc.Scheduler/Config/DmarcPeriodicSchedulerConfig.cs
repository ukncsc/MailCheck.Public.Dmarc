using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dmarc.Scheduler.Config
{
    public interface IDmarcPeriodicSchedulerConfig : IDmarcSchedulerConfig
    {
        int DomainBatchSize { get; }
        long RefreshIntervalSeconds { get; }
    }

    public class DmarcPeriodicSchedulerConfig : DmarcSchedulerConfig, IDmarcPeriodicSchedulerConfig
    {
        public DmarcPeriodicSchedulerConfig(IEnvironmentVariables environmentVariables) : base(environmentVariables)
        {
            DomainBatchSize = environmentVariables.GetAsInt("DomainBatchSize");
            RefreshIntervalSeconds = environmentVariables.GetAsLong("RefreshIntervalSeconds");
        }

        public int DomainBatchSize { get; }

        public long RefreshIntervalSeconds { get; }
    }
}
