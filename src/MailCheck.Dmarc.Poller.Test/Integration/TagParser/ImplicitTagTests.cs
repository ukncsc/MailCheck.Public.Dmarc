using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.Poller;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class ImplicitTagTests : BasePollHandlerTests
    {
        [Test]
        public async Task DefaultTagsArePopulated()
        {
            SetUpTxtRecords("v=DMARC1;p=none;");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            DmarcRecordsPolled dispatchedMessage = GetDispatchedMessage();
            List<Tag> tags = dispatchedMessage.Records.Records[0].Tags;
            Assert.AreEqual(9, tags.Count);

            Assert.AreEqual(TagType.Version, tags[0].TagType);
            Assert.AreEqual("v=DMARC1;", tags[0].Value);

            Assert.AreEqual(TagType.Policy, tags[1].TagType);
            Assert.AreEqual("p=none;", tags[1].Value);

            Assert.AreEqual(TagType.ReportInterval, tags[2].TagType);
            Assert.AreEqual("ri=86400;", tags[2].Value);

            Assert.AreEqual(TagType.ReportFormat, tags[3].TagType);
            Assert.AreEqual("rf=AFRF;", tags[3].Value);

            Assert.AreEqual(TagType.Percent, tags[4].TagType);
            Assert.AreEqual("pct=100;", tags[4].Value);

            Assert.AreEqual(TagType.FailureOption, tags[5].TagType);
            Assert.AreEqual("fo=0;", tags[5].Value);

            Assert.AreEqual(TagType.Aspf, tags[6].TagType);
            Assert.AreEqual("aspf=r;", tags[6].Value);

            Assert.AreEqual(TagType.Adkim, tags[7].TagType);
            Assert.AreEqual("adkim=r;", tags[7].Value);

            Assert.AreEqual(TagType.SubDomainPolicy, tags[8].TagType);
            Assert.AreEqual("sp=none;", tags[8].Value);

        }
    }
}