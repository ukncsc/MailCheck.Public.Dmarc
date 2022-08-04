using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Entity.Config;
using MailCheck.Dmarc.Entity.Entity;
using MailCheck.Dmarc.Entity.Entity.Notifications;
using MailCheck.Dmarc.Entity.Entity.Notifiers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;
using MessageDisplay = MailCheck.Dmarc.Entity.Entity.Notifications.MessageDisplay;
using MessageType = MailCheck.Dmarc.Entity.Entity.Notifications.MessageType;

namespace MailCheck.Dmarc.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class AdvisoryChangedNotifierTests
    {
        private IMessageDispatcher _messageDispatcher;
        private IDmarcEntityConfig _dmarcEntityConfig;
        private IEqualityComparer<Message> _messageEqualityComparer;
        private ILogger<AdvisoryChangedNotifier> _logger;
        private AdvisoryChangedNotifier _advisoryChangedNotifier;

        [SetUp]
        public void SetUp()
        {
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _dmarcEntityConfig = A.Fake<IDmarcEntityConfig>();
            _messageEqualityComparer = new MessageEqualityComparer();
            _advisoryChangedNotifier = new AdvisoryChangedNotifier(_messageDispatcher, _dmarcEntityConfig, _messageEqualityComparer);
        }

        [Test]
        public void DoesNotNotifyWhenMessageAreSameIdButDifferentMessageType()
        {
            Guid id = Guid.NewGuid();

            Message existingMessage = new Message(id, "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.warning, "testText1", string.Empty);
            Message newMessage = new Message(id, "mailcheck.dmarc.testName", "testSource text has changed!", Contracts.SharedDomain.MessageType.error, "testText2", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateDmarcRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "testText1"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenNoChangeSendsAdvisorySustained()
        {
            Guid messageId = Guid.NewGuid();

            Message existingMessage = new Message(messageId, "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText", string.Empty);
            Message newMessage = new Message(messageId, "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateDmarcRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "testText"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAddedAndRemovedWhenNewMessageTypeChanged()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText", string.Empty);
            Message newMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.error, "testText", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateDmarcRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>.That.Matches(x => x.Messages.First().MessageType == MessageType.error), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>.That.Matches(x => x.Messages.First().MessageType == MessageType.info), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void RaisesAddedAndRemovedWhenNewMessageTextChanged()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText1", string.Empty);
            Message newMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText2", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateDmarcRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>.That.Matches(x => x.Messages.First().Text == "testText1"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void RaisesAddedAndRemovedWhenNewMessagesAdded()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText", string.Empty);
            Message newMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "newTestText", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateDmarcRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>.That.Matches(x => x.Messages.First().Text == "newTestText"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>.That.Matches(x => x.Messages.First().Text == "testText"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void RaisesAdvisoryAddedAndAdvisorySustainedWhenNewMessageAddedToExistingList()
        {
            Guid messageId = Guid.NewGuid();

            Message existingMessage = new Message(messageId, "mailcheck.dmarc.testName", "testSource1", Contracts.SharedDomain.MessageType.info, "testText1", string.Empty);

            Message newMessage1 = new Message(messageId, "mailcheck.dmarc.testName", "testSource1", Contracts.SharedDomain.MessageType.info, "testText1", string.Empty);
            Message newMessage2 = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource2", Contracts.SharedDomain.MessageType.info, "testText2", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateDmarcRecordsEvaluatedWithMessages(newMessage1, newMessage2));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "testText1"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAdvisoryRemovedAndAdvisorySustainedWhenRemovedFromExistingList()
        {
            Guid messageId = Guid.NewGuid();

            Message existingMessage1 = new Message(messageId, "mailcheck.dmarc.testName", "testSource1", Contracts.SharedDomain.MessageType.info, "testText1", string.Empty);
            Message existingMessage2 = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource2", Contracts.SharedDomain.MessageType.info, "testText2", string.Empty);

            Message newMessage = new Message(messageId, "mailcheck.dmarc.testName", "testSource1", Contracts.SharedDomain.MessageType.info, "testText1", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage1, existingMessage2), CreateDmarcRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "testText1"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesDmarcAdvisorySustainedWhenNoChangeTest()
        {
            Guid messageId = Guid.NewGuid();

            Message existingMessage = new Message(messageId, "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText", string.Empty);
            Message newMessage = new Message(messageId, "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithRecords(existingMessage), CreateDmarcRecordsEvaluatedWithRecords(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "testText"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAdvisoryAddedWhenMessageTypeChanges()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText", string.Empty);
            Message newMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.error, "testText", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithRecords(existingMessage), CreateDmarcRecordsEvaluatedWithRecords(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>.That.Matches(x => x.Messages.First().MessageType == MessageType.error), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>.That.Matches(x => x.Messages.First().MessageType == MessageType.info), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAdvisoryAddedAndAdivosryRemovedWhenMessageReplaced()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText1", string.Empty);
            Message newMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText2", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithRecords(existingMessage), CreateDmarcRecordsEvaluatedWithRecords(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>.That.Matches(x => x.Messages.First().Text == "testText1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAdvisoryAddedWhenMessageDisplayTypeChanges()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText", string.Empty, Contracts.SharedDomain.MessageDisplay.Prompt);
            Message newMessage = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource", Contracts.SharedDomain.MessageType.info, "testText", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithRecords(existingMessage), CreateDmarcRecordsEvaluatedWithRecords(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>.That.Matches(x => x.Messages.First().MessageDisplay == MessageDisplay.Standard), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void RaisesAdvisoryAddedWhenMessageAdded()
        {
            Guid messageId = Guid.NewGuid();

            Message existingMessage = new Message(messageId, "mailcheck.dmarc.testName", "testSource1", Contracts.SharedDomain.MessageType.info, "testText1", string.Empty);

            Message newMessage1 = new Message(messageId, "mailcheck.dmarc.testName", "testSource1", Contracts.SharedDomain.MessageType.info, "testText1", string.Empty);
            Message newMessage2 = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource2", Contracts.SharedDomain.MessageType.info, "testText2", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithRecords(existingMessage), CreateDmarcRecordsEvaluatedWithRecords(newMessage1, newMessage2));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void RaisesAdvisoryRemovedWhenMessageRemoved()
        {
            Guid messageId = Guid.NewGuid();

            Message existingMessage1 = new Message(messageId, "mailcheck.dmarc.testName", "testSource1", Contracts.SharedDomain.MessageType.info, "testText1", string.Empty);
            Message existingMessage2 = new Message(Guid.NewGuid(), "mailcheck.dmarc.testName", "testSource2", Contracts.SharedDomain.MessageType.info, "testText2", string.Empty);

            Message newMessage = new Message(messageId, "mailcheck.dmarc.testName", "testSource1", Contracts.SharedDomain.MessageType.info, "testText1", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithRecords(existingMessage1, existingMessage2), CreateDmarcRecordsEvaluatedWithRecords(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DmarcAdvisoryRemoved>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        private DmarcEntityState CreateEntityStateWithMessages(params Message[] messages)
        {
            DmarcEntityState entityState = new DmarcEntityState("", 0, new DmarcState(), DateTime.MaxValue)
            {
                Messages = messages.ToList()
            };

            return entityState;
        }

        private DmarcRecordsEvaluated CreateDmarcRecordsEvaluatedWithMessages(params Message[] messages)
        {
            DmarcRecordsEvaluated recordsEvaluated = new DmarcRecordsEvaluated("", null, null, messages.ToList(), DateTime.MinValue);
            return recordsEvaluated;
        }

        private DmarcEntityState CreateEntityStateWithRecords(params Message[] messages)
        {
            List<DmarcRecord> records = messages.Select(x => new DmarcRecord("", null, new List<Message>() { x }, "", "", false, false)).ToList();
            DmarcEntityState entityState = new DmarcEntityState("", 0, new DmarcState(), DateTime.MaxValue)
            {
                DmarcRecords = new DmarcRecords("", records, null, 0)
            };

            return entityState;
        }

        private DmarcRecordsEvaluated CreateDmarcRecordsEvaluatedWithRecords(params Message[] messages)
        {
            DmarcRecordsEvaluated recordsEvaluated = new DmarcRecordsEvaluated("", null, null, messages.ToList(), DateTime.MinValue);
            return recordsEvaluated;
        }
    }
}
