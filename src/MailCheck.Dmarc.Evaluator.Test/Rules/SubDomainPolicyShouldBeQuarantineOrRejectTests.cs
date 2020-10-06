using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Evaluator.Rules;
using NUnit.Framework;

namespace MailCheck.Dmarc.Evaluator.Test.Rules
{
    [TestFixture]
    public class SubDomainPolicyShouldBeQuarantineOrRejectTests
    {
        private SubDomainPolicyShouldBeQuarantineOrReject _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new SubDomainPolicyShouldBeQuarantineOrReject();
        }

        [TestCase(PolicyType.Unknown, false, "abc.com", TestName = "No error for unknown policy type on organisation domain.")]
        [TestCase(PolicyType.Quarantine, false, "abc.com", TestName = "No error for quarantine policy type on organisation domain.")]
        [TestCase(PolicyType.Reject, false, "abc.com", TestName = "No error for reject policy type on organisation domain.")]
        [TestCase(PolicyType.None, true, "abc.com", TestName = "Error for none policy type on organisation domain.")]
        [TestCase(PolicyType.Unknown, false, "xyz.abc.com", TestName = "No error for unknown policy type on non-organisation domain.")]
        [TestCase(PolicyType.Quarantine, false, "xyz.abc.com", TestName = "No error for quarantine policy type on non-organisation domain.")]
        [TestCase(PolicyType.Reject, false, "xyz.abc.com", TestName = "No error for reject policy type on non-organisation domain.")]
        [TestCase(PolicyType.None, false, "xyz.abc.com", TestName = "No error for none policy type on non-organisation domain.")]
        public async Task NoErrorWhenPolicyTermNotFound(PolicyType policyType, bool isErrorExpected, string domain)
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { new SubDomainPolicy("", policyType, true) }, new List<Message>(), domain, "abc.com", false, false);

            List<Message> results = await _rule.Evaluate(dmarcRecord);
            Assert.That(results.Any(), Is.EqualTo(isErrorExpected));
            Assert.That(results.FirstOrDefault(), isErrorExpected ? Is.Not.Null : Is.Null);
        }

        [Test]
        public async Task NoErrorWhenPolicyTermNotFound()
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag>(), new List<Message>(), "abc.com", string.Empty, false, false);
            
            List<Message> results = await _rule.Evaluate(dmarcRecord);
            Assert.That(results.Any(), Is.False);
        }
    }
}
