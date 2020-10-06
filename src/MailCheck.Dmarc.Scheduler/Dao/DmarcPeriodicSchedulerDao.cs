using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Dmarc.Scheduler.Config;
using MailCheck.Dmarc.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using MailCheck.Common.Data.Util;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dmarc.Scheduler.Dao
{
    public interface IDmarcPeriodicSchedulerDao
    {
        Task UpdateLastChecked(List<DmarcSchedulerState> entitiesToUpdate);
        Task<List<DmarcSchedulerState>> GetExpiredDmarcRecords();
    }

    public class DmarcPeriodicSchedulerDao : IDmarcPeriodicSchedulerDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;
        private readonly IDmarcPeriodicSchedulerConfig _config;

        public DmarcPeriodicSchedulerDao(IConnectionInfoAsync connectionInfo, IDmarcPeriodicSchedulerConfig config)
        {
            _connectionInfo = connectionInfo;
            _config = config;
        }

        public async Task<List<DmarcSchedulerState>> GetExpiredDmarcRecords()
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            List<DmarcSchedulerState> results = new List<DmarcSchedulerState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(connectionString,
                DmarcPeriodicSchedulerDaoResources.SelectDmarcRecordsToSchedule,
                new MySqlParameter("refreshIntervalSeconds", _config.RefreshIntervalSeconds),
                new MySqlParameter("limit", _config.DomainBatchSize)))
            {
                while (await reader.ReadAsync())
                {
                    results.Add(CreateDmarcSchedulerState(reader));
                }
            }

            return results;
        }

        public async Task UpdateLastChecked(List<DmarcSchedulerState> entitiesToUpdate)
        {
            string query = string.Format(DmarcPeriodicSchedulerDaoResources.UpdateDmarcRecordsLastChecked,
                string.Join(',', entitiesToUpdate.Select((_, i) => $"@domainName{i}")));

            MySqlParameter[] parameters = entitiesToUpdate
                .Select((_, i) => new MySqlParameter($"domainName{i}", _.Id.ToLower()))
                .ToArray();

            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            await MySqlHelper.ExecuteNonQueryAsync(connectionString, query, parameters);
        }

        private DmarcSchedulerState CreateDmarcSchedulerState(DbDataReader reader)
        {
            return new DmarcSchedulerState(reader.GetString("id"));
        }
    }
}
