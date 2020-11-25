using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Integration.TagParser
{
    public class RufTests : BasePollHandlerTests
    {
        [TestCase("mailto:user1@test.gov.uk")]
        [TestCase(" mailto:user1@test.gov.uk")]
        [TestCase("mailto:user1@test.gov.uk ")]
        public async Task RufWithSingleValidValue(string validValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;ruf={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportUriForensic rufTag = GetTags<ReportUriForensic>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.AreEqual(1, rufTag.Uris.Count);
            Assert.True(rufTag.Uris[0].Valid);
            Assert.AreEqual("mailto:user1@test.gov.uk", rufTag.Uris[0].Value);
            Assert.True(rufTag.Valid);
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task RufWithValidEmptyValue(string validValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;ruf={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportUriForensic rufTag = GetTags<ReportUriForensic>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.AreEqual(0, rufTag.Uris.Count);
            Assert.True(rufTag.Valid);
        }

        [TestCase("mailto:user1@test.gov.uk, mailto:user2@test.gov.uk, mailto:user3@test.gov.uk")]
        [TestCase("mailto:user1@test.gov.uk,mailto:user2@test.gov.uk,mailto:user3@test.gov.uk")]
        public async Task RufWithMultipleValidValues(string validValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;ruf={validValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportUriForensic rufTag = GetTags<ReportUriForensic>().First();

            Assert.IsEmpty(GetAdvisories());
            Assert.AreEqual(3, rufTag.Uris.Count);
            Assert.True(rufTag.Uris[0].Valid);
            Assert.True(rufTag.Uris[1].Valid);
            Assert.True(rufTag.Uris[2].Valid);
            Assert.AreEqual("mailto:user1@test.gov.uk", rufTag.Uris[0].Value);
            Assert.AreEqual("mailto:user2@test.gov.uk", rufTag.Uris[1].Value);
            Assert.AreEqual("mailto:user3@test.gov.uk", rufTag.Uris[2].Value);
            Assert.True(rufTag.Valid);
        }

        [TestCase("user1@test.gov.uk")]
        [TestCase("user1@test.gov.uk=user1@test.gov.uk")]
        [TestCase("18")]
        [TestCase("abc")]
        public async Task RufWithInvalidValue(string invalidValue)
        {
            SetUpTxtRecords($"v=DMARC1;p=none;ruf={invalidValue}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportUriForensic rufTag = GetTags<ReportUriForensic>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(2, advisories.Count);

            Assert.AreEqual($"Invalid uri value: {invalidValue}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);

            Assert.AreEqual("Only URIs with mailto: scheme are guaranteed to have forensic reports delivered. This record has non mailto: scheme URIs in its forensic report URI tag (ruf).", advisories[1].Text);
            Assert.AreEqual($"This DMARC record is using a different method of delivering forensic reports than email (mailto:){Environment.NewLine}The email method is the only one that is well supported so we don't recommend using others.{Environment.NewLine}{Environment.NewLine}It's most likely that this DMARC record has a spelling mistake mailto or is otherwise misconfigured, please investigate.", advisories[1].MarkDown);
            Assert.AreEqual(MessageType.warning, advisories[1].MessageType);

            Assert.AreEqual($"ruf={invalidValue};", rufTag.Value);
            Assert.False(rufTag.Valid);
        }

        [TestCase("user1@test.gov.uk", "user2@test.gov.uk")]
        [TestCase("abc", "123")]
        public async Task RufWithMultipleInvalidValues(params string[] invalidValue)
        {
            string ruf = string.Join(",", invalidValue);
            SetUpTxtRecords($"v=DMARC1;p=none;ruf={ruf}");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            ReportUriForensic rufTag = GetTags<ReportUriForensic>().First();
            List<Message> advisories = GetAdvisories();

            Assert.AreEqual(3, advisories.Count);

            Assert.AreEqual($"Invalid uri value: {invalidValue[0]}.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);

            Assert.AreEqual($"Invalid uri value: {invalidValue[1]}.", advisories[1].Text);
            Assert.AreEqual(string.Empty, advisories[1].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[1].MessageType);

            Assert.AreEqual("Only URIs with mailto: scheme are guaranteed to have forensic reports delivered. This record has non mailto: scheme URIs in its forensic report URI tag (ruf).", advisories[2].Text);
            Assert.AreEqual($"This DMARC record is using a different method of delivering forensic reports than email (mailto:){Environment.NewLine}The email method is the only one that is well supported so we don't recommend using others.{Environment.NewLine}{Environment.NewLine}It's most likely that this DMARC record has a spelling mistake mailto or is otherwise misconfigured, please investigate.", advisories[2].MarkDown);
            Assert.AreEqual(MessageType.warning, advisories[2].MessageType);

            Assert.AreEqual($"ruf={ruf};", rufTag.Value);
            Assert.False(rufTag.Valid);
        }

        [Test]
        public async Task RufShouldOnlyAppearOnce()
        {
            SetUpTxtRecords("v=DMARC1;p=none;ruf=mailto:user1@test.gov.uk;ruf=mailto:user1@test.gov.uk");

            await Handler.Handle(new DmarcPollPending("test.gov.uk"));

            List<Message> advisories = GetAdvisories();
            Assert.AreEqual(1, advisories.Count);
            Assert.AreEqual("The ruf tag should occur no more than once. This record has 2 occurrences.", advisories[0].Text);
            Assert.AreEqual(string.Empty, advisories[0].MarkDown);
            Assert.AreEqual(MessageType.error, advisories[0].MessageType);
        }
    }
}