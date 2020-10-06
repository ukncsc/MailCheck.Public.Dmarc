using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Rules.Records;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Rules.Records
{
    [TestFixture]
    public class TldDmarcRecordBehaviourIsWeaklyDefinedTests
    {
        private TldDmarcRecordBehaviourIsWeaklyDefined _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new TldDmarcRecordBehaviourIsWeaklyDefined();
        }

        [Test]
        public async Task ATldWithADmarcRecordShouldFail()
        {
            DmarcRecords records = CreateRecords("gov.uk", true, true);
            List<Error> errors = await _rule.Evaluate(records);

            Assert.That(errors.Any, Is.EqualTo(true));
            Assert.AreEqual(errors.First().ErrorType, ErrorType.Warning);
        }

        [Test]
        public async Task ATldWithNoDmarcRecordShouldPass()
        {
            DmarcRecords records = CreateRecords("gov.uk", false, true);
            List<Error> errors = await _rule.Evaluate(records);

            Assert.That(errors.Any, Is.False);
        }

        [Test]
        public async Task ANonTldWithADmarcRecordShouldPass()
        {
            DmarcRecords records = CreateRecords("ncsc.gov.uk", true);
            List<Error> errors = await _rule.Evaluate(records);

            Assert.That(errors.Any, Is.False);
        }

        [Test]
        public async Task ANonTldWithNoDmarcRecordShouldPass()
        {
            DmarcRecords records = CreateRecords("ncsc.gov.uk");
            List<Error> errors = await _rule.Evaluate(records);

            Assert.That(errors.Any, Is.False);
        }

        private static DmarcRecords CreateRecords(string domain, bool hasRecord = false, bool isTld = false, bool isInherited = false)
        {
            List<DmarcRecord> records = hasRecord
                ? new List<DmarcRecord> { new DmarcRecord("v=DMARC1;", A.Fake<List<Tag>>(), domain, domain, isTld, isInherited) }
                : new List<DmarcRecord>();

            return new DmarcRecords(domain, records, 0, domain, isTld, isInherited);
        }
    }
}
