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
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test
{
    [TestFixture]
    public class DmarcProcessorTests
    {
        private const string ExampleDmarcRecord = "v=DMARC1;p=reject;adkim=s;aspf=s;rua=mailto:dmarc-rua@dmarc.service.gov.uk;";
        private IDnsClient _dnsClient;
        private IDmarcRecordsParser _dmarcRecordsParser;
        private IDmarcProcessor _dmarcProcessor;
        private IDmarcPollerConfig _config;
        private ILogger<DmarcProcessor> _log;

        [SetUp]
        public void SetUp()
        {
            _dnsClient = A.Fake<IDnsClient>();
            _dmarcRecordsParser = A.Fake<IDmarcRecordsParser>();
            _config = A.Fake<IDmarcPollerConfig>();
            _log = A.Fake<ILogger<DmarcProcessor>>();
            _dmarcProcessor = new DmarcProcessor(_dnsClient, _dmarcRecordsParser, _config, _log);
        }

        [Test]
        public async Task DmarcRecordsReturnedWhenAllGood()
        {
            string domain = "abc.com";

            A.CallTo(() => _config.AllowNullResults).Returns(true);

            DmarcRecordInfo dmarcRecordInfo = new DmarcRecordInfo(new List<string> { ExampleDmarcRecord }, domain, true, false);
            List<DmarcRecordInfo> dmarcRecordInfos = new List<DmarcRecordInfo> { dmarcRecordInfo };
            A.CallTo(() => _dnsClient.GetDmarcRecords(domain))
                .Returns(new DnsResult<List<DmarcRecordInfo>>(dmarcRecordInfos, 1));

            DmarcRecords dmarcRecords = new DmarcRecords(domain, new List<DmarcRecord>(), 1);
            A.CallTo(() => _dmarcRecordsParser.Parse(domain, dmarcRecordInfos, 1))
                .Returns(dmarcRecords);

            DmarcPollResult result = await _dmarcProcessor.Process(domain);

            Assert.That(result.Records, Is.SameAs(dmarcRecords));
        }

        [Test]
        public async Task DmarcExceptionThrownWhenAllowNullResultsNotSetAndEmptyResult()
        {
            string domain = "abc.com";

            A.CallTo(() => _config.AllowNullResults).Returns(false);

            Assert.ThrowsAsync<DmarcPollerException>(() => _dmarcProcessor.Process(domain));
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
        public async Task ErrorResultReturnedWhenDnsErrorRetrievingDmarcRecordTest()
        {
            string domain = "abc.com";

            A.CallTo(() => _config.AllowNullResults).Returns(true);

            A.CallTo(() => _dnsClient.GetDmarcRecords(A<string>._))
                .Returns(new DnsResult<List<DmarcRecordInfo>>("error"));

            DmarcPollResult result = await _dmarcProcessor.Process(domain);
            Assert.That(domain, Is.EqualTo(result.Records.Domain));
        }

        [Test]
        public async Task ExceptionThrownWhenDnsErrorRetrievingDmarcRecordAndNotAllowingNullsTest()
        {
            string domain = "abc.com";

            A.CallTo(() => _config.AllowNullResults).Returns(false);

            A.CallTo(() => _dnsClient.GetDmarcRecords(A<string>._))
                .Returns(new DnsResult<List<DmarcRecordInfo>>("error"));

            Assert.ThrowsAsync<DmarcPollerException>(() => _dmarcProcessor.Process(domain));
        }
    }
}
