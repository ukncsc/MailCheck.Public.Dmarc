using System;
using System.Collections.Generic;

namespace MailCheck.AggregateReport.Client.Domain
{
    public class AggregateReportSummary
    {
        public AggregateReportSummary(int totalEmail, Dictionary<DateTime, AggregateReportSummaryItem> results)
        {
            TotalEmail = totalEmail;
            Results = results;
        }

        public int TotalEmail { get; }
        public Dictionary<DateTime, AggregateReportSummaryItem> Results { get; }
    }
}
