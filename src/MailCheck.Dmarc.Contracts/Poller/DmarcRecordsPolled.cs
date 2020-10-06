using System;
using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.SharedDomain;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;

namespace MailCheck.Dmarc.Contracts.Poller
{
    public class DmarcRecordsPolled : Common.Messaging.Abstractions.Message
    {
        public DmarcRecordsPolled(string id,
            DmarcRecords records, 
            TimeSpan? elapsedQueryTime,
            List<Message> messages = null)
            : base(id)
        {
            Records = records;
            ElapsedQueryTime = elapsedQueryTime;
            Messages = messages ?? new List<Message>();
        }

        public DmarcRecords Records { get; }
        public TimeSpan? ElapsedQueryTime { get; }
        public List<Message> Messages { get; }
    }
}