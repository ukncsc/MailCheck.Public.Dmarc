using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Scheduler.Config;
using MailCheck.Dmarc.Scheduler.Dao;
using MailCheck.Dmarc.Scheduler.Dao.Model;
using MailCheck.Dmarc.Scheduler.Mapping;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dmarc.Scheduler.Handler
{
    public class DmarcSchedulerHandler : 
        IHandle<DmarcEntityCreated>,
        IHandle<DomainDeleted>
    {
        private readonly IDmarcSchedulerDao _dao;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDmarcSchedulerConfig _config;
        private readonly ILogger<DmarcSchedulerHandler> _log;

        public DmarcSchedulerHandler(IDmarcSchedulerDao dao,
            IMessageDispatcher dispatcher,
            IDmarcSchedulerConfig config,
            ILogger<DmarcSchedulerHandler> log)
        {
            _dao = dao;
            _dispatcher = dispatcher;
            _config = config;
            _log = log;
        }

        public async Task Handle(DmarcEntityCreated message)
        {
            string domain = message.Id.ToLower();
            DmarcSchedulerState state = await _dao.Get(domain);

            if (state == null)
            {
                state = new DmarcSchedulerState(domain);

                await _dao.Save(state);

                _dispatcher.Dispatch(state.ToDmarcRecordExpiredMessage(), _config.PublisherConnectionString);

                _log.LogInformation($"New {nameof(DmarcSchedulerState)} saved for {domain}");
            }
            else
            {
                _log.LogInformation($"{nameof(DmarcSchedulerState)} already exists for {domain}");
            }
        }

        public async Task Handle(DomainDeleted message)
        {
            string domain = message.Id.ToLower();
            int rows = await _dao.Delete(domain);
            if (rows == 1)
            {
                _log.LogInformation($"Deleted schedule for DMARC entity with id: {domain}.");
            }
            else
            {
                _log.LogInformation($"Schedule already deleted for DMARC entity with id: {domain}.");
            }
            
        }
    }
}
