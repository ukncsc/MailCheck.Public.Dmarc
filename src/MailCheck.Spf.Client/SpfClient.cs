using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using MailCheck.Common.Api.AuthProviders;
using MailCheck.Common.Api.Utils;
using MailCheck.Spf.Client.Config;
using MailCheck.Spf.Client.Domain;
using Microsoft.Extensions.Logging;

namespace MailCheck.Spf.Client
{
    public interface ISpfClient
    {
        Task<SpfHistoryResponse> GetHistory(string domain);
        Task<SpfResponse> GetSpf(string domain);
        Task<SpfListResponse> GetSpf(List<string> domains);
    }

    internal class SpfClient : ISpfClient
    {
        private const string History = "history";
        private const string Domains = "domains";

        private readonly ISpfClientConfig _config;
        private readonly IAuthenticationHeaderProvider _authenticationHeaderProvider;
        private readonly ILogger<SpfClient> _log;

        public SpfClient(ISpfClientConfig config,
            IAuthenticationHeaderProvider authenticationHeaderProvider,
            ILogger<SpfClient> log)
        {
            _config = config;
            _authenticationHeaderProvider = authenticationHeaderProvider;
            _log = log;
        }

        public async Task<SpfHistoryResponse> GetHistory(string domain)
        {
            try
            {
                Dictionary<string, string> headers = await _authenticationHeaderProvider.GetAuthenticationHeaders();

                HttpResponseMessage httpResponseMessage = await _config.SpfApiEndpoint
                    .AllowAnyHttpStatus()
                    .AppendPathSegments(History, domain)
                    .WithHeaders(headers)
                    .GetAsync();

                SpfHistory history = null;

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    history = await httpResponseMessage.ReceiveJson<SpfHistory>();
                }

                return new SpfHistoryResponse(httpResponseMessage.StatusCode, history);
            }
            catch (Exception e)
            {
                _log.LogError($"The error {e.Message} occured retrieving history for {domain} from {_config.SpfApiEndpoint}");
            }

            return new SpfHistoryResponse(HttpStatusCode.BadRequest, null);
        }

        public async Task<SpfResponse> GetSpf(string domain)
        {
            try
            {
                Dictionary<string, string> headers = await _authenticationHeaderProvider.GetAuthenticationHeaders();

                HttpResponseMessage httpResponseMessage = await _config.SpfApiEndpoint
                    .AllowAnyHttpStatus()
                    .AppendPathSegments(domain)
                    .WithHeaders(headers)
                    .GetAsync();

                Domain.Spf spf = null;

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    spf = await httpResponseMessage.ReceiveJson<Domain.Spf>();
                }

                return new SpfResponse(httpResponseMessage.StatusCode, spf);
            }
            catch (Exception e)
            {
                _log.LogError($"The error {e.Message} occured retrieving spf for {domain} from {_config.SpfApiEndpoint}");
            }

            return new SpfResponse(HttpStatusCode.BadRequest, null);
        }

        public async Task<SpfListResponse> GetSpf(List<string> domains)
        {
            try
            {
                Dictionary<string, string> headers = await _authenticationHeaderProvider.GetAuthenticationHeaders();

                HttpResponseMessage httpResponseMessage = await _config.SpfApiEndpoint
                    .AllowAnyHttpStatus()
                    .AppendPathSegments(Domains)
                    .WithHeaders(headers)
                    .PostJsonAsync(new SpfRequest(domains));

                List<Domain.Spf> spfs = null;

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    spfs = await httpResponseMessage.ReceiveJson<List<Domain.Spf>>();
                }

                return new SpfListResponse(httpResponseMessage.StatusCode, spfs);
            }
            catch (Exception e)
            {
                _log.LogError($"The error {e.Message} occured retrieving spf for {string.Join(", ", domains)} from {_config.SpfApiEndpoint}");
            }

            return new SpfListResponse(HttpStatusCode.BadRequest, null);
        }
    }
}
