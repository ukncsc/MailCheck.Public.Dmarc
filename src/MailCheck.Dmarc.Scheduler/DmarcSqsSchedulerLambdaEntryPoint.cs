using Amazon.Lambda.Core;
using MailCheck.Common.Messaging.Sqs;
using MailCheck.Dmarc.Scheduler.StartUp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MailCheck.Dmarc.Scheduler
{
    public class DmarcSqsSchedulerLambdaEntryPoint : SqsTriggeredLambdaEntryPoint
    {
        public DmarcSqsSchedulerLambdaEntryPoint()
            : base(new DmarcSqsSchedulerLambdaStartUp()) { }
    }
}
