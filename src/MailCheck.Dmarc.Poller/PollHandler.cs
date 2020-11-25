using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.Poller;
using MailCheck.Dmarc.Poller.Config;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Mapping;
using Microsoft.Extensions.Logging;


namespace MailCheck.Dmarc.Poller
{
    public class PollHandler : IHandle<DmarcPollPending>
    {
        private readonly IDmarcProcessor _processor;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDmarcPollerConfig _config;
        private readonly ILogger<PollHandler> _log;

        public PollHandler(IDmarcProcessor processor,
            IMessageDispatcher dispatcher,
            IDmarcPollerConfig config,
            ILogger<PollHandler> log)
        {
            _processor = processor;
            _dispatcher = dispatcher;
            _config = config;
            _log = log;
        }

        public async Task Handle(DmarcPollPending message)
        {
            string domain = message.Id;

            using (_log.BeginScope(new Dictionary<string, string>
            {
                ["Domain"] = domain
            }))
            {
                try
                {
                    DmarcPollResult dmarcPollResult = await _processor.Process(domain);

                    _log.LogInformation("Polled DMARC records for {Domain}", domain);

                    DmarcRecordsPolled dmarcRecordsPolled = dmarcPollResult.ToDmarcRecordsPolled();

                    _dispatcher.Dispatch(dmarcRecordsPolled, _config.SnsTopicArn);

                    _log.LogInformation("Published DMARC records for {Domain}", domain);
                }
                catch (System.Exception e)
                {
                    string error = $"Error occurred polling domain {domain}";
                    _log.LogError(e, error);
                    throw;
                }
            }
        }
    }
}
