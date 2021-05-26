using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Poller;
using MailCheck.Dmarc.EntityHistory.Dao;
using MailCheck.Dmarc.EntityHistory.Service;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dmarc.EntityHistory.Entity
{
    public class DmarcEntityHistory : 
        IHandle<DomainCreated>,
        IHandle<DmarcRecordsPolled>
    {
        private readonly IDmarcEntityHistoryDao _dao;
        private readonly ILogger<DmarcEntityHistory> _log;
        private readonly IDmarcRuaService _ruaService;

        public DmarcEntityHistory(
            ILogger<DmarcEntityHistory> log,
            IDmarcEntityHistoryDao dao,
            IDmarcRuaService ruaService)
        {
            _dao = dao;
            _log = log;
            _ruaService = ruaService;
        }

        public async Task Handle(DomainCreated message)
        {
            string domain = message.Id.ToLower();

            DmarcEntityHistoryState state = await _dao.Get(domain);

            if (state == null)
            {
                state = new DmarcEntityHistoryState(domain);
                await _dao.Save(state);
                _log.LogInformation($"Created DmarcHistoryEntity for {domain}.");
            }
            else
            {
                _log.LogInformation($"Ignoring {nameof(DomainCreated)} as DmarcHistoryEntity already exists for {domain}.");
            }
        }

        public async Task Handle(DmarcRecordsPolled message)
        {
            string messageId = message.Id.ToLower();

            DmarcEntityHistoryState dmarcEntityHistory = await LoadHistoryState(messageId);

            List<string> records = new List<string>();

            message.Records?.Records.ForEach(x => records.Add(x.Record));

            if (dmarcEntityHistory.UpdateHistory(records, message.Timestamp))
            {
                _log.LogInformation("DMARC record has changed for {Id}", messageId);

                await _dao.Save(dmarcEntityHistory);

                records.ForEach(x => _ruaService.Process(messageId, x));
            }
            else
            {
                _log.LogInformation("No DMARC record change for {Id}", messageId);
            }
        }

        private async Task<DmarcEntityHistoryState> LoadHistoryState(string id)
        {
            DmarcEntityHistoryState dmarcEntityHistoryState =
                await _dao.Get(id) ?? new DmarcEntityHistoryState(id);

            return dmarcEntityHistoryState;
        }
    }
}