using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Entity.Dao;
using MailCheck.Dmarc.Entity.Entity;
using MailCheck.Dmarc.Migration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NUnit.Framework;
using MailCheck.Common.Data.Util;
using MailCheck.Common.TestSupport;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dmarc.Entity.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class DmarcEntityDaoTests : DatabaseTestBase
    {
        private const string Id = "abc.com";

        private DmarcEntityDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            IConnectionInfoAsync connectionInfoAsync = A.Fake<IConnectionInfoAsync>();
            A.CallTo(() => connectionInfoAsync.GetConnectionStringAsync()).Returns(ConnectionString);

            _dao = new DmarcEntityDao(connectionInfoAsync);
        }

        [Test]
        public async Task GetNoStateExistsReturnsNull()
        {
            DmarcEntityState state = await _dao.Get(Id);
            Assert.That(state, Is.Null);
        }

        [Test]
        public async Task GetStateExistsReturnsState()
        {
            DmarcEntityState state = new DmarcEntityState(Id, 1, DmarcState.PollPending, DateTime.UtcNow);

            await Insert(state);

            DmarcEntityState stateFromDatabase = await _dao.Get(Id);

            Assert.That(stateFromDatabase.Id, Is.EqualTo(state.Id));
            Assert.That(stateFromDatabase.Version, Is.EqualTo(state.Version));
        }

        [Test]
        public async Task SaveStateExistsStateIsUpdated()
        {
            DmarcEntityState state = new DmarcEntityState(Id, 1, DmarcState.PollPending, DateTime.UtcNow);

            await _dao.Save(state);

            state = new DmarcEntityState(Id, 2, DmarcState.PollPending, DateTime.UtcNow);

            await _dao.Save(state);

            List<DmarcEntityState> states = await SelectAll(Id);

            Assert.That(states.Count, Is.EqualTo(1));
            Assert.That(states[0].Id, Is.EqualTo(state.Id));
            Assert.That(states[0].Version, Is.EqualTo(state.Version));
        }

        [Test]
        public async Task SaveDuplicateEntryThrows()
        {
            DmarcEntityState state1 = new DmarcEntityState(Id, 1, DmarcState.PollPending, DateTime.UtcNow);

            await _dao.Save(state1);
            Assert.ThrowsAsync<InvalidOperationException>(() => _dao.Save(state1));
        }
      
        protected override string GetDatabaseName() => "dmarcentity";

        protected override Assembly GetSchemaAssembly()
        {
            return Assembly.GetAssembly(typeof(Migrator));
        }

        #region TestSupport

        private async Task Insert(DmarcEntityState state)
        {
            await MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO `dmarc_entity`(`id`,`version`,`state`)VALUES(@domain,@version,@state)",
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("version", state.Version),
                new MySqlParameter("state", JsonConvert.SerializeObject(state)));
        }

        private async Task<List<DmarcEntityState>> SelectAll(string id)
        {
            List<DmarcEntityState> list = new List<DmarcEntityState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(ConnectionString,
                @"SELECT state FROM dmarc_entity WHERE id = @domain ORDER BY version;",
                new MySqlParameter("domain", id)))
            {
                while (reader.Read())
                {
                    string state = reader.GetString("state");

                    if (!string.IsNullOrWhiteSpace(state))
                    {
                        list.Add(JsonConvert.DeserializeObject<DmarcEntityState>(state));
                    }
                }
            }

            return list;
        }

        #endregion
    }
}
