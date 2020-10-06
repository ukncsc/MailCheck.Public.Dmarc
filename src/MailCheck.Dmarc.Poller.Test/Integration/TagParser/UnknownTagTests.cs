using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class UnknownTagTests : BasePollHandlerTests
    {
        [Test]
        public async Task UnknownTagGivesError()
        {
            SetUpTxtRecords("v=DMARC1;p=none;unknownThing=150");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            UnknownTag unknownTag = GetTags<UnknownTag>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual("Unknown tag unknownThing with value 150.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.False(unknownTag.Valid);
        }

        [Test]
        public async Task MalformedUnknownTagGivesError()
        {
            SetUpTxtRecords("v=DMARC1;p=none;unknownThing==150");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            UnknownTag unknownTag = GetTags<UnknownTag>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual("Unknown tag unknownThing with value 150.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.False(unknownTag.Valid);
        }

        [Test]
        public async Task EmptyUnknownTagGivesError()
        {
            SetUpTxtRecords("v=DMARC1;p=none;unknownThing=");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            UnknownTag unknownTag = GetTags<UnknownTag>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual("Unknown tag unknownThing with value <null>.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
            Assert.False(unknownTag.Valid);
        }

        [Test]
        public async Task EmptyTagGivesNoError()
        {
            SetUpTxtRecords("v=DMARC1;p=none;;");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));
            
            List<Message> advisories = GetAdvisories();
            Assert.Zero(advisories.Count);
        }
    }
}