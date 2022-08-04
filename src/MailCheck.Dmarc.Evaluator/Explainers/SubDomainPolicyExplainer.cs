using System;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public class SubDomainPolicyExplainer : BaseTagExplainerStrategy<SubDomainPolicy>
    {
        public override string GetExplanation(SubDomainPolicy tConcrete)
        {
            switch (tConcrete.PolicyType)
            {
                case PolicyType.None:
                    return DmarcExplainerResource.SubDomainPolicyNoneExplanation;
                case PolicyType.Quarantine:
                    return DmarcExplainerResource.SubDomainPolicyQuarantineExplanation;
                case PolicyType.Reject:
                    return DmarcExplainerResource.SubDomainPolicyRejectExplanation;
                default:
                    return DmarcExplainerResource.PolicyUnknownExplanation;
            }
        }
    }
}