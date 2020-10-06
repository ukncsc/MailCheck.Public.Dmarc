using System;
using System.Collections.Generic;

namespace MailCheck.Dmarc.Entity.Seeding.History
{
    public class HistoryItem
    {
        public HistoryItem(string id, string orgDomain, bool isTld, bool isInherited, string record,
            DateTime startDate, DateTime? endDate = null)
        {
            Id = id;
            OrgDomain = orgDomain;
            IsTld = isTld;
            IsInherited = isInherited;
            Record = record;
            StartDate = startDate;
            EndDate = endDate;
        }

        public string Id { get; }
        public string OrgDomain { get;  }
        public bool IsTld { get; }
        public bool IsInherited { get; }
        public string Record { get; }
        public DateTime StartDate { get; }
        public DateTime? EndDate { get; }
    }
}