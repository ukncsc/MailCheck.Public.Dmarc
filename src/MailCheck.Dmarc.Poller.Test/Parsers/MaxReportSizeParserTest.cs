using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Parsers
{
    [TestFixture]
    public class MaxReportSizeParserTest
    {
        private MaxReportSizeParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new MaxReportSizeParser();
        }

        [TestCase("100k", 100UL, Unit.K, 0, TestName ="100k is valid max size for reports")]
        [TestCase("1g", 1UL, Unit.G, 0, TestName = "1g is valid max size for reports")]
        [TestCase("10m", 10UL, Unit.M, 0, TestName = "10m is valid max size for reports")]
        [TestCase("1t", 1UL, Unit.T, 0, TestName = "1t is valid max size for reports")]
        [TestCase("10000", 10000UL, Unit.B, 0, TestName = "Bytes is default unit")]
        [TestCase("", null, Unit.Unknown, 1, TestName = "Error if value is empty")]
        [TestCase(null, null, Unit.Unknown, 1, TestName = "Error if value is null")]
        [TestCase("-1t", null, Unit.Unknown, 1, TestName = "Error if size is negative")]
        [TestCase("18446744073709551616", null, Unit.B, 1, TestName = "Error if size larger than ulong MaxValue")]
        public void Test(string value, ulong? size, Unit unit, int errorCount)
        {
            MaxReportSize maxReportSize = _parser.Parse(value);

            Assert.That(maxReportSize.Value, Is.EqualTo(size));
            Assert.That(maxReportSize.Unit, Is.EqualTo(unit));
            Assert.That(maxReportSize.ErrorCount, Is.EqualTo(errorCount));
        }
    }
}