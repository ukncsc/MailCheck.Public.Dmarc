using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dmarc.Entity.Entity.Notifications
{
    public class DmarcAdvisoryRemoved : Message
    {
        public DmarcAdvisoryRemoved(string id, List<AdvisoryMessage> messages) : base(id)
        {
            Messages = messages;
        }

        public List<AdvisoryMessage> Messages { get; }
    }
}
