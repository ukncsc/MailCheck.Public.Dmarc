using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Parsers
{
    [TestFixture]
    public class VersionParserStrategyTests
    {
        private VersionParserStrategy _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new VersionParserStrategy();
        }

        [TestCase("v=DMARC1", 0)]
        [TestCase("v=dmarc1", 0)]
        [TestCase("v=DMARC2", 1)]
        [TestCase("", 1)]
        [TestCase(null, 1)]
        public void Test(string value, int errorCount)
        {
            Version version = (Version)_parser.Parse(string.Empty, value);
            Assert.That(version.ErrorCount, Is.EqualTo(errorCount));
        }
    }
}
