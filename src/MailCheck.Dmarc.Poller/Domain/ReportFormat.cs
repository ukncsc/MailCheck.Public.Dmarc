namespace MailCheck.Dmarc.Poller.Domain
{
    public class ReportFormat : OptionalDefaultTag
    {
        public static ReportFormat Default = new ReportFormat("rf=AFRF", ReportFormatType.AFRF, true);

        public ReportFormat(string value, ReportFormatType reportFormatType, bool isImplicit = false) 
            : base(value, isImplicit)
        {
            ReportFormatType = reportFormatType;
        }

        public ReportFormatType ReportFormatType { get; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(ReportFormatType)}: {ReportFormatType}";
        }
    }
}