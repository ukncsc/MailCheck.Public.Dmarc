namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class SubDomainPolicy : OptionalDefaultTag
    {
        public SubDomainPolicy(string value, PolicyType policyType, bool allValid, bool isImplicit=false) : base(TagType.SubDomainPolicy, value, allValid, isImplicit)
        {
            PolicyType = policyType;
        }

        public PolicyType PolicyType { get; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(PolicyType)}: {PolicyType}";
        }
    }
}