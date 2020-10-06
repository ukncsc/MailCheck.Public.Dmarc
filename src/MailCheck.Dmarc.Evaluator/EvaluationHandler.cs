using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Dmarc.Contracts.Poller;
using MailCheck.Dmarc.Evaluator.Config;

namespace MailCheck.Dmarc.Evaluator
{
    public class EvaluationHandler : IHandle<DmarcRecordsPolled>
    {
        private readonly IDmarcEvaluationProcessor _dmarcEvaluationProcessor;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDmarcEvaluatorConfig _config;

        public EvaluationHandler(IDmarcEvaluationProcessor dmarcEvaluationProcessor,
            IMessageDispatcher dispatcher,
            IDmarcEvaluatorConfig config)
        {
            _dmarcEvaluationProcessor = dmarcEvaluationProcessor;
            _dispatcher = dispatcher;
            _config = config;
        }

        public async Task Handle(DmarcRecordsPolled message)
        {
            await _dmarcEvaluationProcessor.Process(message.Records);

            _dispatcher.Dispatch(
                new DmarcRecordsEvaluated(message.Id, message.Records, message.ElapsedQueryTime, message.Messages, message.Timestamp), _config.SnsTopicArn);
        }
    }
}
