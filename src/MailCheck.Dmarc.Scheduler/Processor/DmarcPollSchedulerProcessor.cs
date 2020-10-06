using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;
using MailCheck.Dmarc.Scheduler.Config;
using MailCheck.Dmarc.Scheduler.Dao;
using MailCheck.Dmarc.Scheduler.Dao.Model;
using MailCheck.Dmarc.Scheduler.Mapping;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dmarc.Scheduler.Processor
{
    public class DmarcPollSchedulerProcessor : IProcess
    {
        private readonly IDmarcPeriodicSchedulerDao _dao;
        private readonly IMessagePublisher _publisher;
        private readonly IDmarcPeriodicSchedulerConfig _config;
        private readonly ILogger<DmarcPollSchedulerProcessor> _log;

        public DmarcPollSchedulerProcessor(IDmarcPeriodicSchedulerDao dao,
            IMessagePublisher publisher,
            IDmarcPeriodicSchedulerConfig config,
            ILogger<DmarcPollSchedulerProcessor> log)
        {
            _dao = dao;
            _publisher = publisher;
            _config = config;
            _log = log;
        }

        public async Task<ProcessResult> Process()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<DmarcSchedulerState> expiredRecords = await _dao.GetExpiredDmarcRecords();

            _log.LogInformation($"Found {expiredRecords.Count} expired records.");

            if (expiredRecords.Any())
            {
                expiredRecords
                    .Select(_ => _publisher.Publish(_.ToDmarcRecordExpiredMessage(), _config.PublisherConnectionString))
                    .Batch(10)
                    .ToList()
                    .ForEach(async _ => await Task.WhenAll(_));

                await _dao.UpdateLastChecked(expiredRecords);

                _log.LogInformation($"Processing for domains {string.Join(',', expiredRecords.Select(_ => _.Id))} took {stopwatch.Elapsed}.");
            }

            stopwatch.Stop();

            return expiredRecords.Any()
                ? ProcessResult.Continue
                : ProcessResult.Stop;
        }
    }
}
