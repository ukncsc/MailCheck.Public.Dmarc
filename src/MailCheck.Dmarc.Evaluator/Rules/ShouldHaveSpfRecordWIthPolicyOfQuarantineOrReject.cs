using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Evaluator.Util;
using MailCheck.Spf.Client;
using MailCheck.Spf.Client.Domain;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;
using MessageType = MailCheck.Dmarc.Contracts.SharedDomain.MessageType;

namespace MailCheck.Dmarc.Evaluator.Rules
{
    public class ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject : IRule<DmarcRecord>
    {
        private readonly ISpfClient _spfApiClient;

        public ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject(ISpfClient spfApiClient)
        {
            _spfApiClient = spfApiClient;
        }

        public async Task<List<Message>> Evaluate(DmarcRecord record)
        {
            List<Message> messages = new List<Message>();

            Policy policy = record.GetEffectivePolicy();

            if (policy == null || policy.PolicyType == PolicyType.None)
            {
                return messages;
            }

            SpfResponse response = await _spfApiClient.GetSpf(record.Domain);

            if (response.StatusCode == HttpStatusCode.OK && !(response.Spf?.SpfRecords?.Records?.Any() ?? false))
            {
                messages.Add(new Message(Guid.Parse("5c1e5fa3-5f45-4e87-bd5c-6378e1d5def8"),
                    MessageSources.DmarcEvaluator, MessageType.info,
                    DmarcRulesResource.ShouldHaveSpfRecordWIthPolicyOfQuarantineOrReject, null));
            }

            return messages;
        }

        public int SequenceNo => 7;
        public bool IsStopRule => false;
    }
}