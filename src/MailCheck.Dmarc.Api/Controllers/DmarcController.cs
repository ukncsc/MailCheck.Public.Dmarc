using System;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Api.Authorisation.Service;
using MailCheck.Common.Api.Authorisation.Service.Domain;
using MailCheck.Common.Api.Domain;
using MailCheck.Common.Api.Utils;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Api.Config;
using MailCheck.Dmarc.Api.Dao;
using MailCheck.Dmarc.Api.Domain;
using MailCheck.Dmarc.Api.Service;
using MailCheck.Dmarc.Contracts.Scheduler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dmarc.Api.Controllers
{
    [Route("/api/dmarc")]
    public class DmarcController : Controller
    {
        private readonly IDmarcApiDao _dao;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IDmarcApiConfig _config;
        private readonly IDmarcService _dmarcService;

        public DmarcController(IDmarcApiDao dao,
            IMailCheckAuthorisationService authorisationService,
            IMessagePublisher messagePublisher,
            IDmarcApiConfig config,
            IDmarcService dmarcService,
            ILogger<DmarcController> log)
        {
            _dao = dao;
            _messagePublisher = messagePublisher;
            _config = config;
            _dmarcService = dmarcService;
        }

        [HttpGet("{domain}/recheck")]
        [MailCheckAuthoriseRole(Role.Standard)]
        public async Task<IActionResult> RecheckDmarc(DmarcDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            DmarcInfoResponse infoResponse = await _dao.GetDmarcForDomain(request.Domain);
            
            if (!infoResponse.LastUpdated.HasValue ||
                DateTime.UtcNow > infoResponse.LastUpdated.Value.AddSeconds(_config.RecheckMinPeriodInSeconds))
            {
                await _messagePublisher.Publish(new DmarcRecordExpired(request.Domain), _config.SnsTopicArn);
            }

            return new OkObjectResult("{}");
        }

        [HttpGet("{domain}")]
        [MailCheckAuthoriseRole(Role.Standard)]
        public async Task<IActionResult> GetDmarc(DmarcDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            DmarcInfoResponse response = await _dmarcService.GetDmarcForDomain(request.Domain);

            if (response == null)
            {
                return new NotFoundObjectResult(new ErrorResponse($"No DMARC found for {request.Domain}",
                    ErrorStatus.Information));
            }

            return new ObjectResult(response);
        }

        [HttpPost]
        [Route("domains")]
        [MailCheckAuthoriseRole(Role.Standard)]
        public async Task<IActionResult> GetDmarcStates([FromBody] DmarcInfoListRequest request)
        {
            return new ObjectResult(await _dao.GetDmarcForDomains(request.HostNames));
        }

        [HttpGet]
        [Route("history/{domain}")]
        [MailCheckAuthoriseResource(Operation.Read, ResourceType.DmarcHistory, "domain")]
        public async Task<IActionResult> GetDmarcHistory(DmarcDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            string history = await _dao.GetDmarcHistory(request.Domain);

            if (history == null)
            {
                return new NotFoundObjectResult(new ErrorResponse($"No DMARC History found for {request.Domain}",
                    ErrorStatus.Information));
            }

            return Content(history, "application/json");
        }
    }
}