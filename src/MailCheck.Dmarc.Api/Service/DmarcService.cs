using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Api.Domain;
using MailCheck.Dmarc.Api.Config;
using MailCheck.Dmarc.Api.Dao;

namespace MailCheck.Dmarc.Api.Service
{
    public interface IDmarcService
    {
        Task<DmarcInfoResponse> GetDmarcForDomain(string requestDomain);
    }

    public class DmarcService : IDmarcService
    {
        private readonly IDmarcApiDao _dao;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IDmarcApiConfig _config;

        public DmarcService(IMessagePublisher messagePublisher, IDmarcApiDao dao, IDmarcApiConfig config)
        {
            _messagePublisher = messagePublisher;
            _dao = dao;
            _config = config;
        }

        public async Task<DmarcInfoResponse> GetDmarcForDomain(string requestDomain)
        {
            DmarcInfoResponse response = await _dao.GetDmarcForDomain(requestDomain);

            if (response is null)
            {
                await _messagePublisher.Publish(new DomainMissing(requestDomain), _config.MicroserviceOutputSnsTopicArn);
            }

            return response;
        }
    }
}
