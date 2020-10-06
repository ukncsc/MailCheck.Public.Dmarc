namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class UnknownTag : Tag
    {
        public UnknownTag(string type, string value, bool valid)
            : base(TagType.Unknown, value, valid)
        {
            Type = type;
        }

        public string Type { get; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Type)}: {Type}";
        }
    }
}