using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Dmarc.Poller.Config;
using MailCheck.Dmarc.Poller.Dns;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Exceptions;
using MailCheck.Dmarc.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test
{
    [TestFixture]
    public class DmarcProcessorTests
    {
        private IDnsClient _dnsClient;
        private IDmarcRecordsParser _dmarcRecordsParser;
        private IDmarcProcessor _dmarcProcessor;
        private IDmarcPollerConfig _config;

        [SetUp]
        public void SetUp()
        {
            _dnsClient = A.Fake<IDnsClient>();
            _dmarcRecordsParser = A.Fake<IDmarcRecordsParser>();
            _config = A.Fake<IDmarcPollerConfig>();
            _dmarcProcessor = new DmarcProcessor(_dnsClient, _dmarcRecordsParser, _config);
        }

        [Test]
        public async Task DmarcExceptionThrownWhenAllowNullResultsNotSetAndEmptyResult()
        {
            string domain = "abc.com";

            A.CallTo(() => _config.AllowNullResults).Returns(false);

            Assert.Throws<DmarcPollerException>(() => _dmarcProcessor.Process(domain).GetAwaiter().GetResult());
        }

        [Test]
        public async Task DmarcExceptionNotThrownWhenAllowNullResultsSetAndEmptyResult()
        {
            string domain = "abc.com";

            A.CallTo(() => _config.AllowNullResults).Returns(true);

            DmarcPollResult result = await _dmarcProcessor.Process(domain);

            Assert.AreEqual(0, result.Records.Records.Count);
        }

        [Test]
        public async Task ErroredWhenRetrievingDmarcRecordTest()
        {
            string domain = "abc.com";

            A.CallTo(() => _config.AllowNullResults).Returns(true);

            A.CallTo(() => _dnsClient.GetDmarcRecords(A<string>._))
                .Returns(new DnsResult<List<DmarcRecordInfo>>("error"));

            DmarcPollResult result = await _dmarcProcessor.Process(domain);
            Assert.That(domain, Is.EqualTo(result.Records.Domain));
        }
    }
}
