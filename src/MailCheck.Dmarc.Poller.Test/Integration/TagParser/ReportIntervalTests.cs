using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class ReportIntervalTests : BasePollHandlerTests
    {
        [TestCase("18")]
        [TestCase("18 ")]
        [TestCase(" 18")]
        public async Task ReportIntervalWithValidValue(string validValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;ri={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportInterval riTag = GetTags<ReportInterval>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.AreEqual(18, riTag.Interval);
            Assert.True(riTag.Valid);
        }

        [TestCase("18=18")]
        [TestCase("-18")]
        [TestCase("abc")]
        [TestCase("")]
        [TestCase(null)]
        public async Task ReportIntervalWithInvalidValue(string invalidValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;ri={invalidValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportInterval riTag = GetTags<ReportInterval>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual($"Invalid ri value: {invalidValue}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.Null(riTag.Interval);
            Assert.False(riTag.Valid);
        }

        [Test]
        public async Task ReportIntervalShouldOnlyAppearOnce()
        {
            SetUpTxtRecords($"v=DMARC1;p=none;ri=18;ri=18");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            List<Message> advisories = GetAdvisories();
            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual("The ri tag should occur no more than once. This record has 2 occurrences.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
        }
    }
}