namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class ReportInterval : OptionalDefaultTag
    {
        public static ReportInterval Default = new ReportInterval("ri=86400", 86400, true);

        public ReportInterval(string value, uint? interval, bool valid, bool isImplicit = false) 
            : base(TagType.ReportInterval, value, valid, isImplicit)
        {
            Interval = interval;
        }

        public uint? Interval { get; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Interval)}: {Interval}";
        }
    }
}