using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Entity.Config;
using MailCheck.DomainStatus.Contracts;
using Microsoft.Extensions.Logging;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;

namespace MailCheck.Dmarc.Entity.Entity.DomainStatus
{
    public interface IDomainStatusPublisher
    {
        void Publish(DmarcRecordsEvaluated message);
    }

    public class DomainStatusPublisher : IDomainStatusPublisher
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDmarcEntityConfig _dmarcEntityConfig;
        private readonly IDomainStatusEvaluator _domainStatusEvaluator;
        private readonly ILogger<DomainStatusPublisher> _log;

        public DomainStatusPublisher(IMessageDispatcher dispatcher, IDmarcEntityConfig dmarcEntityConfig, IDomainStatusEvaluator domainStatusEvaluator, ILogger<DomainStatusPublisher> log)
        {
            _dispatcher = dispatcher;
            _dmarcEntityConfig = dmarcEntityConfig;
            _domainStatusEvaluator = domainStatusEvaluator;
            _log = log;
        }

        public void Publish(DmarcRecordsEvaluated message)
        {
            List<Message> rootMessages = message.Messages;
            List<Message> recordsMessages = message.Records?.Messages;
            IEnumerable<Message> recordsRecordsMessages = message.Records?.Records?.SelectMany(x => x.Messages);

            IEnumerable<Message> messages = (rootMessages ?? new List<Message>())
                .Concat(recordsMessages ?? new List<Message>())
                .Concat(recordsRecordsMessages ?? new List<Message>());

            Status status = _domainStatusEvaluator.GetStatus(messages.ToList());

            DomainStatusEvaluation domainStatusEvaluation = new DomainStatusEvaluation(message.Id, "DMARC", status);

            _log.LogInformation($"Publishing DMARC domain status for {message.Id} of {status}.");

            _dispatcher.Dispatch(domainStatusEvaluation, _dmarcEntityConfig.SnsTopicArn);
        }
    }
}
