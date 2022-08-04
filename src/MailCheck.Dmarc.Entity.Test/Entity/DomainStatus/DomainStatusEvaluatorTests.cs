using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.DomainStatus.Contracts;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Entity.Entity.DomainStatus;
using NUnit.Framework;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;

namespace MailCheck.Dmarc.Entity.Test.Entity.DomainStatus
{
    [TestFixture]
    public class DomainStatusEvaluatorTests
    {
        private DomainStatusEvaluator _domainStatusEvaluator;

        [SetUp]
        public void SetUp()
        {
            _domainStatusEvaluator = new DomainStatusEvaluator();
        }

        [TestCase(Status.Success, null)]
        [TestCase(Status.Success, new MessageType[] { })]
        [TestCase(Status.Info, new[] { MessageType.info, MessageType.info })]
        [TestCase(Status.Warning, new[] { MessageType.info, MessageType.warning })]
        [TestCase(Status.Warning, new[] { MessageType.warning, MessageType.warning })]
        [TestCase(Status.Error, new[] { MessageType.info, MessageType.error })]
        [TestCase(Status.Error, new[] { MessageType.warning, MessageType.error })]
        [TestCase(Status.Error, new[] { MessageType.error, MessageType.error })]
        [TestCase(Status.Error, new[] { MessageType.info, MessageType.warning, MessageType.error })]
        public void StatusIsDeterminedAndDispatched(Status expectedStatus, MessageType[] messageTypes)
        {
            List<Message> messages = messageTypes?.Select(CreateMessage).ToList();

            Status result = _domainStatusEvaluator.GetStatus(messages);

            Assert.AreEqual(result, expectedStatus);
        }

        private Message CreateMessage(MessageType messageType)
        {
            return new Message(Guid.Empty, "mailcheck.dmarc.testName", null, messageType, "", "");
        }
    }
}
