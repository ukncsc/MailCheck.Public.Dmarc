using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Findings;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Notifiers;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Entity.Config;
using Microsoft.Extensions.Logging;
using ErrorMessage = MailCheck.Dmarc.Contracts.SharedDomain.Message;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.Dmarc.Entity.Entity.Notifiers
{
    public class FindingsChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IFindingsChangedNotifier _findingsChangedNotifier;
        private readonly IDmarcEntityConfig _dmarcEntityConfig;

        public FindingsChangedNotifier(
            IMessageDispatcher dispatcher,
            IFindingsChangedNotifier findingsChangedNotifier,
            IDmarcEntityConfig dmarcEntityConfig)
        {
            _dispatcher = dispatcher;
            _findingsChangedNotifier = findingsChangedNotifier;
            _dmarcEntityConfig = dmarcEntityConfig;
        }

        public void Handle(DmarcEntityState state, Message message)
        {
            string messageId = state.Id.ToLower();

            if (message is DmarcRecordsEvaluated evaluationResult)
            {
                FindingsChanged findingsChanged = _findingsChangedNotifier.Process(messageId, "DMARC",
                    ExtractFindingsFromState(messageId, state),
                    ExtractFindingsFromResult(messageId, evaluationResult));
                _dispatcher.Dispatch(findingsChanged, _dmarcEntityConfig.SnsTopicArn);
            }
        }

        private IList<Finding> ExtractFindingsFromState(string domain, DmarcEntityState state)
        {
            var rootMessages = state.Messages?.Where(_ => _.MessageDisplay != MessageDisplay.Prompt).ToList() ?? new List<ErrorMessage>();
            var recordsMessages = state.DmarcRecords?.Messages?.Where(_ => _.MessageDisplay != MessageDisplay.Prompt).ToList() ?? new List<ErrorMessage>();
            var recordsRecords = state.DmarcRecords?.Records;

            return ExtractFindingsFromMessages(domain, rootMessages, recordsMessages, recordsRecords);
        }

        private IList<Finding> ExtractFindingsFromResult(string domain, DmarcRecordsEvaluated result)
        {
            var rootMessages = result.Messages?.Where(_ => _.MessageDisplay != MessageDisplay.Prompt).ToList() ?? new List<ErrorMessage>();
            var recordsMessages = result.Records?.Messages.Where(_ => _.MessageDisplay != MessageDisplay.Prompt).ToList() ?? new List<ErrorMessage>();
            var recordsRecords = result.Records?.Records;

            return ExtractFindingsFromMessages(domain, rootMessages, recordsMessages, recordsRecords);
        }

        private List<Finding> ExtractFindingsFromMessages(string domain, List<ErrorMessage> rootMessages, List<ErrorMessage> recordsMessages, List<DmarcRecord> recordsRecords)
        {
            rootMessages.AddRange(recordsMessages);

            if (recordsRecords != null)
            {
                rootMessages.AddRange(recordsRecords.SelectMany(x => x.Messages)
                    .Where(y => y.MessageDisplay != MessageDisplay.Prompt).ToList());
            }

            List<Finding> findings = rootMessages.Select(msg => new Finding
            {
                Name = msg.Name,
                SourceUrl = $"https://{_dmarcEntityConfig.WebUrl}/app/domain-security/{domain}/dmarc",
                Title = msg.Text,
                EntityUri = $"domain:{domain}",
                Severity = AdvisoryMessageTypeToFindingSeverityMapping[msg.MessageType]
            }).ToList();

            return findings;
        }

        internal static readonly Dictionary<MessageType, string> AdvisoryMessageTypeToFindingSeverityMapping = new Dictionary<MessageType, string>
        {
            [MessageType.info] = "Informational",
            [MessageType.warning] = "Advisory",
            [MessageType.error] = "Urgent",
            [MessageType.positive] = "Positive",
        };
    }
}