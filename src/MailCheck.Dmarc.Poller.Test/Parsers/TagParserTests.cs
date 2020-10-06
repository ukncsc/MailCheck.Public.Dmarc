using System;
using System.Collections.Generic;
using FakeItEasy;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Parsers
{
    [TestFixture]
    public class TagParserTests
    {
        private const string TagName = "pct";

        private TagParser _parser;
        private ITagParserStrategy _strategy;

        [SetUp]
        public void SetUp()
        {
            _strategy = A.Fake<ITagParserStrategy>();
            A.CallTo(() => _strategy.Tag).Returns(TagName);
            _parser = new TagParser(new List<ITagParserStrategy> { _strategy });
        }

        [Test]
        public void MatchingStrategyTokensCorrectlyParsed()
        {
            string value = "100";
            string tagValue = $"{TagName}={value}";

            Percent percent = new Percent(tagValue, 100);

            A.CallTo(() => _strategy.Parse(tagValue, value)).Returns(percent);
            A.CallTo(() => _strategy.MaxOccurences).Returns(1);

            List<Tag> tags = _parser.Parse(new List<string> { tagValue });

            Assert.That(tags[0], Is.SameAs(percent));
            Assert.That(tags[0].ErrorCount, Is.Zero);
            A.CallTo(() => _strategy.Parse(tagValue, value)).MustHaveHappenedOnceExactly();;
        }

        [Test]
        public void MatchingStrategyWithExtraTokensErrorAddedForExtraTokensWhenNoExistingError()
        {
            A.CallTo(() => _strategy.Parse("pct=100=100", "100=100")).Returns(new Percent("pct=100=100", null));
            A.CallTo(() => _strategy.MaxOccurences).Returns(1);

            List<Tag> tags = _parser.Parse(new List<string> { "pct=100=100" });

            Assert.That(tags[0].ErrorCount, Is.EqualTo(1));
            Assert.AreEqual("Unexpected values 100=100 in term pct=100=100.", tags[0].Errors[0].Message);
        }

        [Test]
        public void MatchingStrategyWithExtraTokensErrorNotAddedIfExistingError()
        {
            Percent percent = new Percent("pct=100=100", null);
            percent.AddError(new Error(Guid.Empty, ErrorType.Error, "Error parsing tag", string.Empty));
            A.CallTo(() => _strategy.Parse("pct=100=100", "100=100")).Returns(percent);
            A.CallTo(() => _strategy.MaxOccurences).Returns(1);

            List<Tag> tags = _parser.Parse(new List<string> { "pct=100=100" });

            Assert.That(tags[0].ErrorCount, Is.EqualTo(1));
            Assert.AreEqual("Error parsing tag", tags[0].Errors[0].Message);
        }

        [Test]
        public void NoMatchingStrategyUnknownTagCreatedWithError()
        {
            string value = "100";
            string tagValue = $"NotTagName={value}";

            List<Tag> tags = _parser.Parse(new List<string> { tagValue });

            Assert.That(tags[0], Is.TypeOf<UnknownTag>());
            Assert.That(tags[0].ErrorCount, Is.EqualTo(1));
            A.CallTo(() => _strategy.Parse(tagValue, value)).MustNotHaveHappened();
        }

        [Test]
        public void NullValueUnknownTagCreatedWithError()
        {
            List<Tag> tags = _parser.Parse(new List<string> { null });

            Assert.That(tags[0], Is.TypeOf<UnknownTag>());
            Assert.That(tags[0].ErrorCount, Is.EqualTo(1));
            A.CallTo(() => _strategy.Parse(A<string>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void EmptyStringValueUnknownTagCreatedWithError()
        {
            List<Tag> tags = _parser.Parse(new List<string> { string.Empty });

            Assert.That(tags[0], Is.TypeOf<UnknownTag>());
            Assert.That(tags[0].ErrorCount, Is.EqualTo(1));
            A.CallTo(() => _strategy.Parse(A<string>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void MaxOccurenceExceededTagCreatedWithError()
        {
            string value = "100";
            string tagValue = $"{TagName}={value}";

            Percent percent1 = new Percent(tagValue, 100);
            Percent percent2 = new Percent(tagValue, 100);

            A.CallTo(() => _strategy.Parse(tagValue, value)).ReturnsNextFromSequence(percent1, percent2);
            A.CallTo(() => _strategy.MaxOccurences).Returns(1);

            List<Tag> tags = _parser.Parse(new List<string> { tagValue, tagValue });

            Assert.That(tags[0], Is.SameAs(percent1));
            Assert.That(tags[0].ErrorCount, Is.Zero);

            Assert.That(tags[1], Is.SameAs(percent2));
            Assert.That(tags[1].ErrorCount, Is.EqualTo(1));
            Assert.That(tags[1].Errors[0].Message, Is.EqualTo("The pct tag should occur no more than once. This record has 2 occurrences."));
            A.CallTo(() => _strategy.Parse(tagValue, value)).MustHaveHappenedTwiceExactly();
        }
    }
}
