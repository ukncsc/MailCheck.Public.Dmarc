using System;
using System.Collections.Generic;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Evaluator.Util;
using NUnit.Framework;

namespace MailCheck.Dmarc.Evaluator.Test.Util
{
    [TestFixture]
    public class DmarcRecordExtensionMethodTests
    {

        public static readonly SubDomainPolicy SubDomainPolicyNone = new SubDomainPolicy(string.Empty, PolicyType.None, true);
        public static readonly Policy PolicyQuarantine = new Policy(string.Empty, PolicyType.Quarantine, true);
        public static readonly Policy PolicyNone = new Policy(string.Empty, PolicyType.None, true);

        [TestCaseSource("CreateTestData")]
        public void Test(Policy policy, SubDomainPolicy subDomainPolicy, bool isInherited, Policy expectedPolicy)
        {
            DmarcRecord record = CreateDmarcRecord(policy, subDomainPolicy, isInherited);

            Policy effectivePolicy = record.GetEffectivePolicy();

            Assert.That(effectivePolicy, Is.EqualTo(expectedPolicy));
        }

        public static IEnumerable<TestCaseData> CreateTestData()
        {
            yield return new TestCaseData(PolicyQuarantine, SubDomainPolicyNone, false, PolicyQuarantine).SetName("EffectivePolicyIsPolicyIfNotInherited");
            yield return new TestCaseData(PolicyQuarantine, SubDomainPolicyNone, true, PolicyNone).SetName("EffectivePolicyIsSubPolicyIfInheritedWithSubPolicy");
            yield return new TestCaseData(PolicyQuarantine, null, true, PolicyQuarantine).SetName("EffectivePolicyIsPolicyIfInheritedWithoutSubPolicy");
        }

        private DmarcRecord CreateDmarcRecord(Policy policy, SubDomainPolicy subDomainPolicy, bool isInherited)
        {
            return new DmarcRecord(string.Empty, new List<Tag> { policy, subDomainPolicy }, new List<Message>(), string.Empty, String.Empty, false, isInherited);
        }
    }
}
