using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Scheduler;
using MailCheck.Dmarc.Scheduler.Config;
using MailCheck.Dmarc.Scheduler.Dao;
using MailCheck.Dmarc.Scheduler.Dao.Model;
using MailCheck.Dmarc.Scheduler.Processor;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Dmarc.Scheduler.Test.Processor
{
    [TestFixture]
    public class DmarcPollSchedulerProcessorTests
    {
        private DmarcPollSchedulerProcessor _sut;
        private IDmarcPeriodicSchedulerDao _dao;
        private IMessagePublisher _publisher;
        private IDmarcPeriodicSchedulerConfig _config;
        private ILogger<DmarcPollSchedulerProcessor> _log;

        [SetUp]
        public void SetUp()
        {
            _dao = A.Fake<IDmarcPeriodicSchedulerDao>();
            _publisher = A.Fake<IMessagePublisher>();
            _config = A.Fake<IDmarcPeriodicSchedulerConfig>();
            _log = A.Fake<ILogger<DmarcPollSchedulerProcessor>>();

            _sut = new DmarcPollSchedulerProcessor(_dao, _publisher, _config, _log);
        }

        [Test]
        public async Task ItShouldPublishAndUpdateThenContinueWhenThereAreExpiredRecords()
        {
            A.CallTo(() => _dao.GetExpiredDmarcRecords())
                .Returns(CreateSchedulerStates("ncsc.gov.uk", "fco.gov.uk"));

            ProcessResult result = await _sut.Process();

            A.CallTo(() => _publisher.Publish(A<DmarcRecordExpired>._, A<string>._))
                .MustHaveHappenedTwiceExactly();

            A.CallTo(() => _dao.UpdateLastChecked(A<List<DmarcSchedulerState>>._))
                .MustHaveHappenedOnceExactly();

            Assert.AreEqual(ProcessResult.Continue, result);
        }

        [Test]
        public async Task ItShouldNotPublishOrUpdateThenStopWhenThereAreNoExpiredRecords()
        {
            A.CallTo(() => _dao.GetExpiredDmarcRecords())
                .Returns(CreateSchedulerStates());

            ProcessResult result = await _sut.Process();

            A.CallTo(() => _publisher.Publish(A<DmarcRecordExpired>._, A<string>._))
                .MustNotHaveHappened();

            A.CallTo(() => _dao.UpdateLastChecked(A<List<DmarcSchedulerState>>._))
                .MustNotHaveHappened();

            Assert.AreEqual(ProcessResult.Stop, result);
        }


        private List<DmarcSchedulerState> CreateSchedulerStates(params string[] args) =>
            args.Select(_ => new DmarcSchedulerState(_)).ToList();
    }
}
