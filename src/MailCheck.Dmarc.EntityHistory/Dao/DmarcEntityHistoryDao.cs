using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Dmarc.EntityHistory.Entity;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dmarc.EntityHistory.Dao
{
    public interface IDmarcEntityHistoryDao
    {
        Task<DmarcEntityHistoryState> Get(string domain);
        Task Save(DmarcEntityHistoryState state);
    }

    public class DmarcEntityHistoryDao : IDmarcEntityHistoryDao
    {
        private readonly IConnectionInfoAsync _connectionInfoAsync;

        public DmarcEntityHistoryDao(IConnectionInfoAsync connectionInfoAsync)
        {
            _connectionInfoAsync = connectionInfoAsync;
        }
        public async Task<DmarcEntityHistoryState> Get(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string state = (string)await MySqlHelper.ExecuteScalarAsync(connectionString, DmarcEntityHistoryDaoResouces.SelectDmarcHistoryEntity,
                new MySqlParameter("domain", domain));

            return state == null
                ? null
                : JsonConvert.DeserializeObject<DmarcEntityHistoryState>(state);
        }

        public async Task Save(DmarcEntityHistoryState state)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string serializedState = JsonConvert.SerializeObject(state, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                DmarcEntityHistoryDaoResouces.InsertDmarcEntityHistory,
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("state", serializedState));
        }
    }
}
