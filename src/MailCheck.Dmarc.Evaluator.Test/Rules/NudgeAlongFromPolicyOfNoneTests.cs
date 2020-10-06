using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Dkim.Client;
using MailCheck.Dkim.Client.Domain;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Evaluator.Rules;
using MailCheck.Spf.Client;
using MailCheck.Spf.Client.Domain;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;
using MessageType = MailCheck.Dmarc.Contracts.SharedDomain.MessageType;

namespace MailCheck.Dmarc.Evaluator.Test.Rules
{
    [TestFixture]
    public class NudgeAlongFromPolicyOfNoneTests
    {
        private NudgeAlongFromPolicyOfNone _rule;
        private ISpfClient _spfApiClient;
        private IDkimClient _dkimApiClient;
        private ILogger<NudgeAlongFromPolicyOfNone> _logger;

        [SetUp]
        public void SetUp()
        {
            _spfApiClient = A.Fake<ISpfClient>();
            _dkimApiClient = A.Fake<IDkimClient>();
            _logger = A.Fake<ILogger<NudgeAlongFromPolicyOfNone>>();
            _rule = new NudgeAlongFromPolicyOfNone(_spfApiClient, _dkimApiClient, _logger);
        }

        [TestCase(null, TestName = "No policy no error")]
        [TestCase(PolicyType.Unknown, TestName = "Unknown policy no error")]
        [TestCase(PolicyType.Quarantine, TestName = "Quarantine policy no error")]
        [TestCase(PolicyType.Reject, TestName = "Reject policy no error")]
        public async Task Test(PolicyType? policyType)
        {
            DmarcRecord record = CreateDmarcRecord(policyType);

            List<Message> results = await _rule.Evaluate(record);

            Assert.That(results.Any(), Is.False);
        }

        [Test]
        public async Task PolicyOfNoneAndSpfApiThrowsNoError()
        {
            DmarcRecord record = CreateDmarcRecord(PolicyType.None);

            A.CallTo(() => _spfApiClient.GetHistory(A<string>._)).Throws<Exception>();

            List<Message> results = await _rule.Evaluate(record);

            Assert.That(results.Any, Is.False);
        }

        [Test]
        public async Task PolicyOfNoneAndDkimApiThrowsNoError()
        {
            DmarcRecord record = CreateDmarcRecord(PolicyType.None);
            A.CallTo(() => _dkimApiClient.GetHistory(A<string>._)).Throws<Exception>();
            
            List<Message> results = await _rule.Evaluate(record);

            Assert.That(results.Any, Is.False);
        }

        [Test]
        public async Task PolicyOfNoneAndSpfHasNoLatestHistoryItemNoError()
        {
            DmarcRecord record = CreateDmarcRecord(PolicyType.None);

            A.CallTo(() => _spfApiClient.GetHistory(A<string>._))
                .Returns(new SpfHistoryResponse(HttpStatusCode.OK, new SpfHistory(string.Empty, new List<SpfHistoryItem>())));

            A.CallTo(() => _dkimApiClient.GetHistory(A<string>._))
                .Returns(new DkimHistoryResponse(HttpStatusCode.OK,
                    new DkimHistory(string.Empty, new List<DkimHistoryItem>
                    {
                        new DkimHistoryItem(DateTime.UtcNow.AddDays(-30), null, new List<DkimHistoryEntry>())
                    })));
            
            List<Message> results = await _rule.Evaluate(record);

            Assert.That(results.Any, Is.False);
        }

        [Test]
        public async Task PolicyOfNoneAndDkimHasNoLatestHistoryItemNoError()
        {
            DmarcRecord record = CreateDmarcRecord(PolicyType.None);

            A.CallTo(() => _spfApiClient.GetHistory(A<string>._))
                .Returns(new SpfHistoryResponse(HttpStatusCode.OK, new SpfHistory(string.Empty, new List<SpfHistoryItem>
                {
                    new SpfHistoryItem(DateTime.UtcNow.AddDays(-30), null, new List<string>())
                })));

            A.CallTo(() => _dkimApiClient.GetHistory(A<string>._))
                .Returns(new DkimHistoryResponse(HttpStatusCode.OK,
                    new DkimHistory(string.Empty, new List<DkimHistoryItem>())));
            
            List<Message> results = await _rule.Evaluate(record);
            Assert.That(results.Any, Is.False);
        }

