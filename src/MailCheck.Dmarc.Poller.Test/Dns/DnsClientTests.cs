using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using FakeItEasy;
using MailCheck.Dmarc.Poller.Dns;
using MailCheck.Dmarc.Poller.OrgDomain;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Dmarc.Poller.Test.Dns
{
    [TestFixture]
    public class DnsClientTests
    {
        private Poller.Dns.DnsClient _dnsClient;
        private ILookupClient _lookupClient;

        [SetUp]
        public void SetUp()
        {
            _lookupClient = A.Fake<ILookupClient>();
            _dnsClient = new Poller.Dns.DnsClient(_lookupClient, A.Fake<IOrganisationalDomainProvider>(),
                A.Fake<ILogger<Poller.Dns.DnsClient>>());
        }

        [Test]
        public async Task DoenstReturnErrorForNxDomainResponse()
        {
            const string error = "Non-Existent Domain";

            IDnsQueryResponse response = CreateError(error);

            A.CallTo(() => _lookupClient.QueryAsync(A<string>._, QueryType.TXT, QueryClass.IN, CancellationToken.None)).Returns(response);

            DnsResult<List<DmarcRecordInfo>> recordInfos = await _dnsClient.GetDmarcRecords(string.Empty);

            Assert.That(recordInfos.Value.Count, Is.EqualTo(0));
            Assert.That(recordInfos.Error, Is.Null);
        }

        [Test]
        public async Task DoesntReturnErrorForServerFailureResponse()
        {
            const string error = "Server Failure";

            IDnsQueryResponse response = CreateError(error);

            A.CallTo(() => _lookupClient.QueryAsync(A<string>._, QueryType.TXT, QueryClass.IN, CancellationToken.None)).Returns(response);

            DnsResult<List<DmarcRecordInfo>> recordInfos = await _dnsClient.GetDmarcRecords(string.Empty);

            Assert.That(recordInfos.Value.Count, Is.EqualTo(0));
            Assert.That(recordInfos.Error, Is.Null);
        }
        
        private IDnsQueryResponse CreateError(string error)
        {
            IDnsQueryResponse response = A.Fake<IDnsQueryResponse>();
            A.CallTo(() => response.HasError).Returns(true);
            A.CallTo(() => response.ErrorMessage).Returns(error);

            A.CallTo(() => response.Answers).Returns(new ReadOnlyCollection<DnsResourceRecord>(new List<DnsResourceRecord>()));
            return response;
        }
    }
}
