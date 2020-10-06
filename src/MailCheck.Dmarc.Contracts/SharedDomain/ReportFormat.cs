namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class ReportFormat : OptionalDefaultTag
    {
        public static ReportFormat Default = new ReportFormat("rf=AFRF", ReportFormatType.AFRF, true);

        public ReportFormat(string value, ReportFormatType reportFormatType, bool valid, bool isImplicit = false) 
            : base(TagType.ReportFormat, value, valid, isImplicit)
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