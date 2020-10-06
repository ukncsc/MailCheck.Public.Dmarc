using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class FailureOptionTests : BasePollHandlerTests
    {
        [TestCase("0", FailureOptionType.Zero)]
        [TestCase("0 ", FailureOptionType.Zero)]
        [TestCase("1", FailureOptionType.One)]
        [TestCase("1 ", FailureOptionType.One)]
        [TestCase("d", FailureOptionType.D)]
        [TestCase("d ", FailureOptionType.D)]
        [TestCase("D", FailureOptionType.D)]
        [TestCase("s", FailureOptionType.S)]
        [TestCase("s ", FailureOptionType.S)]
        [TestCase("S", FailureOptionType.S)]
        public async Task FailureOptionWithValidValue(string validValue, FailureOptionType expectedFailureOptionType)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;fo={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            FailureOption failureOptionTag = GetTags<FailureOption>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.AreEqual(expectedFailureOptionType, failureOptionTag.FailureOptionTypes.First());
            Assert.True(failureOptionTag.Valid);
        }

        [TestCase("d=d")]
        [TestCase("999")]
        [TestCase("abc")]
        [TestCase("")]
        [TestCase(null)]
        [TestCase(" 0")]
        [TestCase(" 1")]
        [TestCase(" d")]
        [TestCase(" s")]
        public async Task FailureOptionWithInvalidValue(string invalidValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;fo={invalidValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            FailureOption failureOptionTag = GetTags<FailureOption>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual($"Invalid fo value: {invalidValue}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.AreEqual(FailureOptionType.Unknown, failureOptionTag.FailureOptionTypes.First());
            Assert.False(failureOptionTag.Valid);
        }

        [Test]
        public async Task FailureOptionShouldOnlyAppearOnce()
        {
            SetUpTxtRecords("v=DMARC1;p=none;fo=0;fo=0");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            List<Message> advisories = GetAdvisories();
            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual("The fo tag should occur no more than once. This record has 2 occurrences.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
        }
    }
}