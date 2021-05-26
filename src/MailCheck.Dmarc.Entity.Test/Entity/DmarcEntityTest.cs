using MailCheck.Dmarc.Entity.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Common.Exception;
using MailCheck.Dmarc.Contracts;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Dmarc.Contracts.Scheduler;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Entity.Config;
using MailCheck.Dmarc.Entity.Dao;
using MailCheck.Dmarc.Entity.Entity.DomainStatus;
using MailCheck.Dmarc.Entity.Entity.Notifiers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using A = FakeItEasy.A;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;
using MailCheck.Common.Contracts.Messaging;

namespace MailCheck.Dmarc.Entity.Test.Entity
{
    [TestFixture]
    public class DmarcEntityTest
    {
        private const string Id = "abc.com";

        private IDmarcEntityDao _dmarcEntityDao;
        private IDmarcEntityConfig _dmarcEntityConfig;
        private ILogger<DmarcEntity> _log;
        private IMessageDispatcher _dispatcher;
        private IChangeNotifiersComposite _changeNotifiersComposite;
        private IDomainStatusPublisher _domainStatusPublisher;
        private DmarcEntity _dmarcEntity;

        [SetUp]
        public void SetUp()
        {
            _dmarcEntityDao = A.Fake<IDmarcEntityDao>();
            _dmarcEntityConfig = A.Fake<IDmarcEntityConfig>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _changeNotifiersComposite = A.Fake<IChangeNotifiersComposite>();
            _domainStatusPublisher = A.Fake<IDomainStatusPublisher>();
            _log = A.Fake<ILogger<DmarcEntity>>();
            _dmarcEntity = new DmarcEntity(_dmarcEntityDao, _dmarcEntityConfig, _dispatcher, _changeNotifiersComposite, _domainStatusPublisher, _log);
        }

        [Test]
        public async Task HandleDomainCreatedCreatesDomain()
        {
            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns<DmarcEntityState>(null);
            await _dmarcEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));

            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityState>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<DmarcEntityCreated>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleDomainCreatedDoesNotSave()
        {
            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns(new DmarcEntityState(Id, 1, DmarcState.PollPending, DateTime.UtcNow));
            await _dmarcEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));

            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<DmarcEntityCreated>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleDmarcRecordExpiredRaiseDmarcPollPending()
        {
            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns(new DmarcEntityState(Id, 2, DmarcState.PollPending, DateTime.Now)
            {
                LastUpdated = DateTime.Now.AddDays(-1),
                DmarcRecords = new DmarcRecords(Id, new List<DmarcRecord>(), new List<Message>(), 100),
                DmarcState = DmarcState.Created
            });

            await _dmarcEntity.Handle(new DmarcRecordExpired(Id));

            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityState>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<DmarcPollPending>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleDmarcRecordsEvaluatedAndNewEvaluationUpdatesStateAndPublishes()
        {
            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns(new DmarcEntityState(Id, 2, DmarcState.PollPending, DateTime.Now)
            {
                LastUpdated = DateTime.Now.AddDays(-1),
                DmarcRecords = CreateDmarcRecords()
            });

            DmarcRecords dmarcRecords = CreateDmarcRecords();

            dmarcRecords.Records[0].Messages.Add(new Message(Guid.NewGuid(), MessageSources.DmarcEvaluator, MessageType.error, "EvaluationError", string.Empty));
            dmarcRecords.Records[0].Tags[0].Explanation = "Explanation";

            DmarcRecordsEvaluated dmarcRecordsEvaluated = new DmarcRecordsEvaluated(Id, dmarcRecords, null, null, DateTime.MinValue);

            await _dmarcEntity.Handle(dmarcRecordsEvaluated);

            A.CallTo(() => _dispatcher.Dispatch(A<DmarcRecordEvaluationsChanged>.That.Matches(
                    _ => _.Records.Records[0].Messages[0].Text.Equals(dmarcRecords.Records[0].Messages[0].Text) &&
                         _.Records.Records[0].Tags[0].Explanation.Equals(dmarcRecords.Records[0].Tags[0].Explanation)), A<string>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _domainStatusPublisher.Publish(dmarcRecordsEvaluated)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityState>._)).MustHaveHappenedOnceExactly();
        }

        private static DmarcRecords CreateDmarcRecords(string domain = Id)
        {
            return new DmarcRecords(domain, new List<DmarcRecord>
            {
                new DmarcRecord("v=dmarc......", new List<Tag> { new Adkim("adkim", AlignmentType.R, false)}, null, domain, "", true, false)
            }, new List<Message>(), 100);
        }
    }
}
