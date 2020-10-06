using System;
using System.Collections.Generic;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using Evnt = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.Dmarc.Entity.Entity
{
    public class DmarcEntityState 
    {
        public DmarcEntityState(string id, int version, DmarcState dmarcState, DateTime created) 
        {
            Id = id;
            Version = version;
            DmarcState = dmarcState;
            Created = created;
            Messages = new List<Message>();
        }

        public virtual string Id { get; }

        public virtual int Version { get; set; }

        public virtual DmarcState DmarcState { get; set; }

        public virtual DateTime Created { get; }

        public virtual DmarcRecords DmarcRecords { get; set; }

        public virtual TimeSpan? ElapsedQueryTime { get; set; }

        public virtual List<Message> Messages { get; set; }

        public virtual DateTime? LastUpdated { get; set; }
        
        public Evnt UpdatePollPending()
        {
            DmarcState = DmarcState.PollPending;

            return new DmarcPollPending(Id);
        }
        
        public Evnt UpdateDmarcEvaluation(DmarcRecords dmarcRecords, TimeSpan? elapsedQueryTime, List<Message> messages, DateTime lastUpdated)
        {
            DmarcRecords = dmarcRecords;
            ElapsedQueryTime = elapsedQueryTime;
            LastUpdated = lastUpdated;
            Messages = messages;
            LastUpdated = lastUpdated;
            DmarcState = DmarcState.Evaluated;

            return new DmarcRecordEvaluationsChanged(Id, dmarcRecords);
        }
    }
}
