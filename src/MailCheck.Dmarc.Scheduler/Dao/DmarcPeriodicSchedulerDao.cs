using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Scheduler.Config;
using MailCheck.Dmarc.Scheduler.Dao.Model;
using System;
using MailCheck.Common.Util;
using MailCheck.Common.Data;
using Dapper;

namespace MailCheck.Dmarc.Scheduler.Dao
{
    public interface IDmarcPeriodicSchedulerDao
    {
        Task UpdateLastChecked(List<DmarcSchedulerState> entitiesToUpdate);
        Task<List<DmarcSchedulerState>> GetExpiredDmarcRecords();
    }
    
    public class DmarcPeriodicSchedulerDao : IDmarcPeriodicSchedulerDao
    {
        private readonly IDatabase _database;
        private readonly IDmarcPeriodicSchedulerConfig _config;
        private readonly IClock _clock;

        public DmarcPeriodicSchedulerDao(IDatabase database, IDmarcPeriodicSchedulerConfig config, IClock clock)
        {
            _database = database;
            _config = config;
            _clock = clock;
        }

        public async Task<List<DmarcSchedulerState>> GetExpiredDmarcRecords()
        {
            DateTime nowMinusInterval = _clock.GetDateTimeUtc().AddSeconds(- _config.RefreshIntervalSeconds);

            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                var records = (await connection.QueryAsync<string>(
                    DmarcPeriodicSchedulerDaoResources.SelectDmarcRecordsToSchedule,
                    new {now_minus_interval = nowMinusInterval, limit = _config.DomainBatchSize})).ToList();

                return records.Select(record => new DmarcSchedulerState(record)).ToList();
            }
        }

        public async Task UpdateLastChecked(List<DmarcSchedulerState> entitiesToUpdate)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                var parameters = entitiesToUpdate.Select(ent => new { id = ent.Id, lastChecked = GetAdjustedLastCheckedTime() }).ToArray();
                await connection.ExecuteAsync(DmarcPeriodicSchedulerDaoResources.UpdateDmarcRecordsLastCheckedDistributed, parameters);
            }
        }

        private DateTime GetAdjustedLastCheckedTime()
        {
            return _clock.GetDateTimeUtc().AddSeconds(-(new Random().NextDouble() * 3600));
        }
    }
}
