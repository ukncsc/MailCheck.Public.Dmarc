using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.TestSupport;
using MailCheck.Common.Util;
using MailCheck.Dmarc.Migration;
using MailCheck.Dmarc.Scheduler.Config;
using MailCheck.Dmarc.Scheduler.Dao;
using MailCheck.Dmarc.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dmarc.Scheduler.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class DmarcPeriodicSchedulerDaoTests : DatabaseTestBase
    {
        private DmarcPeriodicSchedulerDao _dao;
        private DateTime now = DateTime.Now;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            TruncateDatabase().Wait();

            IDatabase database = A.Fake<IDatabase>();

            IDmarcPeriodicSchedulerConfig config = A.Fake<IDmarcPeriodicSchedulerConfig>();
            A.CallTo(() => config.RefreshIntervalSeconds).Returns(0);
            A.CallTo(() => config.DomainBatchSize).Returns(10);

            IClock clock = A.Fake<IClock>();
            A.CallTo(() => clock.GetDateTimeUtc()).Returns(now);

            _dao = new DmarcPeriodicSchedulerDao(database, config, clock);
        }

        [Test]
        public async Task ItShouldReturnNothingIfThereAreNoExpiredRecords()
        {
            List<DmarcSchedulerState> states = await _dao.GetExpiredDmarcRecords();
            Assert.That(states.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ItShouldReturnAllExpiredRecords()
        {
            await Insert("ncsc.gov.uk", DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)));

            List<DmarcSchedulerState> state = await _dao.GetExpiredDmarcRecords();

            Assert.That(state[0].Id, Is.EqualTo("ncsc.gov.uk"));
        }

        [Test]
        public async Task ItShouldUpdateTheLastCheckedTime()
        {
            await Insert("ncsc.gov.uk", now.AddSeconds(-3601));

            DateTime current = await GetLastChecked("ncsc.gov.uk");

            await _dao.UpdateLastChecked(new List<DmarcSchedulerState> { new DmarcSchedulerState("ncsc.gov.uk") });

            DateTime updated = await GetLastChecked("ncsc.gov.uk");

            Assert.That(updated, Is.GreaterThan(current));
        }

        [Test]
        public async Task ItShouldUpdateTheLastCheckedTimeRandomly()
        {
            await Insert("ncsc.gov.uk", now.AddSeconds(-3601));

            DateTime current = await GetLastChecked("ncsc.gov.uk");

            await _dao.UpdateLastChecked(new List<DmarcSchedulerState> { new DmarcSchedulerState("ncsc.gov.uk") });

            DateTime updated = await GetLastChecked("ncsc.gov.uk");

            DateTime lowerLimit = now.AddSeconds(-3600);

            Assert.That(updated, Is.GreaterThanOrEqualTo(lowerLimit));
            Assert.That(updated, Is.LessThanOrEqualTo(now));
        }

        protected override string GetDatabaseName() => "dmarc";

        protected override Assembly GetSchemaAssembly() => Assembly.GetAssembly(typeof(Migrator));

        private Task Insert(string domain, DateTime lastChecked) =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO dmarc_scheduled_records (id, last_checked) VALUES (@domain, @last_checked)",
                new MySqlParameter("domain", domain),
                new MySqlParameter("last_checked", lastChecked));

        private async Task<DateTime> GetLastChecked(string domain) =>
            (DateTime)await MySqlHelper.ExecuteScalarAsync(ConnectionString,
                $"SELECT last_checked FROM dmarc_scheduled_records WHERE id = @domain",
                new MySqlParameter("domain", domain));

        private Task TruncateDatabase() =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString, "DELETE FROM dmarc_scheduled_records;");

    }
}