        [Test]
        public async Task PolicyOfNoneAndSpfHasBeenChangedInLast30DaysNoError()
        {
            DmarcRecord record = CreateDmarcRecord(PolicyType.None);

            A.CallTo(() => _spfApiClient.GetHistory(A<string>._))
                .Returns(new SpfHistoryResponse(HttpStatusCode.OK, new SpfHistory(string.Empty, new List<SpfHistoryItem>
                {
                    new SpfHistoryItem(DateTime.UtcNow.AddDays(-29), null, new List<string>())
                })));

            A.CallTo(() => _dkimApiClient.GetHistory(A<string>._))
                .Returns(new DkimHistoryResponse(HttpStatusCode.OK,
                    new DkimHistory(string.Empty, new List<DkimHistoryItem>
                    {
                        new DkimHistoryItem(DateTime.UtcNow.AddDays(-30), null, new List<DkimHistoryEntry>())
                    })));

            List<Message> results = await _rule.Evaluate(record);
            Assert.That(results.Any, Is.False);
        }

        [Test]
        public async Task PolicyOfNoneAndDkimHasBeenChangedInLast30DaysNoError()
        {
            DmarcRecord record = CreateDmarcRecord(PolicyType.None);

            A.CallTo(() => _spfApiClient.GetHistory(A<string>._))
                .Returns(new SpfHistoryResponse(HttpStatusCode.OK, new SpfHistory(string.Empty, new List<SpfHistoryItem>
                {
                    new SpfHistoryItem(DateTime.UtcNow.AddDays(-30), null, new List<string>())
                })));

            A.CallTo(() => _dkimApiClient.GetHistory(A<string>._))
                .Returns(new DkimHistoryResponse(HttpStatusCode.OK,
                    new DkimHistory(string.Empty, new List<DkimHistoryItem>
                    {
                        new DkimHistoryItem(DateTime.UtcNow.AddDays(-29), null, new List<DkimHistoryEntry>())
                    })));

            List<Message> results = await _rule.Evaluate(record);
            Assert.That(results.Any, Is.False);
        }

        [Test]
        public async Task PolicyOfNoneAndSpfAndDkimNotChangedInLast30DaysError()
        {
            DmarcRecord record = CreateDmarcRecord(PolicyType.None);

            A.CallTo(() => _spfApiClient.GetHistory(A<string>._))
                .Returns(new SpfHistoryResponse(HttpStatusCode.OK, new SpfHistory(string.Empty, new List<SpfHistoryItem>
                {
                    new SpfHistoryItem(DateTime.UtcNow.AddDays(-30), null, new List<string>())
                })));

            A.CallTo(() => _dkimApiClient.GetHistory(A<string>._))
                .Returns(new DkimHistoryResponse(HttpStatusCode.OK,
                    new DkimHistory(string.Empty, new List<DkimHistoryItem>
                    {
                        new DkimHistoryItem(DateTime.UtcNow.AddDays(-30), null, new List<DkimHistoryEntry>())
                    })));
            
            List<Message> results = await _rule.Evaluate(record);
            Assert.That(results.Any, Is.True);
            Assert.That(results[0], Is.Not.Null);
            Assert.That(results[0].MessageType, Is.EqualTo(MessageType.info));
            Assert.That(results[0].MessageDisplay, Is.EqualTo(MessageDisplay.Prompt));
            Assert.That(results[0].Text, Is.EqualTo(DmarcRulesResource.NudgeAlongFromPolicyOfNoneMessage));
        }

        private DmarcRecord CreateDmarcRecord(PolicyType? policy = null)
        {
            return policy.HasValue
                ? new DmarcRecord("", new List<Tag> { new Policy("", policy.Value, true) }, new List<Message>(), "", "", false, false)
                : new DmarcRecord("", new List<Tag>(), new List<Message>(), "", "", false, false);
        }
    }
}
