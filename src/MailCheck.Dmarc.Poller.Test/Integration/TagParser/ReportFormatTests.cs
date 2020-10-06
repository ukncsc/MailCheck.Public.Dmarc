using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class ReportFormatTests : BasePollHandlerTests
    {
        [TestCase("afrf", ReportFormatType.AFRF)]
        [TestCase("afrf ", ReportFormatType.AFRF)]
        [TestCase(" afrf", ReportFormatType.AFRF)]
        [TestCase("AFRF", ReportFormatType.AFRF)]
        public async Task ReportFormatWithValidValue(string validValue, ReportFormatType expectedAlignmentType)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;rf={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportFormat rfTag = GetTags<ReportFormat>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.AreEqual(expectedAlignmentType, rfTag.ReportFormatType);
            Assert.True(rfTag.Valid);
        }

        [TestCase("none=none")]
        [TestCase("999")]
        [TestCase("abc")]
        [TestCase("")]
        [TestCase(null)]
        public async Task ReportFormatWithInvalidValue(string invalidValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;rf={invalidValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportFormat rfTag = GetTags<ReportFormat>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual($"Invalid rf value: {invalidValue}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.AreEqual(ReportFormatType.Unknown, rfTag.ReportFormatType);
            Assert.False(rfTag.Valid);
        }

        [Test]
        public async Task ReportFormatShouldOnlyAppearOnce()
        {
            SetUpTxtRecords("v=DMARC1;p=none;rf=afrf;rf=afrf;");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            List<Message> advisories = GetAdvisories();
            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual("The rf tag should occur no more than once. This record has 2 occurrences.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
        }
    }
}