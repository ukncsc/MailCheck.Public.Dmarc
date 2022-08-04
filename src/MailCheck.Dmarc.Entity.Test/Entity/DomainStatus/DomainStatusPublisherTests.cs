using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.DomainStatus.Contracts;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Entity.Config;
using MailCheck.Dmarc.Entity.Entity.DomainStatus;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using A = FakeItEasy.A;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;

namespace MailCheck.Dmarc.Entity.Test.Entity.DomainStatus
{
    [TestFixture]
    public class DomainStatusPublisherTests
    {
        private DomainStatusPublisher _domainStatusPublisher;
        private IMessageDispatcher _dispatcher;
        private IDmarcEntityConfig _dmarcEntityConfig;
        private ILogger<DomainStatusPublisher> _logger;
        private IDomainStatusEvaluator _domainStatusEvaluator;

        [SetUp]
        public void SetUp()
        {
            _dispatcher = A.Fake<IMessageDispatcher>();
            _dmarcEntityConfig = A.Fake<IDmarcEntityConfig>();
            _logger = A.Fake<ILogger<DomainStatusPublisher>>();
            _domainStatusEvaluator = A.Fake<IDomainStatusEvaluator>();
            A.CallTo(() => _dmarcEntityConfig.SnsTopicArn).Returns("testSnsTopicArn");

            _domainStatusPublisher = new DomainStatusPublisher(_dispatcher, _dmarcEntityConfig, _domainStatusEvaluator, _logger);
        }

        [Test]
        public void StatusIsDeterminedAndDispatched()
        {
            Message rootMessage = CreateMessage();
            Message recordsMessage = CreateMessage();
            Message recordsRecordsMessages = CreateMessage();

            DmarcRecordsEvaluated message = new DmarcRecordsEvaluated("testDomain", new DmarcRecords("", new List<DmarcRecord> { new DmarcRecord(null, null, new List<Message> { recordsRecordsMessages }, "","",false,false) }, new List<Message> { recordsMessage }, 0), TimeSpan.MinValue, new List <Message> { rootMessage }, DateTime.MinValue);

            A.CallTo(() => _domainStatusEvaluator.GetStatus(A<List<Message>>.That.Matches(x => x.Contains(rootMessage) && x.Contains(recordsMessage) && x.Contains(recordsRecordsMessages)))).Returns(Status.Warning);

            _domainStatusPublisher.Publish(message);

            Expression<Func<DomainStatusEvaluation, bool>> predicate = x =>
                x.Status == Status.Warning &&
                x.RecordType == "DMARC" &&
                x.Id == "testDomain";

            A.CallTo(() => _dispatcher.Dispatch(A<DomainStatusEvaluation>.That.Matches(predicate), "testSnsTopicArn")).MustHaveHappenedOnceExactly();
        }

        private Message CreateMessage()
        {
            return new Message(Guid.Empty, "mailcheck.dmarc.testName", "", MessageType.error, "", "");
        }
    }
}
