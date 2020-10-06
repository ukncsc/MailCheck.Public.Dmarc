using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Rules.Record
{
    [TestFixture]
    public class PolicyTagMustExistTests
    {
        private PolicyTagMustExist _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new PolicyTagMustExist();
        }

        [TestCase(true, false, TestName = "No error if policy tag exists")]
        [TestCase(false, true, TestName = "Error if policy tag doesnt exist")]
        public async Task Test(bool policyTagExists, bool isErrorExpected)
        {
            Policy policy = policyTagExists ? new Policy("", PolicyType.None) : null;
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { policy }, string.Empty, string.Empty, false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.EqualTo(isErrorExpected));
            Assert.That(errors.FirstOrDefault(), isErrorExpected ? Is.Not.Null : Is.Null);
        }
    }
}
