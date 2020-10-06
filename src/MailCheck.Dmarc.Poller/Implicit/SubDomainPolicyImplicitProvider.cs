using System.Collections.Generic;
using System.Linq;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Implicit
{
    public class SubDomainPolicyImplicitProvider : ImplicitTagProviderStrategyBase<SubDomainPolicy>
    {
        public SubDomainPolicyImplicitProvider() 
            : base(Factory){}

        private static SubDomainPolicy Factory(List<Tag> tags)
        {
            Policy policy = tags.OfType<Policy>().FirstOrDefault();
            return policy == null
                ? null
                : new SubDomainPolicy($"s{policy.Value.Replace(";", string.Empty)}", policy.PolicyType, true);
        }
    }
}