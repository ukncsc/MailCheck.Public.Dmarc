using System;
using System.Threading.Tasks;
using Dapper;
using MailCheck.Common.Data;
using MailCheck.Dmarc.Scheduler.Dao.Model;

namespace MailCheck.Dmarc.Scheduler.Dao
{
    public interface IDmarcSchedulerDao
    {
        Task<DmarcSchedulerState> Get(string domain);
        Task Save(DmarcSchedulerState state);
        Task<int> Delete(string domain);
    }

    public class DmarcSchedulerDao : IDmarcSchedulerDao
    {
        private readonly IDatabase _database;

        public DmarcSchedulerDao(IDatabase database)
        {
            _database = database;
        }

        public async Task<DmarcSchedulerState> Get(string domain)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                string id = await connection.QueryFirstOrDefaultAsync<string>(DmarcSchedulerDaoResources.SelectDmarcRecord,
                    new {id = domain});
                
                return id == null
                    ? null
                    : new DmarcSchedulerState(id);
            }
        }

        public async Task Save(DmarcSchedulerState state)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                int numberOfRowsAffected = await connection.ExecuteAsync(DmarcSchedulerDaoResources.InsertDmarcRecord,
                    new {id = state.Id.ToLower()});
                
                if (numberOfRowsAffected == 0)
                {
                    throw new InvalidOperationException($"Didn't save duplicate {nameof(DmarcSchedulerState)} for {state.Id}");
                }
            }
        }

        public async Task<int> Delete(string domain)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                return await connection.ExecuteAsync(DmarcSchedulerDaoResources.DeleteDmarcRecord,
                    new { id = domain });
            }
        }
    }
}
