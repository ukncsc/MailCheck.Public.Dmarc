using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Common.Exception;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Dmarc.Contracts.Scheduler;
using MailCheck.Dmarc.Entity.Config;
using MailCheck.Dmarc.Entity.Dao;
using MailCheck.Dmarc.Entity.Entity.Notifiers;
using MailCheck.Dmarc.Entity.Entity.DomainStatus;

namespace MailCheck.Dmarc.Entity.Entity
{
    public class DmarcEntity :
        IHandle<DomainCreated>,
        IHandle<DmarcRecordExpired>,
        IHandle<DmarcRecordsEvaluated>,
        IHandle<DomainDeleted>
    {
        private readonly IDmarcEntityDao _dao;
        private readonly ILogger<DmarcEntity> _log;
        private readonly IDmarcEntityConfig _dmarcEntityConfig;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IChangeNotifiersComposite _changeNotifiersComposite;
        private readonly IDomainStatusPublisher _domainStatusPublisher;

        public DmarcEntity(IDmarcEntityDao dao,
            IDmarcEntityConfig dmarcEntityConfig,
            IMessageDispatcher dispatcher, IChangeNotifiersComposite changeNotifiersComposite,
            IDomainStatusPublisher domainStatusPublisher,
            ILogger<DmarcEntity> log)
        {
            _dao = dao;
            _log = log;
            _domainStatusPublisher = domainStatusPublisher;
            _dmarcEntityConfig = dmarcEntityConfig;
            _dispatcher = dispatcher;
            _changeNotifiersComposite = changeNotifiersComposite;
        }

        public async Task Handle(DomainCreated message)
        {
            string domain = message.Id.ToLower();

            DmarcEntityState state = await _dao.Get(domain);

            if (state != null)
            {
                _log.LogInformation($"Ignoring {nameof(DomainCreated)} as DmarcEntity already exists for {domain}.");
            }
            else
            {
                state = new DmarcEntityState(domain, 1, DmarcState.Created, DateTime.UtcNow);
                await _dao.Save(state);
                _log.LogInformation($"Created DmarcEntity for {domain}.");
            }

            DmarcEntityCreated dmarcEntityCreated = new DmarcEntityCreated(domain, state.Version);
            _dispatcher.Dispatch(dmarcEntityCreated, _dmarcEntityConfig.SnsTopicArn);
        }

        public async Task Handle(DomainDeleted message)
        {
            string domain = message.Id.ToLower();
            int rows = await _dao.Delete(domain);
            if (rows == 1)
            {
                _log.LogInformation($"Deleted DMARC entity for: {domain}.");
            }
            else
            {
                _log.LogInformation($"DMARC entity already deleted for: {domain}.");
            }
        }

        public async Task Handle(DmarcRecordExpired message)
        {
            string messageId = message.Id.ToLower();

            DmarcEntityState state = await LoadState(messageId, nameof(message));

            Message evnt = state.UpdatePollPending();

            state.Version++;

            await _dao.Save(state);

            _dispatcher.Dispatch(evnt, _dmarcEntityConfig.SnsTopicArn);
        }

        public async Task Handle(DmarcRecordsEvaluated message)
        {
            string messageId = message.Id.ToLower();

            DmarcEntityState state = await LoadState(messageId, nameof(message));

            _changeNotifiersComposite.Handle(state, message);

            _domainStatusPublisher.Publish(message);

            Message evnt = state.UpdateDmarcEvaluation(message.Records,
                message.ElapsedQueryTime, message.Messages, message.Timestamp);

            state.Version++;

            await _dao.Save(state);

            _dispatcher.Dispatch(evnt, _dmarcEntityConfig.SnsTopicArn);

        }

        private async Task<DmarcEntityState> LoadState(string id, string messageType)
        {
            DmarcEntityState state = await _dao.Get(id);

            if (state == null)
            {
                _log.LogInformation("Ignoring {EventName} as DMARC Entity does not exist for {Id}.", messageType, id);
                throw new MailCheckException(
                    $"Cannot handle event {messageType} as DMARC Entity doesnt exists for {id}.");
            }

            return state;
        }

    }
}
