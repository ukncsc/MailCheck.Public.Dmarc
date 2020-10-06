using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Rules.Record
{
    [TestFixture]
    public class RufTagShouldNotHaveMoreThanTwoUrisTest
    {
        private RufTagShouldNotHaveMoreThanTwoUris _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new RufTagShouldNotHaveMoreThanTwoUris();
        }

        [TestCase(0, false, TestName = "No error when 0 Uris")]
        [TestCase(1, false, TestName = "No error when 1 Uri")]
        [TestCase(2, false, TestName = "No error when 2 Uris")]
        [TestCase(3, true, TestName = "Error when 3 Uris")]
        public async Task Test(int tagCount, bool isErrorExpected)
        {
            List<UriTag> uriTags = Enumerable.Range(0, tagCount)
                .Select(_ => new UriTag("", new DmarcUri(new Uri("mailto:a@b.com")), new MaxReportSize(1000, Unit.K))).ToList();
            ReportUriForensic reportUriForensic = new ReportUriForensic("", uriTags);
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { reportUriForensic }, string.Empty, string.Empty, false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.EqualTo(isErrorExpected));
            Assert.That(errors.FirstOrDefault(), isErrorExpected ? Is.Not.Null : Is.Null);
        }

        [Test]
        public async Task NoErrorWhenRuaTermNotFound()
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag>(), string.Empty, string.Empty, false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.False);
        }

        [Test]
        public async Task MarkdownErrorMessage()
        {
            List<UriTag> uriTags = Enumerable.Range(0, 3)
                .Select(_ => new UriTag("", new DmarcUri(new Uri("mailto:a@b.com")), new MaxReportSize(1000, Unit.K))).ToList();

            string expectedMarkdown = $"This DMARC record has requested mail servers to send forensic reports to 3 destinations."
               + $"{Environment.NewLine}{Environment.NewLine}Please be aware that email servers are only obliged to deliver to the first two."
               + $"{Environment.NewLine}Any extra destinations may get a random selection of reports or none at all."
               + $"{Environment.NewLine}{Environment.NewLine}[From the DMARC RFC:](https://tools.ietf.org/html/rfc7489#section-6.2)"
               + $"{Environment.NewLine}{Environment.NewLine}`A report is normally sent to each listed URI in the order provided by the Domain Owner. Receivers MAY impose a limit on the number of URIs to which they will send reports but MUST support the ability to send to at least two.`";

            ReportUriForensic reportUriForensic = new ReportUriForensic("", uriTags);

            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { reportUriForensic }, string.Empty, string.Empty, false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.True);
            
            Assert.That(errors.First().Markdown, Is.EqualTo(expectedMarkdown));
        }
    }
}
