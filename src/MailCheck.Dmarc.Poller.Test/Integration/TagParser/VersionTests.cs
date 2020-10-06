using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Poller.Exceptions;
using NUnit.Framework;
using Version = MailCheck.Dmarc.Contracts.SharedDomain.Version;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class VersionTests : BasePollHandlerTests
    {
        [TestCase("DMARC1")]
        [TestCase("DMARC1 ")]
        [TestCase("dmarc1")]
        [TestCase("dmarc1 ")]
        public async Task VersionWithValidValue(string validValue)
        {
            SetUpTxtRecords($"v={validValue};p=none");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            Version versionTag = GetTags<Version>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.True(versionTag.Valid);
        }

        [TestCase("DMARC")]
        [TestCase("DMARC2")]
        public async Task VersionWithInvalidDmarcValue(string invalidValue)
        {
            SetUpTxtRecords($"v={invalidValue};p=none");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            Version versionTag = GetTags<Version>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual($"Invalid v value: {invalidValue}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.False(versionTag.Valid);
        }

        [Test]
        public async Task VersionWithRepeatedValue()
        {
            SetUpTxtRecords("v=DMARC1=DMARC1;p=none");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            Version versionTag = GetTags<Version>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual("Unexpected values DMARC1=DMARC1 in term v=DMARC1=DMARC1.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.False(versionTag.Valid);
        }

        [TestCase("18")]
        [TestCase("-18")]
        [TestCase("abc")]
        [TestCase("")]
        [TestCase(" dmarc1")]
        [TestCase(" DMARC1")]
        [TestCase(null)]
        public void VersionWithInvalidValue(string invalidValue)
        {
            SetUpTxtRecords($"v={invalidValue}; p=none");

            DmarcPollerException exception = Assert.ThrowsAsync<DmarcPollerException>(() => Handler.Handle(new DmarcPollPending("test.gov.uk")));

            Assert.AreEqual("Unable to retrieve dmarc records for test.gov.uk.", exception.Message);
        }

        [Test]
        public async Task VersionShouldOnlyAppearOnce()
        {
            SetUpTxtRecords($"v=DMARC1;v=DMARC1;p=none");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual("The v tag should occur no more than once. This record has 2 occurrences.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
        }
    }
}