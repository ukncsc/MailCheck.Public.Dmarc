using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Dmarc.EntityHistory.Entity
{
    public class DmarcHistoryRecord
    {
        public DmarcHistoryRecord(DateTime startDate, DateTime? endDate, List<string> dmarcRecords = null)
        {
            StartDate = startDate;
            EndDate = endDate;
            this.DmarcRecords = dmarcRecords ?? new List<string>();
        }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> DmarcRecords { get; set; }
    }

    public class DmarcEntityHistoryState 
    {
        public DmarcEntityHistoryState(string id, List<DmarcHistoryRecord> records = null)
        {
            Id = id;
            DmarcHistory = records ?? new List<DmarcHistoryRecord>();
        }

        public string Id { get; set; }
        public List<DmarcHistoryRecord> DmarcHistory { get; set; }

        public bool UpdateHistory(List<string> polledRecords, DateTime timeStamp)
        {
            bool hasChanged;

            DmarcHistoryRecord currentRecord = DmarcHistory.FirstOrDefault();

            if (currentRecord == null)
            {
                DmarcHistory.Add(new DmarcHistoryRecord(timeStamp, null, polledRecords));
                hasChanged = true;
            }
            else
            {
                List<string> cleanedCurrentRecords = currentRecord.DmarcRecords.Select(x => x.Replace("; ", ";")).ToList();
                List<string> cleanedPolledRecords = polledRecords.Select(x => x.Replace("; ", ";")).ToList();

                hasChanged = !(cleanedCurrentRecords.All(cleanedPolledRecords.Contains) && cleanedPolledRecords.Count == cleanedCurrentRecords.Count);

                if (hasChanged)
                {
                    currentRecord.EndDate = timeStamp;

                    DmarcHistory.Insert(0, new DmarcHistoryRecord(timeStamp, null, cleanedPolledRecords));
                }
            }


            return hasChanged;
        }
    }
}
