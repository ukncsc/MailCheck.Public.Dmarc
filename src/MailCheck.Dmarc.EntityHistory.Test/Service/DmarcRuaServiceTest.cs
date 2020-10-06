using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.EntityHistory.Config;
using MailCheck.Dmarc.EntityHistory.Service;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using A = FakeItEasy.A;

namespace MailCheck.Dmarc.EntityHistory.Test.Service
{
    [TestFixture]
    public class DmarcRuaServiceTest
    {
        private const string Id = "abc.com";

        private IDmarcRuaValidator _ruaValidator;
        private ILogger<DmarcRuaService> _log;
        private DmarcRuaService _dmarcRuaService;
        private IDmarcEntityHistoryConfig _config;
        private IMessageDispatcher _dispatcher;

        [SetUp]
        public void SetUp()
        {
            _log = A.Fake<ILogger<DmarcRuaService>>();
            _config = A.Fake<IDmarcEntityHistoryConfig>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _ruaValidator = new DmarcRuaValidator();
            _dmarcRuaService = new DmarcRuaService(_config, _dispatcher, _ruaValidator, _log);
        }

        [Test]
        public void NothingPublishedWithInvalidRua()
        {
            string record = "v=DMARC;rua=mailto:test@hello.com";
            _dmarcRuaService.Process(Id, record);
            A.CallTo(() => _dispatcher.Dispatch(A<RuaVerificationChangeFound>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void PublishedRuaVerificationChangeFoundWithOneValidRua()
        {
            string record = "v=DMARC;rua=mailto:test1234567@dmarc-rua.mailcheck.service.ncsc.gov.uk";
            _dmarcRuaService.Process(Id, record);
            A.CallTo(() => _dispatcher.Dispatch(A<RuaVerificationChangeFound>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void PublishedRuaVerificationChangeFoundWithTwoValidRua()
        {
            string record = "v=DMARC;rua=mailto:test1234567@dmarc-rua.mailcheck.service.ncsc.gov.uk,mailto:test1234568@dmarc-rua.mailcheck.service.ncsc.gov.uk";
            _dmarcRuaService.Process(Id, record);
            A.CallTo(() => _dispatcher.Dispatch(A<RuaVerificationChangeFound>._, A<string>._)).MustHaveHappenedTwiceExactly();
        }
    }
}