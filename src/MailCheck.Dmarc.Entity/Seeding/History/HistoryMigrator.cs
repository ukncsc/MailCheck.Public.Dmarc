using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Util;

namespace MailCheck.Dmarc.Entity.Seeding.History
{
    public interface IHistoryMigrator
    {
        Task Migrate();
    }

    public class HistoryMigrator : IHistoryMigrator
    {
        private readonly IHistoryReaderDao _historyReaderDao;
        private readonly IHistoryWriterDao _historyWriterDao;

        public HistoryMigrator(IHistoryReaderDao historyReaderDao, IHistoryWriterDao historyWriterDao)
        {
            _historyReaderDao = historyReaderDao;
            _historyWriterDao = historyWriterDao;
        }

        public async Task Migrate()
        {
            List<HistoryItem> historyItems = await _historyReaderDao.GetHistory();

            List<DmarcRecordState> states = historyItems
                .GroupBy(_ => _.Id)
                .Select(CreateDmarcRecordState)
                .Select(_ => _)
                .ToList();

            IEnumerable<IEnumerable<DmarcRecordState>> batches = states.Batch(500);

            foreach (IEnumerable<DmarcRecordState> batch in batches)
            {
                await _historyWriterDao.WriteHistory(batch.ToList());
            }
        }

        private DmarcRecordState CreateDmarcRecordState(IGrouping<string, HistoryItem> domainHistory)
        {
            IEnumerable<IGrouping<DateTime, HistoryItem>> groupByStartDate = domainHistory.GroupBy(_ => _.StartDate);

            List<DmarcHistoryRecord> dmarcHistoryRecords = new List<DmarcHistoryRecord>();

            foreach (IGrouping<DateTime, HistoryItem> groupDmarc in groupByStartDate)
            {
                List<HistoryItem> records = groupDmarc.ToList();

                List<string> dmarcRecords = records.Where(x => !string.IsNullOrWhiteSpace(x.Record))
                    .Select(_ => _.Record).ToList();

                HistoryItem historyItem = records.First();

                DmarcHistoryRecord dmarcHistoryRecord = new DmarcHistoryRecord(historyItem?.OrgDomain, dmarcRecords,
                    historyItem.IsTld, historyItem.IsInherited, historyItem.StartDate, historyItem.EndDate);

                dmarcHistoryRecords.Add(dmarcHistoryRecord);
            }

            List<DmarcHistoryRecord> toDeleteDmarcHistoryRecords = new List<DmarcHistoryRecord>();

            for (int i = 1; i < dmarcHistoryRecords.Count; i++)
            {
                DmarcHistoryRecord current = dmarcHistoryRecords[i];
                DmarcHistoryRecord previous = dmarcHistoryRecords[i - 1];

                if (current.EndDate == previous.EndDate)
                {
                    current.DmarcRecords.AddRange(previous.DmarcRecords);
                    previous.EndDate = current.StartDate;
                }
                else
                {
                    IEnumerable<string> x = current.DmarcRecords.Except(previous.DmarcRecords);

                    if (!x.Any() && previous.DmarcRecords.Count == current.DmarcRecords.Count)
                    {
                        current.StartDate = previous.StartDate;
                        toDeleteDmarcHistoryRecords.Add(previous);
                    }
                }
            }

            dmarcHistoryRecords.RemoveAll(x => toDeleteDmarcHistoryRecords.Contains(x));

            dmarcHistoryRecords.Reverse();

            return new DmarcRecordState(domainHistory.Key, dmarcHistoryRecords);
        }
    }
}
