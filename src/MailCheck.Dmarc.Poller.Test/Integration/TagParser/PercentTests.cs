using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class PercentTests : BasePollHandlerTests
    {
        [TestCase("18")]
        [TestCase("18 ")]
        [TestCase(" 18")]
        public async Task PercentWithValidValue(string validValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;pct={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            Percent pctTag = GetTags<Percent>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.AreEqual(18, pctTag.PercentValue);
            Assert.True(pctTag.Valid);
        }

        [TestCase("18=18")]
        [TestCase("-18")]
        [TestCase("abc")]
        [TestCase("")]
        [TestCase(null)]
        public async Task PercentWithInvalidValue(string invalidValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;pct={invalidValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            Percent pctTag = GetTags<Percent>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual($"Invalid pct value: {invalidValue}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.Null(pctTag.PercentValue);
            Assert.False(pctTag.Valid);
        }

        [Test]
        public async Task PercentShouldOnlyAppearOnce()
        {
            SetUpTxtRecords($"v=DMARC1;p=none;pct=18;pct=18");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            List<Message> advisories = GetAdvisories();
            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual("The pct tag should occur no more than once. This record has 2 occurrences.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories.First().MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
        }
    }
}