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
    public class RuaTagsShouldBeMailToTests
    {
        private RuaTagsShouldBeMailTo _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new RuaTagsShouldBeMailTo();
        }

        [TestCase(null, false, TestName = "No error when uri is null")]
        [TestCase("mailto:a@b.com", false, TestName = "No error when uri is mailto")]
        [TestCase("http://a.c.com", true, TestName = "Error when uri is not mailto")]
        [TestCase("a@c.com", true, TestName = "Error when email provided without mailto")]
        public async Task Test(string value, bool isErrorExpected)
        {
            Uri.TryCreate(value, UriKind.Absolute, out Uri uri);
            DmarcRecord dmarcRecord = new DmarcRecord("",
                new List<Tag>
                {
                    new ReportUriAggregate("",
                        new List<UriTag>
                        {
                            new UriTag(value ?? "", new DmarcUri(uri),
                                new MaxReportSize(1000, Unit.K))
                        })
                },
                string.Empty, string.Empty, false, false);
            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.EqualTo(isErrorExpected));

            Assert.That(errors.FirstOrDefault(), isErrorExpected ? Is.Not.Null : Is.Null);
        }
    }
}
