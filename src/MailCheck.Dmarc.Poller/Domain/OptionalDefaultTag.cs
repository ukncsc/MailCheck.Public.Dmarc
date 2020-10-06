namespace MailCheck.Dmarc.Poller.Domain
{
    public class OptionalDefaultTag : Tag
    {
        public OptionalDefaultTag(string value, bool isImplicit) 
            : base(value)
        {
            IsImplicit = isImplicit;
        }

        public bool IsImplicit { get; }
    }
}