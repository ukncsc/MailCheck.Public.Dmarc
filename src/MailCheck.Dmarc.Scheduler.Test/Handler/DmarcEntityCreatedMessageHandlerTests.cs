using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.Scheduler;
using MailCheck.Dmarc.Scheduler.Config;
using MailCheck.Dmarc.Scheduler.Dao;
using MailCheck.Dmarc.Scheduler.Dao.Model;
using MailCheck.Dmarc.Scheduler.Handler;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Dmarc.Scheduler.Test.Handler
{
    [TestFixture]
    public class DmarcEntityCreatedMessageHandlerTests
    {
        private DmarcSchedulerHandler _sut;
        private IDmarcSchedulerDao _dao;
        private IMessageDispatcher _dispatcher;
        private IDmarcSchedulerConfig _config;
        private ILogger<DmarcSchedulerHandler> _log;

        [SetUp]
        public void SetUp()
        {
            _dao = A.Fake<IDmarcSchedulerDao>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _config = A.Fake<IDmarcSchedulerConfig>();
            _log = A.Fake<ILogger<DmarcSchedulerHandler>>();

            _sut = new DmarcSchedulerHandler(_dao, _dispatcher, _config, _log);
        }

        [Test]
        public async Task ItShouldSaveAndDispatchTheDmarcStateIfItDoesntExist()
        {
            A.CallTo(() => _dao.Get(A<string>._)).Returns<DmarcSchedulerState>(null);

            await _sut.Handle(new DmarcEntityCreated("ncsc.gov.uk", 1));

            A.CallTo(() => _dao.Save(A<DmarcSchedulerState>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _dispatcher.Dispatch(A<DmarcRecordExpired>._, A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task ItShouldNotSaveOrDispatchTheDmarcStateIfItExists()
        {
            A.CallTo(() => _dao.Get(A<string>._)).Returns(new DmarcSchedulerState("ncsc.gov.uk"));

            await _sut.Handle(new DmarcEntityCreated("ncsc.gov.uk", 1));

            A.CallTo(() => _dao.Save(A<DmarcSchedulerState>._)).MustNotHaveHappened();

            A.CallTo(() => _dispatcher.Dispatch(A<DmarcRecordExpired>._, A<string>._))
                .MustNotHaveHappened();
        }
    }
}
