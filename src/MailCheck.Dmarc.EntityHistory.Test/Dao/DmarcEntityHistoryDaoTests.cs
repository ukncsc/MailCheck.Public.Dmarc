using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.Common.TestSupport;
using MailCheck.Dmarc.EntityHistory.Dao;
using MailCheck.Dmarc.EntityHistory.Entity;
using MailCheck.Dmarc.Migration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NUnit.Framework;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dmarc.EntityHistory.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class DmarcEntityHistoryDaoTests : DatabaseTestBase
    {
        private const string Id = "abc.com";

        private IDmarcEntityHistoryDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            IConnectionInfoAsync connectionInfoAsync = A.Fake<IConnectionInfoAsync>();
            A.CallTo(() => connectionInfoAsync.GetConnectionStringAsync()).Returns(ConnectionString);

            _dao = new DmarcEntityHistoryDao(connectionInfoAsync);
        }

        [Test]
        public async Task GetNoStateExistsReturnsNull()
        {
            DmarcEntityHistoryState state = await _dao.Get(Id);
            Assert.That(state, Is.Null);
        }

        [Test]
        public async Task GetStateExistsReturnsState()
        {
            string dmarcRecord1 = "dmarcRecord1";

            DmarcEntityHistoryState state = new DmarcEntityHistoryState(Id,
                new List<DmarcHistoryRecord> { new DmarcHistoryRecord(DateTime.UtcNow.AddDays(-1), null, new List<string> { dmarcRecord1 }) });
            
            await Insert(state);

            DmarcEntityHistoryState stateFromDatabase = await _dao.Get(Id);

            Assert.That(stateFromDatabase.Id, Is.EqualTo(state.Id));
            Assert.That(stateFromDatabase.DmarcHistory.Count, Is.EqualTo(state.DmarcHistory.Count));
            Assert.That(stateFromDatabase.DmarcHistory[0].DmarcRecords.Count, Is.EqualTo(state.DmarcHistory[0].DmarcRecords.Count));
            Assert.That(stateFromDatabase.DmarcHistory[0].DmarcRecords[0], Is.EqualTo(state.DmarcHistory[0].DmarcRecords[0]));
        }

        [Test]
        public async Task HistoryIsSavedForChanges()
        {
            string dmarcRecord1 = "dmarcRecord1";
            string dmarcRecord2 = "dmarcRecord2";

            DmarcEntityHistoryState state = new DmarcEntityHistoryState(Id,
                new List<DmarcHistoryRecord> { new DmarcHistoryRecord(DateTime.UtcNow.AddDays(-1), null, new List<string> { dmarcRecord1 }) });

            await _dao.Save(state);

            DmarcEntityHistoryState state2 = (await SelectAllHistory(Id)).First();
            state2.DmarcHistory[0].EndDate = DateTime.UtcNow;
            state2.DmarcHistory.Insert(0, new DmarcHistoryRecord(DateTime.UtcNow, null, new List<string> { dmarcRecord2 }));

            await _dao.Save(state2);

            List<DmarcEntityHistoryState> historyStates = await SelectAllHistory(Id);
            Assert.That(historyStates[0].DmarcHistory.Count, Is.EqualTo(2));

            Assert.That(historyStates[0].DmarcHistory[0].EndDate, Is.Null);
            Assert.That(historyStates[0].DmarcHistory[0].DmarcRecords.Count, Is.EqualTo(1));
            Assert.That(historyStates[0].DmarcHistory[0].DmarcRecords[0], Is.EqualTo(dmarcRecord2));

            Assert.That(historyStates[0].DmarcHistory[1].EndDate, Is.Not.Null);
            Assert.That(historyStates[0].DmarcHistory[1].DmarcRecords.Count, Is.EqualTo(1));
            Assert.That(historyStates[0].DmarcHistory[1].DmarcRecords[0], Is.EqualTo(dmarcRecord1));
        }

        protected override string GetDatabaseName() => "dmarcentity";

        protected override Assembly GetSchemaAssembly()
        {
            return Assembly.GetAssembly(typeof(Migrator));
        }

        #region TestSupport

        private async Task Insert(DmarcEntityHistoryState state)
        {
            await MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO `dmarc_entity_history`(`id`,`state`)VALUES(@domain,@state)",
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("state", JsonConvert.SerializeObject(state)));
        }

        private async Task<List<DmarcEntityHistoryState>> SelectAllHistory(string id)
        {
            List<DmarcEntityHistoryState> list = new List<DmarcEntityHistoryState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(ConnectionString,
                @"SELECT state FROM dmarc_entity_history WHERE id = @domain ORDER BY id;",
                new MySqlParameter("domain", id)))
            {
                while (reader.Read())
                {
                    string state = reader.GetString("state");

                    if (!string.IsNullOrWhiteSpace(state))
                    {
                        list.Add(JsonConvert.DeserializeObject<DmarcEntityHistoryState>(state));
                    }
                }
            }

            return list;
        }

        #endregion
    }
}
