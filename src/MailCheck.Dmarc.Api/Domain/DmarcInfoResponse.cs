using System;
using System.Collections.Generic;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Api.Domain
{
    public class DmarcInfoResponse
    {
        public DmarcInfoResponse(string id, DmarcState dmarcState, DmarcRecords dmarcRecords = null, List<Message> messages = null, DateTime? lastUpdated = null)
        {
            Id = id;
            Status = dmarcState;
            DmarcRecords = dmarcRecords;
            Messages = messages;
            LastUpdated = lastUpdated;
        }

        public string Id { get; }

        public DmarcState Status { get; }

        public DmarcRecords DmarcRecords { get; }

        public List<Message> Messages { get; }

        public DateTime? LastUpdated { get; }
    }
}
