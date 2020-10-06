using System;
using System.Collections.Generic;

namespace MailCheck.Dkim.Client.Domain
{
    public class DkimHistoryItem
    {
        public DkimHistoryItem(DateTime startDate, DateTime? endDate, List<DkimHistoryEntry> entries)
        {
            StartDate = startDate;
            EndDate = endDate;
            Entries = entries ?? new List<DkimHistoryEntry>();
        }

        public DateTime StartDate { get; }
        public DateTime? EndDate { get; }

        public List<DkimHistoryEntry> Entries { get; }
    }
}