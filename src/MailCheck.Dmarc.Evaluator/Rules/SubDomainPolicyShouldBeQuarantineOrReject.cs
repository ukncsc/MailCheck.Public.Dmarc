using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Rules
{
    public class SubDomainPolicyShouldBeQuarantineOrReject : IRule<DmarcRecord>
    {
        public Guid Id => Guid.Parse("75E84628-BA93-4193-9675-B3E5F395D6FD");

        public Task<List<Message>> Evaluate(DmarcRecord record)
        {
            SubDomainPolicy subDomainPolicy = record.Tags.OfType<SubDomainPolicy>().FirstOrDefault();
            Policy policy = record.Tags.OfType<Policy>().FirstOrDefault();

            List<Message> messages = new List<Message>();

            if (!record.IsInherited && subDomainPolicy != null && policy.PolicyType != PolicyType.None && subDomainPolicy.PolicyType == PolicyType.None)
            {
                string errorMessage = string.Format(DmarcRulesResource.SubdomainPolicyMustBeQuarantineOrRejectErrorMessage, subDomainPolicy?.PolicyType);
                string markDown = DmarcRulesMarkDownResource.SubdomainPolicyMustBeQuarantineOrRejectErrorMessage;

                messages.Add(new Message(Id, MessageSources.DmarcEvaluator, MessageType.warning, errorMessage, markDown));
            }

            return Task.FromResult(messages);
        }

        public int SequenceNo => 3;
        public bool IsStopRule => false;
    }
}
