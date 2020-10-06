using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class PolicyTests : BasePollHandlerTests
    {
        [TestCase("none", PolicyType.None)]
        [TestCase("none ", PolicyType.None)]
        [TestCase(" none", PolicyType.None)]
        [TestCase("NONE", PolicyType.None)]
        [TestCase("reject", PolicyType.Reject)]
        [TestCase("reject ", PolicyType.Reject)]
        [TestCase(" reject", PolicyType.Reject)]
        [TestCase("REJECT", PolicyType.Reject)]
        [TestCase("quarantine", PolicyType.Quarantine)]
        [TestCase("quarantine ", PolicyType.Quarantine)]
        [TestCase(" quarantine", PolicyType.Quarantine)]
        [TestCase("QUARANTINE", PolicyType.Quarantine)]
        public async Task PolicyWithValidValue(string validValue, PolicyType expectedAlignmentType)
        {
            SetUpTxtRecords($"v=DMARC1;p={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            Policy pTag = GetTags<Policy>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.AreEqual(expectedAlignmentType, pTag.PolicyType);
            Assert.True(pTag.Valid);
        }

        [TestCase("none=none")]
        [TestCase("999")]
        [TestCase("abc")]
        [TestCase("")]
        [TestCase(null)]
        public async Task PolicyWithInvalidValue(string invalidValue)
        {
            SetUpTxtRecords($"v=DMARC1;p={invalidValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            Policy pTag = GetTags<Policy>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual($"Invalid p value: {invalidValue}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.AreEqual(PolicyType.Unknown, pTag.PolicyType);
            Assert.False(pTag.Valid);
        }

        [Test]
        public async Task PolicyShouldOnlyAppearOnce()
        {
            SetUpTxtRecords("v=DMARC1;p=none;p=none;");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            List<Message> advisories = GetAdvisories();
            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual("The p tag should occur no more than once. This record has 2 occurrences.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
        }
    }
}