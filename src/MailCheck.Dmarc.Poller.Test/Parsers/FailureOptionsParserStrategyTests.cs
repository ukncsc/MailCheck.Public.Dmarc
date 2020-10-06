using System.Collections.Generic;
using System.Linq;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Parsers
{
    [TestFixture]
    public class FailureOptionsParserStrategyTests
    {
        private FailureOptionsParserStrategy _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new FailureOptionsParserStrategy();
        }

        [TestCase("0", FailureOptionType.Zero, 0)]
        [TestCase("1", FailureOptionType.One, 0)]
        [TestCase("s", FailureOptionType.S, 0)]
        [TestCase("d", FailureOptionType.D, 0)]
        [TestCase("S", FailureOptionType.S, 0)]
        [TestCase("D", FailureOptionType.D, 0)]
        [TestCase("asdfa", FailureOptionType.Unknown, 1)]
        [TestCase("", FailureOptionType.Unknown, 1)]
        [TestCase(null, FailureOptionType.Unknown, 1)]
        public void Test(string value, FailureOptionType failureOptionType, int errorCount)
        {
            FailureOption tag = (FailureOption)_parser.Parse(string.Empty, value);

            Assert.That(tag.FailureOptionTypes.First(), Is.EqualTo(failureOptionType));
            Assert.That(tag.ErrorCount, Is.EqualTo(errorCount));
        }

        [Test]
        public void TestMultipleValuesWithZeroAndS()
        {
            string value = "0:s";

            List<FailureOptionType> expectedFailureOptionTypes = new List<FailureOptionType>()
                {FailureOptionType.Zero, FailureOptionType.S};

            int errorCount = 0;

            FailureOption tag = (FailureOption) _parser.Parse(string.Empty, value);

            Assert.That(tag.FailureOptionTypes.Count, Is.EqualTo(expectedFailureOptionTypes.Count));

            foreach (FailureOptionType failureOptionType in expectedFailureOptionTypes)
            {
                Assert.IsTrue(tag.FailureOptionTypes.Any(x => x == failureOptionType));
            }
            
            Assert.That(tag.ErrorCount, Is.EqualTo(errorCount));
        }

        [Test]
        public void TestMultipleValuesWithAllValues()
        {
            string value = "0:1:d:s";

            List<FailureOptionType> expectedFailureOptionTypes = new List<FailureOptionType>()
                {FailureOptionType.Zero, FailureOptionType.One, FailureOptionType.D, FailureOptionType.S};

            int errorCount = 0;

            FailureOption tag = (FailureOption)_parser.Parse(string.Empty, value);

            Assert.That(tag.FailureOptionTypes.Count, Is.EqualTo(expectedFailureOptionTypes.Count));

            foreach (FailureOptionType failureOptionType in expectedFailureOptionTypes)
            {
                Assert.IsTrue(tag.FailureOptionTypes.Any(x => x == failureOptionType));
            }

            Assert.That(tag.ErrorCount, Is.EqualTo(errorCount));
        }

        [Test]
        public void TestMultipleValuesWhichIncludesUnknown()
        {
            string value = "0:x";

            List<FailureOptionType> expectedFailureOptionTypes = new List<FailureOptionType>()
                {FailureOptionType.Zero, FailureOptionType.Unknown};

            int errorCount = 1;

            FailureOption tag = (FailureOption)_parser.Parse(string.Empty, value);

            Assert.That(tag.FailureOptionTypes.Count, Is.EqualTo(expectedFailureOptionTypes.Count));

            foreach (FailureOptionType failureOptionType in expectedFailureOptionTypes)
            {
                Assert.IsTrue(tag.FailureOptionTypes.Any(x => x == failureOptionType));
            }

            Assert.That(tag.ErrorCount, Is.EqualTo(errorCount));
        }
    }
}