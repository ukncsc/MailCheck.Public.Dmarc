using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Evaluator.Rules;
using NUnit.Framework;

namespace MailCheck.Dmarc.Evaluator.Test.Rules
{
    [TestFixture]
    public class PolicyShouldBeQuarantineOrRejectTests
    {
        private PolicyShouldBeQuarantineOrReject _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new PolicyShouldBeQuarantineOrReject();
        }

        [TestCase(PolicyType.Unknown, false, TestName = "No error for unknown policy type")]
        [TestCase(PolicyType.Quarantine, false, TestName = "No error for quarantine policy type")]
        [TestCase(PolicyType.Reject, false, TestName = "No error for reject policy type")]
        [TestCase(PolicyType.None, true, TestName = "Error for none policy type")]
        public async Task Test(PolicyType policyType, bool isErrorExpected)
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { new Policy("", policyType, true) }, new List<Message>(), string.Empty, string.Empty, false, false);

            List<Message> messages = await _rule.Evaluate(dmarcRecord);

            Assert.That(messages.Any(), Is.EqualTo(isErrorExpected));

            Assert.That(messages.FirstOrDefault(), isErrorExpected ? Is.Not.Null : Is.Null);
        }

        [Test]
        public async Task NoErrorWhenPolicyTermNotFound()
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag>(), new List<Message>(), string.Empty, string.Empty, false, false);

            List<Message> messages = await _rule.Evaluate(dmarcRecord);

            Assert.That(messages.Any(), Is.False);
        }
    }
}
