using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Contracts.Entity
{
    public class DmarcRecordEvaluationsChanged : Common.Messaging.Abstractions.Message
    {
        public DmarcRecordEvaluationsChanged(string id, DmarcRecords records) : base(id)
        {
            Records = records;
        }

        public DmarcRecords Records { get; }

        public DmarcState State => DmarcState.Evaluated;
    }
}