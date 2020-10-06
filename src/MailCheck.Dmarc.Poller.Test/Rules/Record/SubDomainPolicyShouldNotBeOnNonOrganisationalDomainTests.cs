using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Rules.Record
{
    [TestFixture]
    public class SubDomainPolicyShouldNotBeOnNonOrganisationalDomainTests
    {
        private SubDomainPolicyShouldNotBeOnNonOrganisationalDomain _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new SubDomainPolicyShouldNotBeOnNonOrganisationalDomain();
        }

        [Test]
        public async Task NoErrorWhenOnOrganisationalDomain()
        {
            const string domain = "abc.com";
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { new SubDomainPolicy("", PolicyType.Unknown) }, domain, domain, false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.EqualTo(false));
        }

        [Test]
        public async Task NoErrorWhenRecordInherited()
        {
            const string domain = "abc.com";
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { new SubDomainPolicy("", PolicyType.Unknown) }, domain, domain, false, true);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.EqualTo(false));
        }

        [Test]
        public async Task NoErrorWhenNoSubDomainPolicyAndNonOrganisationalDomain()
        {
            const string domain = "abc.com";
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag>(), domain, domain, false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.EqualTo(false));
        }

        [Test]
        public async Task NoErrorWhenOnNonOrganisationalDomainIsImplicit()
        {
            const string domain = "abc.com";
            DmarcRecord dmarcRecord = new DmarcRecord("",
                new List<Tag> { new SubDomainPolicy("", PolicyType.Unknown, true) }, domain, "def.com", false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.EqualTo(false));
        }

        [Test]
        public async Task ErrorWhenOnNonOrganisationalDomain()  
        {
            const string domain = "abc.com";
            DmarcRecord dmarcRecord = new DmarcRecord("", new List<Tag> { new SubDomainPolicy("", PolicyType.Unknown) }, domain, "def.com", false, false);

            List<Error> errors = await _rule.Evaluate(dmarcRecord);

            Assert.That(errors.Any, Is.EqualTo(true));
            Assert.That(errors.First().Message, Is.EqualTo($"The specified sub-domain policy (sp) is ineffective because {domain} is not an organisational domain."));
            Assert.That(errors.First().ErrorType, Is.EqualTo(ErrorType.Info));
        }
    }
}
