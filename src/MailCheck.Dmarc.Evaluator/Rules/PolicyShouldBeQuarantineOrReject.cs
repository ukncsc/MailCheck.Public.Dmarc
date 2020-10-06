using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts;
using MailCheck.Dmarc.Contracts.SharedDomain;
using DmarcRecord = MailCheck.Dmarc.Contracts.SharedDomain.DmarcRecord;

namespace MailCheck.Dmarc.Evaluator.Rules
{
    public class PolicyShouldBeQuarantineOrReject : IRule<DmarcRecord>
    {
        public Guid Id => Guid.Parse("7432644D-3BF9-4094-83E8-15192D809A0A");

        public Task<List<Message>> Evaluate(DmarcRecord record)
        {
            Policy policy = record.Tags.OfType<Policy>().FirstOrDefault();
            List<Message> messages = new List<Message>();

            //Dont error on unknown because there will already be a parser error for this
            if (policy != null && policy.PolicyType == PolicyType.None)
            {
                string errorMessage = string.Format(DmarcRulesResource.PolicyShouldBeQuarantineOrRejectErrorMessage, policy.PolicyType);
                string markdown = DmarcRulesMarkDownResource.PolicyShouldBeQuarantineOrRejectErrorMessage;
                messages.Add(new Message(Id, MessageSources.DmarcEvaluator, Contracts.SharedDomain.MessageType.warning, errorMessage, markdown));
            }

            return Task.FromResult(messages);
        }

        public int SequenceNo => 2;
        public bool IsStopRule => false;
    }
}