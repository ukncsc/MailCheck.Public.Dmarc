using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Dmarc.Contracts.External;
using MailCheck.Dmarc.Contracts.History;
using MailCheck.Dmarc.Contracts.Poller;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.EntityHistory.Config;
using MailCheck.Dmarc.EntityHistory.Dao;
using MailCheck.Dmarc.EntityHistory.Entity;
using MailCheck.Dmarc.EntityHistory.Service;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using A = FakeItEasy.A;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;

namespace MailCheck.Dmarc.EntityHistory.Test.Entity
{
    [TestFixture]
    public class DmarcHistoryEntityTest
    {
        private const string Id = "abc.com";

        private IDmarcEntityHistoryDao _dmarcEntityDao;
        private ILogger<DmarcEntityHistory> _log;
        private IDmarcRuaService _dmarcRuaService;
        private DmarcEntityHistory _dmarcHistoryEntity;

        [SetUp]
        public void SetUp()
        {
            _dmarcEntityDao = A.Fake<IDmarcEntityHistoryDao>();
            _log = A.Fake<ILogger<DmarcEntityHistory>>();
            _dmarcRuaService = A.Fake<IDmarcRuaService>();
            _dmarcHistoryEntity = new DmarcEntityHistory(_log, _dmarcEntityDao, _dmarcRuaService);
        }

