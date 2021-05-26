using System;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Dmarc.Entity.Entity;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dmarc.Entity.Dao
{
    public interface IDmarcEntityDao
    {
        Task<DmarcEntityState> Get(string domain);
        Task Save(DmarcEntityState state);
        Task<int> Delete(string domain);
    }

    public class DmarcEntityDao : IDmarcEntityDao
    {
        private readonly IConnectionInfoAsync _connectionInfoAsync;

        public DmarcEntityDao(IConnectionInfoAsync connectionInfoAsync)
        {
            _connectionInfoAsync = connectionInfoAsync;
        }

        public async Task<DmarcEntityState> Get(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string state = (string)await MySqlHelper.ExecuteScalarAsync(connectionString, DmarcEntityDaoResouces.SelectDmarcEntity,
                new MySqlParameter("domain", domain));

            return state == null
                ? null
                : JsonConvert.DeserializeObject<DmarcEntityState>(state);
        }

        public async Task Save(DmarcEntityState state)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string serializedState = JsonConvert.SerializeObject(state);

            int rowsAffected = await MySqlHelper.ExecuteNonQueryAsync(connectionString, DmarcEntityDaoResouces.InsertDmarcEntity,
                new MySqlParameter("domain", state.Id.ToLower()),
                new MySqlParameter("version", state.Version),
                new MySqlParameter("state", serializedState));

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    $"Didn't update DmarcEntityState because version {state.Version} has already been persisted.");
            }
        }

        public async Task<int> Delete(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            return await MySqlHelper.ExecuteNonQueryAsync(connectionString, DmarcEntityDaoResouces.DeleteDmarcEntity, new MySqlParameter("id", domain));
        }
    }
}
