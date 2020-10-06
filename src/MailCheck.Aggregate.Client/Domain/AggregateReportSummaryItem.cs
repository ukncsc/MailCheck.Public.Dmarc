namespace MailCheck.AggregateReport.Client.Domain
{
    public class AggregateReportSummaryItem
    {
        public AggregateReportSummaryItem(int fullyTrusted, int partiallyTrusted, int untrusted, int quarantined, int rejected)
        {
            FullyTrusted = fullyTrusted;
            PartiallyTrusted = partiallyTrusted;
            Untrusted = untrusted;
            Quarantined = quarantined;
            Rejected = rejected;
            Total = fullyTrusted + partiallyTrusted + untrusted + quarantined + rejected;
        }

        public int FullyTrusted { get; }
        public int PartiallyTrusted { get; }
        public int Untrusted { get; }
        public int Quarantined { get; }
        public int Rejected { get; }
        public int Total { get; }
    }
}