        [Test]
        public async Task HandleDomainCreatedCreatesDomain()
        {
            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns<DmarcEntityHistoryState>(null);
            await _dmarcHistoryEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));
            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityHistoryState>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void HandleDomainCreatedThrowsIfEntityAlreadyExistsForDomain()
        {
            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns(new DmarcEntityHistoryState(Id));
            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityHistoryState>._)).MustNotHaveHappened();
            A.CallTo(() => _dmarcRuaService.Process(A<string>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleDmarcRecordsPolledAndUpdateWhenNoRecordsExistUpdatesHistoryState()
        {
            var dmarcRecords1 = CreateDmarcRecords().Records.Select(x => x.Record).ToList();

            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns(new DmarcEntityHistoryState(Id));

            DmarcRecordsPolled dmarcRecordsEvaluated =
                new DmarcRecordsPolled(Id, CreateDmarcRecords(), null);

            await _dmarcHistoryEntity.Handle(dmarcRecordsEvaluated);

            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityHistoryState>.That.Matches(_ =>
                _.DmarcHistory.Count == 1 &&
                _.DmarcHistory[0].EndDate == null &&
                _.DmarcHistory[0].DmarcRecords.SequenceEqual(dmarcRecords1)))).MustHaveHappenedOnceExactly();

            A.CallTo(() => _dmarcRuaService.Process(A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleDmarcRecordsPolledAndExistingDmarcRecordsHistoryStateNotUpdated()
        {
            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns(new DmarcEntityHistoryState(Id, new List<DmarcHistoryRecord>
            {
                new DmarcHistoryRecord(DateTime.UtcNow.AddDays(-1), null, CreateDmarcRecords().Records.Select(x => x.Record).ToList())
            }));

            DmarcRecordsPolled dmarcRecordsEvaluated = new DmarcRecordsPolled(Id, CreateDmarcRecords(), null);

            await _dmarcHistoryEntity.Handle(dmarcRecordsEvaluated);

            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityHistoryState>._)).MustNotHaveHappened();
            A.CallTo(() => _dmarcRuaService.Process(A<string>._, A<string>._)).MustNotHaveHappened();

        }

        [Test]
        public async Task HandleDmarcRecordsPolledAndNewDmarcRecordUpdatesHistoryWhichHasOnePreviousRecord()
        {
            var dmarcRecords1 = CreateDmarcRecords().Records.Select(x => x.Record).ToList();
            var dmarcRecords2 = CreateDmarcRecords(record: "v=dmarc2......").Records.Select(x => x.Record).ToList();

            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns(new DmarcEntityHistoryState(Id,
                new List<DmarcHistoryRecord>
                {
                    new DmarcHistoryRecord(DateTime.UtcNow.AddDays(-1), null, dmarcRecords1
                    )
                }));

            DmarcRecordsPolled dmarcRecordsEvaluated =
                new DmarcRecordsPolled(Id, CreateDmarcRecords(record: "v=dmarc2......"), null)
                {
                    Timestamp = DateTime.UtcNow
                };

            await _dmarcHistoryEntity.Handle(dmarcRecordsEvaluated);

            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityHistoryState>.That.Matches(_ =>
                _.DmarcHistory.Count == 2 &&
                _.DmarcHistory[0].EndDate == null &&
                _.DmarcHistory[0].DmarcRecords.SequenceEqual(dmarcRecords2) &&
                _.DmarcHistory[1].EndDate == dmarcRecordsEvaluated.Timestamp &&
                _.DmarcHistory[1].DmarcRecords.SequenceEqual(dmarcRecords1)))).MustHaveHappenedOnceExactly();

            A.CallTo(() => _dmarcRuaService.Process(A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        }


        [Test]
        public async Task HandleDmarcRecordsPolledAndNewDmarcRecordUpdatesHistoryWhichHasTwoPreviousRecord()
        {
            var dmarcRecords1 = CreateDmarcRecords().Records.Select(x => x.Record).ToList();
            var dmarcRecords2 = CreateDmarcRecords(record: "v=dmarc2......").Records.Select(x => x.Record).ToList();
            var dmarcRecords3 = CreateDmarcRecords(record: "v=dmarc3......").Records.Select(x => x.Record).ToList();

            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns(new DmarcEntityHistoryState(Id,
                new List<DmarcHistoryRecord>
                {
                    new DmarcHistoryRecord(DateTime.UtcNow.AddDays(-2), null, dmarcRecords2),
                    new DmarcHistoryRecord(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2), dmarcRecords1)
                }));

            DmarcRecordsPolled dmarcRecordsEvaluated =
                new DmarcRecordsPolled(Id, CreateDmarcRecords(record: "v=dmarc3......"), null)
                {
                    Timestamp = DateTime.UtcNow
                };

            await _dmarcHistoryEntity.Handle(dmarcRecordsEvaluated);

            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityHistoryState>.That.Matches(_ =>
                _.DmarcHistory.Count == 3 &&
                _.DmarcHistory[0].EndDate == null &&
                _.DmarcHistory[0].DmarcRecords.SequenceEqual(dmarcRecords3) &&
                _.DmarcHistory[1].EndDate == dmarcRecordsEvaluated.Timestamp &&
                _.DmarcHistory[1].DmarcRecords.SequenceEqual(dmarcRecords2) &&
                _.DmarcHistory[2].DmarcRecords.SequenceEqual(dmarcRecords1)))).MustHaveHappenedOnceExactly();

            A.CallTo(() => _dmarcRuaService.Process(A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        }


        [Test]
        public async Task HandleDmarcRecordsPolledWhenRecordsInDifferentOrderButSameRecordsNoUpdate()
        {
            var dmarcRecord = CreateDmarcRecords(record: "one,two");

            var dmarcRecord2 = CreateDmarcRecords(record: "two,one");

            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns(new DmarcEntityHistoryState(Id,
                new List<DmarcHistoryRecord>
                {
                    new DmarcHistoryRecord(DateTime.UtcNow.AddDays(-2), null, dmarcRecord.Records.Select(x => x.Record).ToList())
                }));

            DmarcRecordsPolled dmarcRecordsEvaluated =
                new DmarcRecordsPolled(Id, dmarcRecord2, null)
                {
                    Timestamp = DateTime.UtcNow
                };

            await _dmarcHistoryEntity.Handle(dmarcRecordsEvaluated);

            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityHistoryState>._)).MustNotHaveHappened();
            A.CallTo(() => _dmarcRuaService.Process(A<string>._, A<string>._)).MustNotHaveHappened();

        }

        [Test]
        public async Task HandleDmarcRecordsPolledWhenRecordsInSameOrderNoUpdate()
        {
            var dmarcRecord = CreateDmarcRecords(record: "one,two");

            var dmarcRecord2 = CreateDmarcRecords(record: "one,two");


            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns(new DmarcEntityHistoryState(Id,
                new List<DmarcHistoryRecord>
                {
                    new DmarcHistoryRecord(DateTime.UtcNow.AddDays(-2), null, dmarcRecord.Records.Select(x => x.Record).ToList())
                }));

            DmarcRecordsPolled dmarcRecordsEvaluated =
                new DmarcRecordsPolled(Id, dmarcRecord2, null)
                {
                    Timestamp = DateTime.UtcNow
                };

            await _dmarcHistoryEntity.Handle(dmarcRecordsEvaluated);

            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityHistoryState>._)).MustNotHaveHappened();
            A.CallTo(() => _dmarcRuaService.Process(A<string>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleDmarcRecordsPolledWhenNewRecords()
        {
            var dmarcRecord = CreateDmarcRecords(record: "one,two");
            dmarcRecord.Records.AddRange(CreateDmarcRecords().Records);

            var dmarcRecord2 = CreateDmarcRecords(record: "two,three");
            dmarcRecord2.Records.Reverse();

            A.CallTo(() => _dmarcEntityDao.Get(Id)).Returns(new DmarcEntityHistoryState(Id,
                new List<DmarcHistoryRecord>
                {
                    new DmarcHistoryRecord(DateTime.UtcNow.AddDays(-2), null, new List<string> { dmarcRecord.Records[0].Record})
                }));

            DmarcRecordsPolled dmarcRecordsEvaluated =
                new DmarcRecordsPolled(Id, dmarcRecord2, null)
                {
                    Timestamp = DateTime.UtcNow
                };

            await _dmarcHistoryEntity.Handle(dmarcRecordsEvaluated);

            A.CallTo(() => _dmarcEntityDao.Save(A<DmarcEntityHistoryState>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dmarcRuaService.Process(A<string>._, A<string>._)).MustHaveHappenedTwiceExactly();
        }

        private static DmarcRecords CreateDmarcRecords(string domain = Id, string record = "v=dmarc......")
        {
            List<DmarcRecord> records = new List<DmarcRecord>();

            foreach (string dmarcRecord in record.Split(','))
            {
                records.Add(new DmarcRecord(dmarcRecord, new List<Tag> { new Adkim("adkim", AlignmentType.R, false) }, null, domain, "", true, false));
            }


            return new DmarcRecords(domain, records, new List<Message>(), 100);
        }
    }
}
