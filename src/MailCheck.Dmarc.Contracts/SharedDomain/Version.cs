namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class Version : Tag
    {
        public Version(string value, bool valid) 
            : base(TagType.Version, value, valid)
        {}
    }
}