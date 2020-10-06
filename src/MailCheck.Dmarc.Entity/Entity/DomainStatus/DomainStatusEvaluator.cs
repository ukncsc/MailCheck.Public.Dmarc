﻿using System.Collections.Generic;
using System.Linq;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.DomainStatus.Contracts;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;

namespace MailCheck.Dmarc.Entity.Entity.DomainStatus
{
    public interface IDomainStatusEvaluator
    {
        Status GetStatus(List<Message> messages);
    }

    public class DomainStatusEvaluator : IDomainStatusEvaluator
    {
        public Status GetStatus(List<Message> messages)
        {
            if (messages is null)
            {
                return Status.Success;
            }

            IEnumerable<MessageType> statuses = messages.Select(x => x.MessageType).ToList();

            Status status = Status.Success;

            if (statuses.Any(x => x == MessageType.error))
            {
                status = Status.Error;
            }
            else if (statuses.Any(x => x == MessageType.warning))
            {
                status = Status.Warning;
            }
            else if (statuses.Any(x => x == MessageType.info))
            {
                status = Status.Info;
            }

            return status;
        }
    }
}
