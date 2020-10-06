using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Rules.Record;
using NUnit.Framework;
using Version = MailCheck.Dmarc.Poller.Domain.Version;

namespace MailCheck.Dmarc.Poller.Test.Rules.Record
{
    [TestFixture]
    public class VersionMustBeFirstTagTests
    {
        private VersionMustBeFirstTag _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new VersionMustBeFirstTag();
        }

        [Test]
        public async Task VersionIsFirstTagNoError()
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { new Version("v=DMARC1") }, string.Empty, string.Empty, false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.False);
        }

        [Test]
        public async Task NoVersionTagError()
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag>(), string.Empty, string.Empty, false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.True);
        }

        [Test]
        public async Task FirstTagNotVersionMarkdownErrorMessage()
        {
            string policy = "p=none";

            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { new Policy(policy, PolicyType.None) }, string.Empty, string.Empty, false, false);

            string expectedMarkdown = $"A DMARC record must always start with v=DMARC1 to be valid.{Environment.NewLine}This record starts with {policy}; which is a misconfiguration."; ;

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.True);

            Assert.That(errors.First().Markdown, Is.EqualTo(expectedMarkdown));
        }

        [Test]
        public async Task VersionTagIsNotFirstError()
        {
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag>
            {
                new SubDomainPolicy("", PolicyType.None),
                new Version("v=DMARC1")
            }, string.Empty, string.Empty, false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.True);
        }
    }
}
