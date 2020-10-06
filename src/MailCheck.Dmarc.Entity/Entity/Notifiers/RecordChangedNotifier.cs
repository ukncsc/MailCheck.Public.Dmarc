using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Dmarc.Entity.Config;
using MailCheck.Dmarc.Entity.Entity.Notifications;

namespace MailCheck.Dmarc.Entity.Entity.Notifiers
{
    public class RecordChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDmarcEntityConfig _dmarcEntityConfig;

        public RecordChangedNotifier(IMessageDispatcher dispatcher, IDmarcEntityConfig dmarcEntityConfig)
        {
            _dispatcher = dispatcher;
            _dmarcEntityConfig = dmarcEntityConfig;
        }

        public void Handle(DmarcEntityState state, Message message)
        {
            if (message is DmarcRecordsEvaluated dmarcRecordsEvaluated)
            {
                List<string> currentRecords = state.DmarcRecords?.Records.Select(x => x.Record?.Replace("; ", ";")).ToList() ?? new List<string>();
                List<string> newRecords = dmarcRecordsEvaluated.Records?.Records.Select(x => x.Record?.Replace("; ", ";")).ToList() ?? new List<string>();

                List<string> addedRecords = newRecords.Except(currentRecords).ToList();
                if (addedRecords.Any())
                {
                    DmarcRecordAdded dmarcAdvisoryAdded = new DmarcRecordAdded(state.Id, addedRecords);
                    _dispatcher.Dispatch(dmarcAdvisoryAdded, _dmarcEntityConfig.SnsTopicArn);
                }

                List<string> removedRecords = currentRecords.Except(newRecords).ToList();
                if (removedRecords.Any())
                {
                    DmarcRecordRemoved dmarcAdvisoryRemoved = new DmarcRecordRemoved(state.Id, removedRecords);
                    _dispatcher.Dispatch(dmarcAdvisoryRemoved, _dmarcEntityConfig.SnsTopicArn);
                }
            }
        }
    }
}