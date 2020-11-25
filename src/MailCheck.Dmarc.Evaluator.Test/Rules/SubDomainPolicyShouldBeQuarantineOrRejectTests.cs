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

        [TestCase(PolicyType.Unknown, false, "abc.com", false, TestName = "No error for unknown policy type on non-inherited record.")]
        [TestCase(PolicyType.Quarantine, false, "abc.com", false, TestName = "No error for quarantine policy type on non-inherited record.")]
        [TestCase(PolicyType.Reject, false, "abc.com", false, TestName = "No error for reject policy type on non-inherited record.")]
        [TestCase(PolicyType.None, true, "abc.com", false, TestName = "Error for none policy type on non-inherited record.")]
        [TestCase(PolicyType.Unknown, false, "xyz.abc.com", true, TestName = "No error for unknown policy type on inherited record.")]
        [TestCase(PolicyType.Quarantine, false, "xyz.abc.com", true, TestName = "No error for quarantine policy type on inherited record.")]
        [TestCase(PolicyType.Reject, false, "xyz.abc.com", true, TestName = "No error for reject policy type on inherited record.")]
        [TestCase(PolicyType.None, false, "xyz.abc.com", true, TestName = "No error for none policy type on inherited record.")]
        public async Task NoErrorWhenPolicyTermNotFound(PolicyType policyType, bool isErrorExpected, string domain, bool isInherited)
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { new SubDomainPolicy("", policyType, true), new Policy("", PolicyType.Unknown, true) }, new List<Message>(), domain, "abc.com", false, isInherited);

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

        [Test]
        public async Task NoErrorOnDoubleNone()
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { new Policy("", PolicyType.None, true), 
                new SubDomainPolicy("", PolicyType.None, true) }, new List<Message>(), "abc.com", string.Empty, false, false);

            List<Message> results = await _rule.Evaluate(dmarcRecord);
            Assert.That(results.Any(), Is.False);
        }
    }
}
