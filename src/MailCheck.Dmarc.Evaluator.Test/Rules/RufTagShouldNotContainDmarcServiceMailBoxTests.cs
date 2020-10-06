using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Evaluator.Rules;
using NUnit.Framework;

namespace MailCheck.Dmarc.Evaluator.Test.Rules
{
    [TestFixture]
    public class RufTagShouldNotContainDmarcServiceMailBoxTests
    {
        private RufTagShouldNotContainDmarcServiceMailBox _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new RufTagShouldNotContainDmarcServiceMailBox();
        }

        [Test]
        public async Task NoMailCheckMailBoxInRufNoError()
        {
            DmarcRecord record = CreateDmarcRecord(CreateReportUriForensic(new Uri("mailto:abc@abc.com")));

            List<Message> messages = await _rule.Evaluate(record);

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public async Task MultipleForensicTagsNoError()
        {
            DmarcRecord record = CreateDmarcRecord(
                CreateReportUriForensic(new Uri("mailto:abc@dmarc.service.gov.uk")),
                CreateReportUriForensic(new Uri("mailto:abc@dmarc.service.gov.uk")));

            List<Message> messages = await _rule.Evaluate(record);

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public async Task MailCheckMailBoxInRufError()
        {
            DmarcRecord record = CreateDmarcRecord(
                CreateReportUriForensic(new Uri("mailto:abc@dmarc.service.gov.uk")));

            List<Message> messages = await _rule.Evaluate(record);

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages.First().Text, Is.EqualTo(DmarcRulesResource.RufTagShouldNotContainDmarcServiceMailBoxErrorMessage));
        }

        [Test]
        public async Task MultipleMailCheckMailBoxesInRufError()
        {
            DmarcRecord record = CreateDmarcRecord(
                CreateReportUriForensic(
                    new Uri("mailto:abc@dmarc.service.gov.uk"),
                    new Uri("mailto:def@dmarc.service.gov.uk")));

            List<Message> messages = await _rule.Evaluate(record);

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages.First().Text, Is.EqualTo(DmarcRulesResource.RufTagShouldNotContainDmarcServiceMailBoxErrorMessage));
        }



        private static DmarcRecord CreateDmarcRecord(params Tag[] tags)
        {
            return new DmarcRecord("", tags.ToList(), new List<Message>(), string.Empty, string.Empty, false, false);
        }

        private static ReportUriForensic CreateReportUriForensic(params Uri[] uris)
        {
            return new ReportUriForensic("",
                uris?.Select(_ => new UriTag("", new DmarcUri(_, true), new MaxReportSize(1000, Unit.K, true), true))
                    .ToList(), true);
        }
    }
}
