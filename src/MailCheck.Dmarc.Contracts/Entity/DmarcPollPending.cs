
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dmarc.Contracts.Entity
{
    public class DmarcPollPending : Message
    {
        public DmarcPollPending(string id) 
            : base(id){}

        public DmarcState State => DmarcState.PollPending;
    }
}