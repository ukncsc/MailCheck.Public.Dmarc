using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dmarc.Contracts.Entity
{
    public class DmarcEntityCreated : VersionedMessage
    {
        public DmarcEntityCreated(string id, int version) 
            : base(id, version)
        {
        }

        public DmarcState State => DmarcState.Created;
    }
}
