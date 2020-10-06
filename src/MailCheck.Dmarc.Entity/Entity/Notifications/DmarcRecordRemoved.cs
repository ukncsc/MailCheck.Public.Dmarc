using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dmarc.Entity.Entity.Notifications
{
    public class DmarcRecordRemoved : Message
    {
        public DmarcRecordRemoved(string id, List<string> records) : base(id)
        {
            Records = records;
        }

        public List<string> Records { get; }
    }
}