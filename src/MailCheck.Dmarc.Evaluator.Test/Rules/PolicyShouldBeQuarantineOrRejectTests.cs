﻿using System.Collections.Generic;
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

        [TestCase(PolicyType.Unknown, PolicyType.Quarantine, false, null, TestName = "No error for quarantine subdomain policy type")]
        [TestCase(PolicyType.Unknown, PolicyType.Reject, false, null, TestName = "No error for reject subdomain policy type")]
        [TestCase(PolicyType.Unknown, PolicyType.None, true, "Weak sub-domain DMARC policy (sp=none). Your sub-domains (fake or real) can be used to send malicious spoofed emails.", TestName = "Error for none subdomain policy type")]
        [TestCase(PolicyType.Unknown, PolicyType.Unknown, false, null, TestName = "No error for unknown subdomain policy type")]
        public async Task SubdomainPolicySupercedesParentPolicy(PolicyType parentPolicy, PolicyType subdomainPolicy, bool isErrorExpected, string errorMessage)
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { new Policy("", parentPolicy, true), new SubDomainPolicy("", subdomainPolicy, true) },
                new List<Message>(), string.Empty, string.Empty, false, true);

            List<Message> messages = await _rule.Evaluate(dmarcRecord);

            Assert.That(messages.Any(), Is.EqualTo(isErrorExpected));

            Assert.That(messages.FirstOrDefault(), isErrorExpected ? Is.Not.Null : Is.Null);

            if(isErrorExpected)
            {
                Assert.That(messages.FirstOrDefault().Text, Is.EqualTo(errorMessage));
            }
        }
    }
}
