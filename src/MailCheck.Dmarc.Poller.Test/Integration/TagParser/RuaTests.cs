using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class RuaTests : BasePollHandlerTests
    {
        [TestCase("mailto:user1@test.gov.uk")]
        [TestCase(" mailto:user1@test.gov.uk")]
        [TestCase("mailto:user1@test.gov.uk ")]
        public async Task RuaWithSingleValidValue(string validValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;rua={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportUriAggregate ruaTag = GetTags<ReportUriAggregate>().First();
            Assert.IsEmpty(GetAdvisories());
            
            Assert.AreEqual(1, ruaTag.Uris.Count);
            Assert.True(ruaTag.Uris[0].Valid);
            Assert.AreEqual("mailto:user1@test.gov.uk", ruaTag.Uris[0].Value);
            Assert.True(ruaTag.Valid);
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task RuaWithValidEmptyValue(string validValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;rua={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportUriAggregate ruaTag = GetTags<ReportUriAggregate>().First();
            Assert.IsEmpty(GetAdvisories());

            Assert.AreEqual(0, ruaTag.Uris.Count);
            Assert.True(ruaTag.Valid);
        }

        [TestCase("mailto:user1@test.gov.uk, mailto:user2@test.gov.uk, mailto:user3@test.gov.uk")]
        [TestCase("mailto:user1@test.gov.uk,mailto:user2@test.gov.uk,mailto:user3@test.gov.uk")]
        public async Task RuaWithMultipleValidValues(string validValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;rua={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportUriAggregate ruaTag = GetTags<ReportUriAggregate>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.AreEqual(3, ruaTag.Uris.Count);
            Assert.True(ruaTag.Uris[0].Valid);
            Assert.True(ruaTag.Uris[1].Valid);
            Assert.True(ruaTag.Uris[2].Valid);
            Assert.AreEqual("mailto:user1@test.gov.uk", ruaTag.Uris[0].Value);
            Assert.AreEqual("mailto:user2@test.gov.uk", ruaTag.Uris[1].Value);
            Assert.AreEqual("mailto:user3@test.gov.uk", ruaTag.Uris[2].Value);
            Assert.True(ruaTag.Valid);
        }

        [TestCase("user1@test.gov.uk")]
        [TestCase("user1@test.gov.uk=user1@test.gov.uk")]
        [TestCase("18")]
        [TestCase("abc")]
        public async Task RuaWithInvalidValue(string invalidValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;rua={invalidValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportUriAggregate ruaTag = GetTags<ReportUriAggregate>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(2, advisories.Count);

            Assert.AreEqual($"Invalid uri value: {invalidValue}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);

            Assert.AreEqual("Only URIs with mailto: scheme are guaranteed to have aggregate reports delivered. This record has non mailto: scheme URIs in its aggregate report URI tag (rua).", advisories[1].Text);
            Assert.AreEqual($"This DMARC record is using an unsupported method of delivering aggregate reports than mailto.{Environment.NewLine}{Environment.NewLine}It's most likely that mailto has a spelling mistake, please investigate.", advisories[1].MarkDown);
            Assert.AreEqual(MessageType.warning, advisories[1].MessageType);

            Assert.AreEqual($"rua={invalidValue};", ruaTag.Value);
            Assert.False(ruaTag.Valid);
        }

        [TestCase("user1@test.gov.uk", "user2@test.gov.uk")]
        [TestCase("abc", "123")]
        public async Task RuaWithMultipleInvalidValues(params string[] invalidValue)
        {
            string rua = string.Join(",", invalidValue);
            SetUpTxtRecords($"v=DMARC1;p=none;rua={rua}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportUriAggregate ruaTag = GetTags<ReportUriAggregate>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(3, advisories.Count);
            Assert.AreEqual($"Invalid uri value: {invalidValue[0]}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);

            Assert.AreEqual($"Invalid uri value: {invalidValue[1]}.", advisories[1].Text);
            Assert.AreEqual(string.Empty, advisories[1].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[1].MessageType);

            Assert.AreEqual("Only URIs with mailto: scheme are guaranteed to have aggregate reports delivered. This record has non mailto: scheme URIs in its aggregate report URI tag (rua).", advisories[2].Text);
            Assert.AreEqual($"This DMARC record is using an unsupported method of delivering aggregate reports than mailto.{Environment.NewLine}{Environment.NewLine}It's most likely that mailto has a spelling mistake, please investigate.", advisories[2].MarkDown);
            Assert.AreEqual(MessageType.warning, advisories[2].MessageType);

            Assert.AreEqual($"rua={rua};", ruaTag.Value);
            Assert.False(ruaTag.Valid);
        }

        [Test]
        public async Task RuaShouldOnlyAppearOnce()
        {
            SetUpTxtRecords("v=DMARC1;p=none;rua=mailto:user1@test.gov.uk;rua=mailto:user1@test.gov.uk");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            List<Message> advisories = GetAdvisories();
            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual("The rua tag should occur no more than once. This record has 2 occurrences.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
        }
    }
}