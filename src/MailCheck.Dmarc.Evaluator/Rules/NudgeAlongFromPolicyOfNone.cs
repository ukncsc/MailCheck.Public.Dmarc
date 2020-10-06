using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Api.Utils;
using MailCheck.Dkim.Client;
using MailCheck.Dkim.Client.Domain;
using MailCheck.Dmarc.Contracts;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Spf.Client;
using MailCheck.Spf.Client.Domain;
using Microsoft.Extensions.Logging;
using DmarcRecord = MailCheck.Dmarc.Contracts.SharedDomain.DmarcRecord;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;
using MessageType = MailCheck.Dmarc.Contracts.SharedDomain.MessageType;

namespace MailCheck.Dmarc.Evaluator.Rules
{
    public class NudgeAlongFromPolicyOfNone : IRule<DmarcRecord>
    {
        private readonly TimeSpan _thirtyDays = TimeSpan.FromDays(30);

        private readonly ISpfClient _spfApiClient;
        private readonly IDkimClient _dkimApiClient;
        private readonly ILogger<NudgeAlongFromPolicyOfNone> _log;

        public NudgeAlongFromPolicyOfNone(ISpfClient spfApiClient,
            IDkimClient dkimApiClient,
            ILogger<NudgeAlongFromPolicyOfNone> log)
        {
            _spfApiClient = spfApiClient;
            _dkimApiClient = dkimApiClient;
            _log = log;
        }

        public Guid Id => Guid.Parse("9D6D44F4-FDCD-442F-AE08-50A57DF25FEF");

        public Task<List<Message>> Evaluate(DmarcRecord record)
        {
            Policy policy = record.Tags.OfType<Policy>().FirstOrDefault();

            List<Message> messages = new List<Message>();

            if (policy?.PolicyType != PolicyType.None)
            {
                return Task.FromResult(messages);
            }

            try
            {
                Task<DkimHistoryResponse> dkimTask = _dkimApiClient.GetHistory(record.Domain);

                Task<SpfHistoryResponse> spfTask = _spfApiClient.GetHistory(record.Domain);

                Task.WhenAll(dkimTask, spfTask).GetAwaiter().GetResult();

                DkimHistoryItem latestDkimHistoryItem = GetLatestDkimHistoryItem(dkimTask.Result, record.Domain);

                SpfHistoryItem latestSpfHistoryItem = GetLatestSpfHistoryItem(spfTask.Result, record.Domain);

                DateTime now = DateTime.UtcNow;

                if (now.Subtract(latestDkimHistoryItem?.StartDate ?? now) >= _thirtyDays &&
                    now.Subtract(latestSpfHistoryItem?.StartDate ?? now) >= _thirtyDays)
                {
                    messages.Add(new Message(Id, MessageSources.DmarcEvaluator, MessageType.info, DmarcRulesResource.NudgeAlongFromPolicyOfNoneMessage, string.Empty, MessageDisplay.Prompt));
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Failed to process rule {nameof(NudgeAlongFromPolicyOfNone)} with following error: {e.Message}{Environment.NewLine}{e.StackTrace}");
            }

            return Task.FromResult(messages);
        }

        private SpfHistoryItem GetLatestSpfHistoryItem(SpfHistoryResponse spfHistoryResponse, string domain)
        {
            if (!spfHistoryResponse.StatusCode.IsSuccessStatusCode())
            {
                _log.LogWarning($"Failed to get spf history. Http status code: {spfHistoryResponse.StatusCode}");
                return null;
            }

            SpfHistoryItem latestSpfHistoryItem = spfHistoryResponse.SpfHistory.History.FirstOrDefault(_ => !_.EndDate.HasValue);

            if (latestSpfHistoryItem == null)
            {
                _log.LogWarning($"Didn't find latest spf history item for {domain}");
            }

            return latestSpfHistoryItem;
        }

        private DkimHistoryItem GetLatestDkimHistoryItem(DkimHistoryResponse dkimHistoryResponse, string domain)
        {
            if (!dkimHistoryResponse.StatusCode.IsSuccessStatusCode())
            {
                _log.LogWarning($"Failed to get dkim history. Http status code: {dkimHistoryResponse.StatusCode}");
                return null;
            }

            DkimHistoryItem latestDkimHistoryItem = dkimHistoryResponse.History.History.FirstOrDefault(_ => !_.EndDate.HasValue);

            if (latestDkimHistoryItem == null)
            {
                _log.LogWarning($"Didn't find latest dkim history item for {domain}");
            }

            return latestDkimHistoryItem;
        }

        public int SequenceNo => 5;
        public bool IsStopRule => false;
    }
}
