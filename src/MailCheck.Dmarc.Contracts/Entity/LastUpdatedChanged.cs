using System;
using MailCheck.Dmarc.Contracts.Entity;

namespace MailCheck.Dmarc.Contracts.Entity
{
    public class LastUpdatedChanged : Common.Messaging.Abstractions.Message
    {
        public LastUpdatedChanged(string id, DateTime lastUpdated) 
            : base(id)
        {
            LastUpdated = lastUpdated;
        }

        public DateTime LastUpdated { get; }

        public DmarcState State => DmarcState.Unchanged;
    }
}