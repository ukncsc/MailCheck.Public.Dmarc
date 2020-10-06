using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class AspfTests : BasePollHandlerTests
    {
        [TestCase("r", AlignmentType.R)]
        [TestCase("r ", AlignmentType.R)]
        [TestCase(" r", AlignmentType.R)]
        [TestCase("R", AlignmentType.R)]
        [TestCase("s", AlignmentType.S)]
        [TestCase("s ", AlignmentType.S)]
        [TestCase(" s", AlignmentType.S)]
        [TestCase("S", AlignmentType.S)]
        public async Task AspfWithValidValue(string validValue, AlignmentType expectedAlignmentType)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;aspf={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            Aspf aspfTag = GetTags<Aspf>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.AreEqual(expectedAlignmentType, aspfTag.AlignmentType);
            Assert.True(aspfTag.Valid);
        }

        [TestCase("r=r")]
        [TestCase("999")]
        [TestCase("abc")]
        [TestCase("")]
        [TestCase(null)]
        public async Task AspfWithInvalidValue(string invalidValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;aspf={invalidValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            Aspf aspfTag = GetTags<Aspf>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual($"Invalid aspf value: {invalidValue}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.AreEqual(AlignmentType.Unknown, aspfTag.AlignmentType);
            Assert.False(aspfTag.Valid);
        }

        [Test]
        public async Task AspfShouldOnlyAppearOnce()
        {
            SetUpTxtRecords("v=DMARC1;p=none;aspf=r;aspf=r");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));
            
            List<Message> advisories = GetAdvisories();
            
            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual("The aspf tag should occur no more than once. This record has 2 occurrences.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
        }
    }
}