using System;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Sqs;
using MailCheck.Dmarc.Api.Domain;
using MailCheck.Dmarc.Api.Config;
using MailCheck.Dmarc.Api.Dao;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dmarc.Api.Service
{
    public interface IDmarcService
    {
        Task<DmarcInfoResponse> GetDmarcForDomain(string requestDomain);
        Task<PolicyResponse> GetDmarcPolicyForDomain(string domain);
    }

    public class DmarcService : IDmarcService
    {
        private readonly IDmarcApiDao _dao;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IDmarcApiConfig _config;
        private readonly IPolicyResolver _policyResolver;
        private readonly ILogger<DmarcService> _log;

        public DmarcService(IMessagePublisher messagePublisher, IDmarcApiDao dao, IDmarcApiConfig config, IPolicyResolver policyResolver, ILogger<DmarcService> log)
        {
            _messagePublisher = messagePublisher;
            _dao = dao;
            _config = config;
            _log = log;
            _policyResolver = policyResolver;
        }

        public async Task<DmarcInfoResponse> GetDmarcForDomain(string requestDomain)
        {
            DmarcInfoResponse response = await _dao.GetDmarcForDomain(requestDomain);

            if (response is null)
            {
                _log.LogInformation($"Dmarc entity state does not exist for domain {requestDomain} - publishing DomainMissing");
                await _messagePublisher.Publish(new DomainMissing(requestDomain), _config.MicroserviceOutputSnsTopicArn);
            }

            return response;
        }


        public async Task<PolicyResponse> GetDmarcPolicyForDomain(string domain)
        {
            DmarcInfoResponse response = await _dao.GetDmarcForDomain(domain);
            string policy = _policyResolver.Resolve(response);
            return policy == null ? null : new PolicyResponse { Policy = policy };
        }
    }
}
