using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Evaluator.Rules;
using MailCheck.Spf.Client;
using MailCheck.Spf.Client.Domain;
using NUnit.Framework;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;
using MessageType = MailCheck.Dmarc.Contracts.SharedDomain.MessageType;

namespace MailCheck.Dmarc.Evaluator.Test.Rules
{
    [TestFixture]
    public class ShouldHaveSpfRecordWIthPolicyOfQuarantineOrRejectTests
    {
        private ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject _rule;
        private ISpfClient _spfClient;

        [SetUp]
        public void SetUp()
        {
            _spfClient = A.Fake<ISpfClient>();
            _rule = new ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject(_spfClient);
        }

        [Test]
        public async Task PolicyOfNoneNoError()
        {
            List<Message> results = await _rule.Evaluate(CreateDmarcRecord(PolicyType.None));

            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task PolicyOfQuarantineAndSpfRecordNoError()
        {
            A.CallTo(() => _spfClient.GetSpf(A<string>._))
                .Returns(new SpfResponse(HttpStatusCode.OK, CreateSpfRecord(new List<string>{"v=spf1..."})));

            List<Message> results = await _rule.Evaluate(CreateDmarcRecord(PolicyType.Quarantine));

            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task PolicyOfQuarantineAndNullSpfCausesError()
        {
            A.CallTo(() => _spfClient.GetSpf(A<string>._))
                .Returns(new SpfResponse(HttpStatusCode.OK, null));

            List<Message> results = await _rule.Evaluate(CreateDmarcRecord(PolicyType.Quarantine));

            Message message = results.FirstOrDefault();

            Assert.That(message, Is.Not.Null);
            Assert.That(message.MessageType, Is.EqualTo(MessageType.info));
            Assert.That(message.Text, Is.EqualTo(DmarcRulesResource.ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject));
        }

        [Test]
        public async Task PolicyOfQuarantineAndEmptySpfRecordsCausesError()
        {
            A.CallTo(() => _spfClient.GetSpf(A<string>._))
                .Returns(new SpfResponse(HttpStatusCode.OK, CreateSpfRecord()));

            List<Message> results = await _rule.Evaluate(CreateDmarcRecord(PolicyType.Quarantine));

            Message message = results.FirstOrDefault();

            Assert.That(message, Is.Not.Null);
            Assert.That(message.MessageType, Is.EqualTo(MessageType.info));
            Assert.That(message.Text, Is.EqualTo(DmarcRulesResource.ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject));
        }

        [Test]
        public async Task PolicyOfQuarantineAndEmptyRecordsCausesError()
        {
            A.CallTo(() => _spfClient.GetSpf(A<string>._))
                .Returns(new SpfResponse(HttpStatusCode.OK, CreateSpfRecord(new List<string>())));

            List<Message> results = await _rule.Evaluate(CreateDmarcRecord(PolicyType.Quarantine));

            Message message = results.FirstOrDefault();

            Assert.That(message, Is.Not.Null);
            Assert.That(message.MessageType, Is.EqualTo(MessageType.info));
            Assert.That(message.Text, Is.EqualTo(DmarcRulesResource.ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject));
        }

        [Test]
        public async Task PolicyOfQuarantineAndCouldNotRetrieveSpfRecordNoError()
        {
            A.CallTo(() => _spfClient.GetSpf(A<string>._))
                .Returns(new SpfResponse(HttpStatusCode.BadRequest, null));

            List<Message> results = await _rule.Evaluate(CreateDmarcRecord(PolicyType.Quarantine));

            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task PolicyOfRejectAndSpfRecordNoError()
        {
            A.CallTo(() => _spfClient.GetSpf(A<string>._))
                .Returns(new SpfResponse(HttpStatusCode.OK, CreateSpfRecord(new List<string> { "v=spf1..." })));

            List<Message> results = await _rule.Evaluate(CreateDmarcRecord(PolicyType.Reject));

            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task PolicyOfRejectAndNullSpfCausesError()
        {
            A.CallTo(() => _spfClient.GetSpf(A<string>._))
                .Returns(new SpfResponse(HttpStatusCode.OK, null));

            List<Message> results = await _rule.Evaluate(CreateDmarcRecord(PolicyType.Reject));

            Message message = results.FirstOrDefault();

            Assert.That(message, Is.Not.Null);
            Assert.That(message.MessageType, Is.EqualTo(MessageType.info));
            Assert.That(message.Text, Is.EqualTo(DmarcRulesResource.ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject));
        }

        [Test]
        public async Task PolicyOfRejectAndEmptySpfRecordsCausesError()
        {
            A.CallTo(() => _spfClient.GetSpf(A<string>._))
                .Returns(new SpfResponse(HttpStatusCode.OK, CreateSpfRecord()));

            List<Message> results = await _rule.Evaluate(CreateDmarcRecord(PolicyType.Reject));

            Message message = results.FirstOrDefault();

            Assert.That(message, Is.Not.Null);
            Assert.That(message.MessageType, Is.EqualTo(MessageType.info));
            Assert.That(message.Text, Is.EqualTo(DmarcRulesResource.ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject));
        }

        [Test]
        public async Task PolicyOfRejectAndEmptyRecordsCausesError()
        {
            A.CallTo(() => _spfClient.GetSpf(A<string>._))
                .Returns(new SpfResponse(HttpStatusCode.OK, CreateSpfRecord(new List<string>())));

            List<Message> results = await _rule.Evaluate(CreateDmarcRecord(PolicyType.Reject));

            Message message = results.FirstOrDefault();

            Assert.That(message, Is.Not.Null);
            Assert.That(message.MessageType, Is.EqualTo(MessageType.info));
            Assert.That(message.Text, Is.EqualTo(DmarcRulesResource.ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject));
        }

        [Test]
        public async Task PolicyOfRejectAndCouldNotRetrieveSpfRecordNoError()
        {
            A.CallTo(() => _spfClient.GetSpf(A<string>._))
                .Returns(new SpfResponse(HttpStatusCode.BadRequest, null));

            List<Message> results = await _rule.Evaluate(CreateDmarcRecord(PolicyType.Reject));

            Assert.That(results, Is.Empty);
        }

        private DmarcRecord CreateDmarcRecord(PolicyType policyType)
        {
            Policy policy = new Policy(string.Empty, policyType, true);
            return new DmarcRecord(string.Empty, new List<Tag>{policy}, new List<Message>(), string.Empty, String.Empty, false, false);
        }

        private Spf.Client.Domain.Spf CreateSpfRecord(List<string> records = null)
        {
            return records == null 
                ? new Spf.Client.Domain.Spf()
                : new Spf.Client.Domain.Spf
            {
                SpfRecords = new SpfRecords
                {
                    Records = records.Select((v,i) => new SpfRecord(i, v, null)).ToList()
                }
            };
        }
    }
}
