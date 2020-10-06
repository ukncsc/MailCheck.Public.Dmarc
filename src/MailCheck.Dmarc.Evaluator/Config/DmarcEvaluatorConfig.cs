using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dmarc.Evaluator.Config
{
    public interface IDmarcEvaluatorConfig
    {
        string SnsTopicArn { get; }
    }

    public class DmarcEvaluatorConfig : IDmarcEvaluatorConfig
    {
        public DmarcEvaluatorConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
    }
}
