using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Dmarc.Contracts.Poller;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Entity.Config;
using MailCheck.Dmarc.Entity.Entity;
using MailCheck.Dmarc.Entity.Entity.Notifications;
using MailCheck.Dmarc.Entity.Entity.Notifiers;
using NUnit.Framework;

namespace MailCheck.Dmarc.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class RecordChangedNotifierTests
    {
        private IMessageDispatcher _messageDispatcher;
        private IDmarcEntityConfig _dmarcEntityConfig;
        private RecordChangedNotifier _recordChangedNotifier;

        [SetUp]
        public void SetUp()
        {
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _dmarcEntityConfig = A.Fake<IDmarcEntityConfig>();

            _recordChangedNotifier = new RecordChangedNotifier(_messageDispatcher, _dmarcEntityConfig);
        }

        [Test]
        public void DoesNotNotifyWhenNoChanges()
        {
            _recordChangedNotifier.Handle(CreateEntityState("testRecord"), CreateDmarcRecordsEvaluated("testRecord"));

            A.CallTo(() => _messageDispatcher.Dispatch(A<Common.Messaging.Abstractions.Message>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void NotifiesWhenRecordChanges()
        {
            _recordChangedNotifier.Handle(CreateEntityState("testRecord1"), CreateDmarcRecordsEvaluated("testRecord2"));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcRecordAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcRecordAdded>.That.Matches(x => x.Records.First() == "testRecord2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcRecordRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcRecordRemoved>.That.Matches(x => x.Records.First() == "testRecord1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NotifiesWhenRecordAdded()
        {
            _recordChangedNotifier.Handle(CreateEntityState("testRecord1"), CreateDmarcRecordsEvaluated("testRecord1", "testRecord2"));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcRecordAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcRecordAdded>.That.Matches(x => x.Records.First() == "testRecord2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcRecordRemoved>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void NotifiesWhenRecordRemoved()
        {
            _recordChangedNotifier.Handle(CreateEntityState("testRecord1", "testRecord2"), CreateDmarcRecordsEvaluated("testRecord1"));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcRecordAdded>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcRecordRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcRecordRemoved>.That.Matches(x => x.Records.First() == "testRecord2"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        private DmarcEntityState CreateEntityState(params string[] recordStrings)
        {
            IEnumerable<DmarcRecord> dmarcRecords = recordStrings.Select(x => new DmarcRecord(x, null, null, null, null, false, false));
            DmarcEntityState entityState = new DmarcEntityState("", 0, new DmarcState(), DateTime.MaxValue)
            {
                DmarcRecords = new DmarcRecords("", dmarcRecords.ToList(), null, 0)
            };

            return entityState;
        }

        private DmarcRecordsEvaluated CreateDmarcRecordsEvaluated(params string[] recordStrings)
        {
            IEnumerable<DmarcRecord> dmarcRecords = recordStrings.Select(x => new DmarcRecord(x, null, null, null, null, false, false));
            DmarcRecordsEvaluated recordsEvaluated = new DmarcRecordsEvaluated("", new DmarcRecords("", dmarcRecords.ToList(), null, 0), null, null, DateTime.MinValue);
            return recordsEvaluated;
        }
    }
}
