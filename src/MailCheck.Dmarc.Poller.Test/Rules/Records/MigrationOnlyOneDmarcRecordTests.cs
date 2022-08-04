using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Rules;
using MailCheck.Dmarc.Poller.Rules.Records;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Rules.Records
{
    [TestFixture]
    public class MigrationOnlyOneDmarcRecordTests
    {
        private MigrationOnlyOneDmarcRecord _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new MigrationOnlyOneDmarcRecord();
        }

        [Test]
        public async Task WhenThereIsOnlyOneDmarcRecordNoErrorMessage()
        {
            DmarcRecords dmarcRecords =
                new DmarcRecords(string.Empty, new List<DmarcRecord> { new DmarcRecord("", new List<Tag>(), string.Empty, string.Empty, false, false) }, 0, string.Empty);

            List<Error> errors = await _rule.Evaluate(dmarcRecords);

            Assert.That(errors.Any, Is.EqualTo(false));
        }

        [Test]
        public async Task WhenThereIsMoreThanOneDmarcRecordAErrorMessageIsReturned()
        {
            List<DmarcRecord> records = Enumerable.Range(0, 3).Select(_ => new DmarcRecord("", new List<Tag>(), string.Empty, string.Empty, false, false)).ToList();
            DmarcRecords dmarcRecords = new DmarcRecords(string.Empty, records, 0, string.Empty);

            List<Error> errors = await _rule.Evaluate(dmarcRecords);

            Assert.That(errors.Any, Is.EqualTo(true));
            Assert.That(errors.First().Message, Is.EqualTo(DmarcRulesResource.OnlyOneDmarcRecordErrorMessage));
        }

        [Test]
        public async Task WhenThereIsNoDmarcRecordAHelpfulMessageShouldBeReturned()
        {
            const string domain = "abc.gov.uk";
            DmarcRecords dmarcRecords = new DmarcRecords(domain, new List<DmarcRecord>(), 0, string.Empty);

            List<Error> errors = await _rule.Evaluate(dmarcRecords);

            Assert.That(errors.Any, Is.EqualTo(true));
            Assert.That(errors.First().Message, Is.EqualTo(string.Format(DmarcRulesResource.NoDmarcErrorMessage, domain)));
            Assert.That(errors.First().Markdown, Is.EqualTo(string.Format(DmarcRulesMarkdownResource.MigrationNoDmarcErrorMessage, domain)));
            StringAssert.Contains("_dmarc.abc.gov.uk TXT v=DMARC1; p=none; rua=mailto:dmarc-rua@dmarc.service.gov.uk;", errors.First().Markdown);
            StringAssert.Contains("_dmarc.abc.gov.uk TXT v=DMARC1; p=none; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk;", errors.First().Markdown);
        }

        [Test]
        public async Task WhenDmarcRecordArePresentOnATldThereShouldBeNoError()
        {
            List<DmarcRecord> records = Enumerable.Range(0, 3).Select(_ => new DmarcRecord("", new List<Tag>(), string.Empty, string.Empty, true, false)).ToList();
            DmarcRecords dmarcRecords = new DmarcRecords(string.Empty, records, 0, string.Empty, true, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecords);

            Assert.That(errors.Any, Is.EqualTo(false));
        }
    }
}
