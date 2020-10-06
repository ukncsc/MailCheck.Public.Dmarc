using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Entity.Config;
using MailCheck.Dmarc.Entity.Entity.Notifications;
using Message = MailCheck.Common.Messaging.Abstractions.Message;
using MessageDisplay = MailCheck.Dmarc.Entity.Entity.Notifications.MessageDisplay;
using MessageType = MailCheck.Dmarc.Entity.Entity.Notifications.MessageType;

namespace MailCheck.Dmarc.Entity.Entity.Notifiers
{
    public class AdvisoryChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDmarcEntityConfig _dmarcEntityConfig;
        private readonly IEqualityComparer<Contracts.SharedDomain.Message> _messageEqualityComparer;

        public AdvisoryChangedNotifier(IMessageDispatcher dispatcher, IDmarcEntityConfig dmarcEntityConfig, IEqualityComparer<Contracts.SharedDomain.Message> messageEqualityComparer)
        {
            _dispatcher = dispatcher;
            _dmarcEntityConfig = dmarcEntityConfig;
            _messageEqualityComparer = messageEqualityComparer;
        }

        public void Handle(DmarcEntityState state, Message message)
        {
            if (message is DmarcRecordsEvaluated evaluationResult)
            {
                List<Contracts.SharedDomain.Message> currentMessages = state.Messages.Where(_ => _.MessageDisplay != Contracts.SharedDomain.MessageDisplay.Prompt).ToList();
                List<DmarcRecord> currentRecords = state.DmarcRecords?.Records;
                if (currentRecords != null)
                {
                    currentMessages.AddRange(currentRecords.SelectMany(x => x.Messages)
                        .Where(y => y.MessageDisplay != Contracts.SharedDomain.MessageDisplay.Prompt).ToList());
                }

                List<Contracts.SharedDomain.Message> newMessages = evaluationResult.Messages.Where(_ => _.MessageDisplay != Contracts.SharedDomain.MessageDisplay.Prompt).ToList();
                List<DmarcRecord> newRecords = evaluationResult.Records?.Records;
                if (newRecords != null)
                {
                    newMessages.AddRange(newRecords.SelectMany(x => x.Messages)
                        .Where(y => y.MessageDisplay != Contracts.SharedDomain.MessageDisplay.Prompt).ToList());
                }

                List<Contracts.SharedDomain.Message> addedMessages = newMessages.Except(currentMessages, _messageEqualityComparer).ToList();
                if (addedMessages.Any())
                {
                    DmarcAdvisoryAdded dmarcAdvisoryAdded = new DmarcAdvisoryAdded(state.Id, addedMessages.Select(x => new AdvisoryMessage((MessageType) x.MessageType, x.Text, (MessageDisplay) x.MessageDisplay)).ToList());
                    _dispatcher.Dispatch(dmarcAdvisoryAdded, _dmarcEntityConfig.SnsTopicArn);
                }

                List<Contracts.SharedDomain.Message> removedMessages = currentMessages.Except(newMessages, _messageEqualityComparer).ToList();
                if (removedMessages.Any())
                {
                    DmarcAdvisoryRemoved dmarcAdvisoryRemoved = new DmarcAdvisoryRemoved(state.Id, removedMessages.Select(x => new AdvisoryMessage((MessageType) x.MessageType, x.Text, (MessageDisplay) x.MessageDisplay)) .ToList());
                    _dispatcher.Dispatch(dmarcAdvisoryRemoved, _dmarcEntityConfig.SnsTopicArn);
                }
            }
        }
    }
}