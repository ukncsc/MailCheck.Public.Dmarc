using System;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Dmarc.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dmarc.Scheduler.Dao
{
    public interface IDmarcSchedulerDao
    {
        Task<DmarcSchedulerState> Get(string domain);
        Task Save(DmarcSchedulerState state);
        Task Delete(string domain);
    }

    public class DmarcSchedulerDao : IDmarcSchedulerDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;

        public DmarcSchedulerDao(IConnectionInfoAsync connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<DmarcSchedulerState> Get(string domain)
        {
            string id = (string)await MySqlHelper.ExecuteScalarAsync(
                await _connectionInfo.GetConnectionStringAsync(),
                DmarcSchedulerDaoResources.SelectDmarcRecord,
                new MySqlParameter("id", domain));

            return id == null
                ? null
                : new DmarcSchedulerState(id);
        }

        public async Task Save(DmarcSchedulerState state)
        {
            int numberOfRowsAffected = await MySqlHelper.ExecuteNonQueryAsync(
                await _connectionInfo.GetConnectionStringAsync(),
                DmarcSchedulerDaoResources.InsertDmarcRecord,
                new MySqlParameter("id", state.Id.ToLower()));

            if (numberOfRowsAffected == 0)
            {
                throw new InvalidOperationException($"Didn't save duplicate {nameof(DmarcSchedulerState)} for {state.Id}");
            }
        }

        public async Task Delete(string domain)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            await MySqlHelper.ExecuteNonQueryAsync(connectionString, DmarcSchedulerDaoResources.DeleteDmarcRecord, new MySqlParameter("id", domain));
        }
    }
}
