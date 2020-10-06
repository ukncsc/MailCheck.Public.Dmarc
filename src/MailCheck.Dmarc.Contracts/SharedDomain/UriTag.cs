namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class UriTag
    {
        public UriTag(string value, DmarcUri uri, MaxReportSize maxReportSize, bool valid)
        {
            Value = value;
            Uri = uri;
            MaxReportSize = maxReportSize;
            Valid = valid;
        }

        public string Value { get; }
        public DmarcUri Uri { get; }
        public MaxReportSize MaxReportSize { get; }
        public bool Valid { get; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}, {nameof(Uri)}: {Uri}, {nameof(MaxReportSize)}: {MaxReportSize}";
        }
    }
}