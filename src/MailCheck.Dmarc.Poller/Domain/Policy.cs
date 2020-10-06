namespace MailCheck.Dmarc.Poller.Domain
{
    public class Policy : Tag
    {
        public Policy(string value, PolicyType policyType) : base(value)
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