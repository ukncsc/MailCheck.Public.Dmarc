using Amazon.Lambda.Core;
using MailCheck.Common.Messaging.Sqs;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MailCheck.Dmarc.Poller
{
    public class DmarcPollerLambdaEntryPoint : SqsTriggeredLambdaEntryPoint
    {
        public DmarcPollerLambdaEntryPoint() : base(new StartUp.StartUp())
        {
        }
    }
}
