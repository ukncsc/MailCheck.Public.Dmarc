using System;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Common.Exception;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Dmarc.Contracts.External;
using MailCheck.Dmarc.Contracts.Scheduler;
using MailCheck.Dmarc.Entity.Config;
using MailCheck.Dmarc.Entity.Dao;
using MailCheck.Dmarc.Entity.Entity.Notifiers;
using Microsoft.Extensions.Logging;
using System.Linq;
using MailCheck.Dmarc.Entity.Entity.DomainStatus;

namespace MailCheck.Dmarc.Entity.Entity
{
    public class DmarcEntity:
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
            string messageId = message.Id.ToLower();

            DmarcEntityState state = await _dao.Get(messageId);

            if (state != null)
            {
                _log.LogError("Ignoring {EventName} as DmarcEntity already exists for {Id}.", nameof(DomainCreated), messageId);
                throw new MailCheckException($"Cannot handle event {nameof(DomainCreated)} as DmarcEntity already exists for {messageId}.");
            }
            
            state = new DmarcEntityState(messageId, 1, DmarcState.Created, DateTime.UtcNow);
            await _dao.Save(state);
            DmarcEntityCreated dmarcEntityCreated = new DmarcEntityCreated(messageId, state.Version);
            _dispatcher.Dispatch(dmarcEntityCreated, _dmarcEntityConfig.SnsTopicArn);
            _log.LogInformation("Created DmarcEntity for {Id}.", messageId);
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

        public class DomainStatusEvaluation : Message
        {
            public string RecordType { get; }

            public Contracts.SharedDomain.MessageType MessageType { get; }

            public DomainStatusEvaluation(string id, string recordType, Contracts.SharedDomain.MessageType messageType) : base(id)
            {
                RecordType = recordType;
                MessageType = messageType;
            }
        }

        private async Task<DmarcEntityState> LoadState(string id, string messageType)
        {
            DmarcEntityState state = await _dao.Get(id);

            if (state == null)
            {
                _log.LogError("Ignoring {EventName} as DMARC Entity does not exist for {Id}.", messageType, id);
                throw new MailCheckException(
                    $"Cannot handle event {messageType} as DMARC Entity doesnt exists for {id}.");
            }

            return state;
        }

        public async Task Handle(DomainDeleted message)
        {
            await _dao.Delete(message.Id);
            _log.LogInformation($"Deleted DMARC entity with id: {message.Id}.");
        }
    }
}
