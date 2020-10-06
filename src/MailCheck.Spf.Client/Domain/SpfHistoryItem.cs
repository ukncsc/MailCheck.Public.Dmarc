using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MailCheck.Spf.Client.Domain
{
    public class SpfHistoryItem
    {
        public SpfHistoryItem(DateTime startDate, DateTime? endDate, List<string> records)
        {
            StartDate = startDate;
            EndDate = endDate;
            Records = records;
        }

        public DateTime StartDate { get; }
        public DateTime? EndDate { get; }

        [JsonProperty("SpfRecords")]
        public List<string> Records { get; }
    }
}