using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Rules.Record
{
    [TestFixture]
    public class MaxLengthOf450CharactersTests
    {
        private const string RecordOk = "record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record re";
        private const string RecordTooLong = "record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record record rec";

        private MaxLengthOf450Characters _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new MaxLengthOf450Characters();
        }

        [TestCase(RecordOk, false, TestName = "No error for string of 450 characters")]
        [TestCase(RecordTooLong, true, TestName = "Error for string of 451 characters")]
        public async Task Test(string record, bool isErrorExpected)
        {
            DmarcRecord dmarcRecord = new DmarcRecord(record, new List<Tag>(), string.Empty, string.Empty, false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);
            
            Assert.That(errors.Any, Is.EqualTo(isErrorExpected));
            Assert.That(errors.FirstOrDefault(), isErrorExpected ? Is.Not.Null : Is.Null);
        }
    }
}
