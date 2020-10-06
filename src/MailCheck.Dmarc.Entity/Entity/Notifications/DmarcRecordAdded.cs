using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dmarc.Entity.Entity.Notifications
{
    public class DmarcRecordAdded : Message
    {
        public DmarcRecordAdded(string id, List<string> records) : base(id)
        {
            Records = records;
        }

        public List<string> Records { get; }
    }
}