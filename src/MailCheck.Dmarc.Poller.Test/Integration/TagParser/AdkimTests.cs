using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class AdkimTests : BasePollHandlerTests
    {
        [TestCase("r", AlignmentType.R)]
        [TestCase("r ", AlignmentType.R)]
        [TestCase(" r", AlignmentType.R)]
        [TestCase("R", AlignmentType.R)]
        [TestCase("s", AlignmentType.S)]
        [TestCase("s ", AlignmentType.S)]
        [TestCase(" s", AlignmentType.S)]
        [TestCase("S", AlignmentType.S)]
        public async Task AdkimWithValidValue(string validValue, AlignmentType expectedAlignmentType)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;adkim={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            Adkim adkimTag = GetTags<Adkim>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.AreEqual(expectedAlignmentType, adkimTag.AlignmentType);
            Assert.True(adkimTag.Valid);
        }

        [TestCase("r=r")]
        [TestCase("999")]
        [TestCase("abc")]
        [TestCase("")]
        [TestCase(null)]
        public async Task AdkimWithInvalidValue(string invalidValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;adkim={invalidValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            Adkim adkimTag = GetTags<Adkim>().First();
            List<Message> advisories = GetAdvisories();
            
            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual($"Invalid adkim value: {invalidValue}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.AreEqual(AlignmentType.Unknown, adkimTag.AlignmentType);
            Assert.False(adkimTag.Valid);
        }

        [Test]
        public async Task AdkimShouldOnlyAppearOnce()
        {
            SetUpTxtRecords("v=DMARC1;p=none;adkim=r;adkim=r");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));
            
            List<Message> advisories = GetAdvisories();
            
            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual("The adkim tag should occur no more than once. This record has 2 occurrences.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
        }
    }
}