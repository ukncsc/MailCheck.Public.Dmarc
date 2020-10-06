using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Evaluator.Rules;
using NUnit.Framework;

namespace MailCheck.Dmarc.Evaluator.Test.Rules
{
    [TestFixture]
    public class PctValueShouldBe100Tests
    {
        private PctValueShouldBe100 _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new PctValueShouldBe100();
        }

        [TestCase(null, false, TestName = "No error when pct value is null")]
        [TestCase(100, false, TestName = "No error when pct value is 100")]
        [TestCase(99, true, TestName = "Error when pct value is not 100")]
        public async Task Test(int? percent, bool isErrorExpected)
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { new Percent("", percent, true) }, new List<Message>(), string.Empty, string.Empty, false, false);

            List<Message> messages = await _rule.Evaluate(dmarcRecord);

            Assert.That(messages.Any(), Is.EqualTo(isErrorExpected));
            Assert.That(messages.FirstOrDefault(), isErrorExpected ? Is.Not.Null : Is.Null);
        }

        [Test]
        public async Task NoErrorWhenPercentTermNotFound()
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag>(), new List<Message>(), string.Empty, string.Empty, false, false);

            List<Message> messages = await _rule.Evaluate(dmarcRecord);

            Assert.That(messages.Any(), Is.False);
        }
    }
}
