namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class OptionalDefaultTag : Tag
    {
        public OptionalDefaultTag(TagType tagType, string value, bool valid, bool isImplicit) 
            : base(tagType, value, valid)
        {
            AllValid = valid;
            IsImplicit = isImplicit;
        }

        public bool AllValid { get; }
        public bool IsImplicit { get; }
    }
}