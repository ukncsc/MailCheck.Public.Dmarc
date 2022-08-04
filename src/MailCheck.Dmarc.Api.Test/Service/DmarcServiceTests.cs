using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Api.Config;
using MailCheck.Dmarc.Api.Dao;
using MailCheck.Dmarc.Api.Domain;
using MailCheck.Dmarc.Api.Service;
using MailCheck.Dmarc.Contracts.Entity;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Dmarc.Api.Test.Service
{
    [TestFixture]
    public class DmarcServiceTests
    {
        private DmarcService _dmarcService;
        private IMessagePublisher _messagePublisher;
        private IDmarcApiDao _dao;
        private IDmarcApiConfig _config;
        private IPolicyResolver _policyResolver;
        private ILogger<DmarcService> _log;

        [SetUp]
        public void SetUp()
        {
            _messagePublisher = A.Fake<IMessagePublisher>();
            _dao = A.Fake<IDmarcApiDao>();
            _config = A.Fake<IDmarcApiConfig>();
            _policyResolver = A.Fake<IPolicyResolver>();
            _log = A.Fake<ILogger<DmarcService>>();
            _dmarcService = new DmarcService(_messagePublisher, _dao, _config, _policyResolver, _log);
        }

        [Test]
        public async Task PublishesDomainMissingMessageWhenDomainDoesNotExist()
        {
            A.CallTo(() => _dao.GetDmarcForDomain("testDomain"))
                .Returns(Task.FromResult<DmarcInfoResponse>(null));

            DmarcInfoResponse result = await _dmarcService.GetDmarcForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustHaveHappenedOnceExactly();
            Assert.AreEqual(null, result);
        }

        [Test]
        public async Task DoesNotPublishDomainMissingMessageWhenDomainExists()
        {
            DmarcInfoResponse dmarcInfoResponse = new DmarcInfoResponse("", DmarcState.Created);
            A.CallTo(() => _dao.GetDmarcForDomain("testDomain"))
                .Returns(Task.FromResult(dmarcInfoResponse));

            DmarcInfoResponse result = await _dmarcService.GetDmarcForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustNotHaveHappened();
            Assert.AreSame(dmarcInfoResponse, result);

        }
    }
}