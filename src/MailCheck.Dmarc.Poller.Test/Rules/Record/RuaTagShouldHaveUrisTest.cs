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
    public class RuaTagShouldHaveUrisTest
    {
        private RuaTagShouldHaveUris _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new RuaTagShouldHaveUris("Test error message", string.Empty);
        }

        [TestCase(0, true, TestName = "Error when 0 Uris")]
        [TestCase(1, false, TestName = "No error when 1 Uri")]
        [TestCase(2, false, TestName = "No error when 2 Uris")]
        public async Task Test(int tagCount, bool isErrorExpected)
        {
            List<UriTag> uriTags = Enumerable.Range(0, tagCount)
                .Select(_ => new UriTag("", new DmarcUri(new Uri("mailto:a@b.com")), new MaxReportSize(1000, Unit.K))).ToList();
            ReportUriAggregate reportUriAggregate = new ReportUriAggregate("", uriTags);
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { reportUriAggregate }, string.Empty, string.Empty, false, false);

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
    }
}
