using MailCheck.Common.Messaging.CloudWatch;
using MailCheck.Dmarc.Scheduler.StartUp;

namespace MailCheck.Dmarc.Scheduler
{
    public class DmarcPeriodicSchedulerLambdaEntryPoint : CloudWatchTriggeredLambdaEntryPoint
    {
        public DmarcPeriodicSchedulerLambdaEntryPoint()
            : base(new DmarcPeriodicSchedulerLambdaStartUp()) { }
    }
}