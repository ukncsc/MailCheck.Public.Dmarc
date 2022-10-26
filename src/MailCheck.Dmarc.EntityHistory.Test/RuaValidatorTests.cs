using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MailCheck.Dmarc.EntityHistory.Test
{
    [TestFixture]
    public class RuaValidatorTests
    {
        private IDmarcRuaValidator _ruaValidator = new DmarcRuaValidator();


        [Test]
        public void DmarcRecordWithOneValidRuaTag()
        {
            string ruaEmail = "test1234567@dmarc-rua.mailcheck.service.ncsc.gov.uk";
            string dmarcRecord = $"v=DMARC1;rua=mailto:{ruaEmail};";
            RuaResult result = _ruaValidator.Validate(dmarcRecord);
            Assert.That(result.Valid, Is.True);
            Assert.That(result.Tokens.Count, Is.EqualTo(1));
            Assert.That(result.Tokens[0], Is.EqualTo("test1234567"));
        }


        [Test]
        public void DmarcRecordWithTwoValidRuaTag()
        {
            string ruaEmail1 = "test1234567@dmarc-rua.mailcheck.service.ncsc.gov.uk";
            string ruaEmail2 = "test1234568@dmarc-rua.mailcheck.service.ncsc.gov.uk";
            string dmarcRecord = $"v=DMARC1;rua=mailto:{ruaEmail1},mailto:{ruaEmail2};";
            RuaResult result = _ruaValidator.Validate(dmarcRecord);
            Assert.That(result.Valid, Is.True);
            Assert.That(result.Tokens.Count, Is.EqualTo(2));
            Assert.That(result.Tokens[0], Is.EqualTo("test1234567"));
            Assert.That(result.Tokens[1], Is.EqualTo("test1234568"));
        }

        [Test]
        public void DmarcRecordWithInvalidRuaTag()
        {
            string ruaEmail = "dmarc-rua@dmarc.service.gov.uk";
            string dmarcRecord = $"v=DMARC1;rua=mailto:{ruaEmail},mailto:{ruaEmail};";
            RuaResult result = _ruaValidator.Validate(dmarcRecord);
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Tokens.Count, Is.EqualTo(0));
        }

        [Test]
        public void DmarcRecordWithInvalidRuaTagLessThan11CharactersToken()
        {
            string ruaEmail = "test12345@dmarc-rua.mailcheck.service.ncsc.gov.uk";
            string dmarcRecord = $"v=DMARC1;rua=mailto:{ruaEmail};";
            RuaResult result = _ruaValidator.Validate(dmarcRecord);
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Tokens.Count, Is.EqualTo(0));
        }

        [Test]
        public void DmarcRecordWitInvalidRuaTagMoreThan11CharactersToken()
        {
            const string ruaEmail = "test12345678@dmarc-rua.mailcheck.service.ncsc.gov.uk";
            string dmarcRecord = $"v=DMARC1;rua=mailto:{ruaEmail};";
            RuaResult result = _ruaValidator.Validate(dmarcRecord);
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Tokens.Count, Is.EqualTo(0));
        }

        [TestCase("rua=mailto:D25DeT6aGUr@dmarc-rua.mailcheck.service.ncsc.gov.uk, " +
                  "mailto:abc12345678@dmarc-rua.mailcheck.service.ncsc.gov.uk, " +
                  "mailto:26a79797d2f5097@rep.dmarcanalyzer.com;")]
        [TestCase("rua=mailto:D25DeT6aGUr@dmarc-rua.mailcheck.service.ncsc.gov.uk,\t \t" +
                  "mailto:abc12345678@dmarc-rua.mailcheck.service.ncsc.gov.uk,          ")]
        [TestCase("rua=mailto:D25DeT6aGUr@dmarc-rua.mailcheck.service.ncsc.gov.uk    ,   \t  " +
                  "mailto:abc12345678@dmarc-rua.mailcheck.service.ncsc.gov.uk   \t,       \t   ")]
        [TestCase("rua =   mailto:D25DeT6aGUr@dmarc-rua.mailcheck.service.ncsc.gov.uk,  "+
                  "mailto:abc12345678@dmarc-rua.mailcheck.service.ncsc.gov.uk;  ")]
        public void DmarcRecordWithWhiteSpaceInRuaTag(string dmarcRecord)
        {
            RuaResult result = _ruaValidator.Validate(dmarcRecord);
            Assert.That(result.Valid, Is.True);
            Assert.That(result.Tokens.Count, Is.EqualTo(2));
        }
    }
}