using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dmarc.Entity.Seeding.History
{
    public interface IHistoryReaderDao
    {
        Task<List<HistoryItem>> GetHistory();
    }

    public class HistoryReaderDao : IHistoryReaderDao
    {
        private readonly IConnectionInfo _connectionInfo;

        public HistoryReaderDao(IConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<List<HistoryItem>> GetHistory()
        {
            string command =
                "SELECT name as \'entity_id\', org_domain, is_tld, is_inherited, record, start_date, end_date \nFROM domain d\nLEFT JOIN dns_record_dmarc dmarc ON dmarc.domain_id = d.id\nWHERE (d.monitor = b\'1\' OR d.publish = b\'1\') AND dmarc.start_date IS NOT NULL \nORDER BY name, start_date;";

            List<HistoryItem> historyItems = new List<HistoryItem>();
            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(_connectionInfo.ConnectionString, command))
            {
                while (await reader.ReadAsync())
                {
                    historyItems.Add(new HistoryItem(
                        reader.GetString("entity_id"),
                        reader.GetString("org_domain"),
                        reader.GetBoolean("is_tld"),
                        reader.GetBoolean("is_inherited"),
                        reader.GetString("record"),
                        reader.GetDateTime("start_date"),
                        reader.GetDateTimeNullable("end_date")));
                }
            }

            return historyItems;
        }
    }
}
