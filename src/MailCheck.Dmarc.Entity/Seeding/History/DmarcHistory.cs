using System;
using System.Collections.Generic;
using System.Text;

namespace MailCheck.Dmarc.Entity.Seeding.History
{
    public class DmarcHistoryRecord
    {
        public DmarcHistoryRecord(string orgDomain, List<string> dmarcRecords,  bool isTld, bool isInherited, DateTime startDate, DateTime? endDate)
        {
            OrgDomain = orgDomain;
            IsTld = isTld;
            IsInherited = isInherited;
            StartDate = startDate;
            EndDate = endDate;
            Id = Guid.NewGuid().ToString();
            DmarcRecords = dmarcRecords ?? new List<string>();
        }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> DmarcRecords { get; }
        public string Id { get; set; }
        public string OrgDomain { get; set; }
        public bool IsTld { get; set; }
        public bool IsInherited { get; set;}
    }

    public class DmarcRecordState
    {
        public DmarcRecordState(string id, List<DmarcHistoryRecord> records = null)
        {
            Id = id;
            DmarcHistory = records ?? new List<DmarcHistoryRecord>();
        }

        public string Id { get; set; }
        public List<DmarcHistoryRecord> DmarcHistory { get; set; }
    }

}
