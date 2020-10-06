using System;
using System.Collections.Generic;
using MailCheck.Dmarc.Contracts.SharedDomain;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.Dmarc.Contracts.Evaluator
{
    public class DmarcRecordsEvaluated : Message
    {
        public DmarcRecordsEvaluated(string id, DmarcRecords records, TimeSpan? elapsedQueryTime, List<SharedDomain.Message> messages, DateTime lastUpdated) : base(id)
        {
            Records = records;
            ElapsedQueryTime = elapsedQueryTime;
            Messages = messages;
            LastUpdated = lastUpdated;
        }

        public DmarcRecords Records { get; }

        public TimeSpan? ElapsedQueryTime { get; }

        public List<SharedDomain.Message> Messages { get; }

        public DateTime LastUpdated { get; }
    }
}
