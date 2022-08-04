using System;
using System.Collections.Generic;
using MailCheck.Dmarc.Contracts.Poller;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Mapping;
using NUnit.Framework;
using DmarcRecord = MailCheck.Dmarc.Poller.Domain.DmarcRecord;
using DmarcRecords = MailCheck.Dmarc.Poller.Domain.DmarcRecords;
using OptionalDefaultTag = MailCheck.Dmarc.Poller.Domain.OptionalDefaultTag;
using Tag = MailCheck.Dmarc.Poller.Domain.Tag;

namespace MailCheck.Dmarc.Poller.Test.Mapping
{
    [TestFixture]
    public class MappingTests
    {
        [TestCase(ErrorType.Error, MessageType.error)]
        [TestCase(ErrorType.Info, MessageType.info)]
        [TestCase(ErrorType.Warning, MessageType.warning)]
        public void TagErrorsAreMapped(ErrorType sourceErrorType, MessageType expectedMessageType)
        {
            OptionalDefaultTag erroneousTag = new OptionalDefaultTag(string.Empty, false);
            erroneousTag.AddError(new Error(Guid.Empty, "mailcheck.dmarc.testName", sourceErrorType, "testMessage", "testMarkdown"));

            DmarcRecord dmarcRecord = new DmarcRecord(string.Empty, new List<Tag> { erroneousTag }, string.Empty, string.Empty, false, false);

            DmarcPollResult dmarcPollResult = new DmarcPollResult(new DmarcRecords(string.Empty, new List<DmarcRecord> { dmarcRecord }, 0), TimeSpan.MinValue);

            DmarcRecordsPolled result = dmarcPollResult.ToDmarcRecordsPolled();

            Assert.AreEqual(1, result.Records.Records[0].Messages.Count);
            Assert.AreEqual(expectedMessageType, result.Records.Records[0].Messages[0].MessageType);
            Assert.AreEqual("testMarkdown", result.Records.Records[0].Messages[0].MarkDown);
            Assert.AreEqual("testMessage", result.Records.Records[0].Messages[0].Text);
        }
    }
}
