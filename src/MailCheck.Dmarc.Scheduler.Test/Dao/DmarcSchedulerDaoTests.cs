using System;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.TestSupport;
using MailCheck.Dmarc.Migration;
using MailCheck.Dmarc.Scheduler.Dao;
using MailCheck.Dmarc.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dmarc.Scheduler.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class DmarcSchedulerDaoTests : DatabaseTestBase
    {
        private DmarcSchedulerDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            TruncateDatabase().Wait();

            IDatabase database = A.Fake<IDatabase>();

            _dao = new DmarcSchedulerDao(database);
        }

        [Test]
        public async Task ItShouldReturnNullIfTheStateDoesntExist()
        {
            DmarcSchedulerState state = await _dao.Get("ncsc.gov.uk");
            Assert.That(state, Is.Null);
        }

        [Test]
        public async Task ItShouldGetTheStateIfItExists()
        {
            await Insert("ncsc.gov.uk");

            DmarcSchedulerState state = await _dao.Get("ncsc.gov.uk");

            Assert.AreEqual(state.Id, "ncsc.gov.uk");
        }

        [Test]
        public async Task ItShouldSaveTheStateIfItDoesNotExist()
        {
            await _dao.Save(new DmarcSchedulerState("ncsc.gov.uk"));

            await _dao.Get("ncsc.gov.uk");

            DmarcSchedulerState state = await _dao.Get("ncsc.gov.uk");

            Assert.AreEqual(state.Id, "ncsc.gov.uk");
        }

        [Test]
        public async Task ItShouldThrowAnExceptionIfTheStateAlreadyExists()
        {
            await Insert("ncsc.gov.uk");

            Assert.ThrowsAsync<InvalidOperationException>(() => _dao.Save(new DmarcSchedulerState("ncsc.gov.uk")));
        }

        protected override string GetDatabaseName() => "dmarc";

        protected override Assembly GetSchemaAssembly() => Assembly.GetAssembly(typeof(Migrator));

        private Task Insert(string domain) =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO dmarc_scheduled_records (id, last_checked) VALUES (@domain, UTC_TIMESTAMP())",
                new MySqlParameter("domain", domain));

        private Task TruncateDatabase() =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString, "DELETE FROM dmarc_scheduled_records;");
    }
}
