using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Parsers
{
    [TestFixture]
    public class ReportIntervalParserStrategyTest
    {
        private ReportIntervalParserStrategy _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new ReportIntervalParserStrategy();
        }

        [TestCase("86400", 86400, TestName = "86400 valid report interval.")]
        [TestCase("3600", 3600, TestName = "3600 valid report interval.")]
        public void ParseValidReportIntervals(string value, int reportInterval)
        {
            ReportInterval tag = (ReportInterval)_parser.Parse(string.Empty, value);
            
            Assert.AreEqual(reportInterval, tag.Interval);
        }

        [TestCase("-86400", TestName = "negative report interval invalid.")]
        [TestCase("asdf", TestName = "random string report interval invalid.")]
        [TestCase("", TestName = "emtpy string report interval invalid.")]
        [TestCase(null, TestName = "null report interval invalid.")]
        public void ParseInvalidReportIntervals(string value)
        {
            ReportInterval tag = (ReportInterval)_parser.Parse(string.Empty, value);

            Assert.Null(tag.Interval);
            Assert.AreEqual(1, tag.ErrorCount);
            Assert.AreEqual($"Invalid ri value: {value}.", tag.AllErrors[0].Message);
            Assert.AreEqual(string.Empty, tag.AllErrors[0].Markdown);
        }
    }
}