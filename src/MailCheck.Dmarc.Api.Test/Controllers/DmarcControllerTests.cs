using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Api.Authorisation.Service;
using MailCheck.Common.Api.Authorisation.Service.Domain;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Api.Config;
using MailCheck.Dmarc.Api.Controllers;
using MailCheck.Dmarc.Api.Dao;
using MailCheck.Dmarc.Api.Domain;
using MailCheck.Dmarc.Api.Service;
using MailCheck.Dmarc.Contracts.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Dmarc.Api.Test.Controllers
{
    [TestFixture]
    public class DmarcControllerTests
    {
        private DmarcController _sut;
        private IDmarcService _dmarcService;
        private IMailCheckAuthorisationService _authorisationService;
        private IMessagePublisher _messagePublisher;
        private IDmarcApiDao _dao;
        private IDmarcApiConfig _config;

        [SetUp]
        public void SetUp()
        {
            _dmarcService = A.Fake<IDmarcService>();
            _authorisationService = A.Fake<IMailCheckAuthorisationService>();
            _messagePublisher = A.Fake<IMessagePublisher>();
            _dao = A.Fake<IDmarcApiDao>();
            _config = A.Fake<IDmarcApiConfig>();


            _sut = new DmarcController(_dao, _authorisationService, _messagePublisher, _config, _dmarcService, A.Fake<ILogger<DmarcController>>());
        }

        [Test]
        public async Task ItShouldReturnNotFoundWhenThereIsNoDmarcState()
        {
            A.CallTo(() => _authorisationService.IsAuthorised(A<Role>._))
                .Returns(true);

            A.CallTo(() => _dmarcService.GetDmarcForDomain(A<string>._))
                .Returns(Task.FromResult<DmarcInfoResponse>(null));

            IActionResult response = await _sut.GetDmarc(new DmarcDomainRequest {Domain = "ncsc.gov.uk"});

            Assert.That(response, Is.TypeOf(typeof(NotFoundObjectResult)));
        }

        [Test]
        public async Task ItShouldReturnTheFirstResultWhenTheDmarcStateExists()
        {
            A.CallTo(() => _authorisationService.IsAuthorised(A<Role>._))
                .Returns(true);

            DmarcInfoResponse state = new DmarcInfoResponse("ncsc.gov.uk", DmarcState.Created);

            A.CallTo(() => _dmarcService.GetDmarcForDomain(A<string>._))
                .Returns(Task.FromResult(state));

            ObjectResult response = (ObjectResult)await _sut.GetDmarc(new DmarcDomainRequest { Domain = "ncsc.gov.uk" });

            Assert.AreSame(response.Value, state);
        }

        [Test]
        public async Task RecheckDmarcForRecordOlderThan5MinutesShouldPublishRecheck()
        {
            A.CallTo(() => _authorisationService.IsAuthorised(A<Role>._))
                .Returns(true);

            DmarcInfoResponse state = new DmarcInfoResponse("ncsc.gov.uk", DmarcState.Created);

            A.CallTo(() => _dao.GetDmarcForDomain(A<string>._))
                .Returns(Task.FromResult(state));

            ObjectResult response = (ObjectResult)await _sut.RecheckDmarc(new DmarcDomainRequest { Domain = "ncsc.gov.uk" });

            A.CallTo(() => _messagePublisher.Publish(A<Message>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task RecheckDmarcForRecordLessThan5MinutesShouldNotPublishRecheck()
        {
            A.CallTo(() => _config.RecheckMinPeriodInSeconds).Returns(300);

            A.CallTo(() => _authorisationService.IsAuthorised(A<Role>._))
                .Returns(true);

            DmarcInfoResponse state = new DmarcInfoResponse("ncsc.gov.uk", DmarcState.Created, null, null, DateTime.Now);

            A.CallTo(() => _dao.GetDmarcForDomain(A<string>._))
                .Returns(Task.FromResult(state));

            ObjectResult response = (ObjectResult)await _sut.RecheckDmarc(new DmarcDomainRequest { Domain = "ncsc.gov.uk" });

            A.CallTo(() => _messagePublisher.Publish(A<Message>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void AllEndpointsHaveAuthorisation()
        {
            IEnumerable<MethodInfo> controllerMethods = _sut.GetType().GetMethods().Where(x => x.DeclaringType == typeof(DmarcController));

            foreach (MethodInfo methodInfo in controllerMethods)
            {
                Assert.That(methodInfo.CustomAttributes.Any(x => x.AttributeType == typeof(MailCheckAuthoriseResourceAttribute) || x.AttributeType == typeof(MailCheckAuthoriseRoleAttribute)));
            }
        }
    }
}
