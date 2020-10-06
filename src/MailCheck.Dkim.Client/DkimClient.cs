using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using MailCheck.Common.Api.AuthProviders;
using MailCheck.Common.Api.Utils;
using MailCheck.Dkim.Client.Config;
using MailCheck.Dkim.Client.Domain;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dkim.Client
{
    public interface IDkimClient
    {
        Task<DkimHistoryResponse> GetHistory(string domain);

        //Calls to GetDkim would also be exposed here.
    }

    internal class DkimClient : IDkimClient
    {
        private const string History = "history";

        private readonly IDkimClientConfig _config;
        private readonly IAuthenticationHeaderProvider _authenticationHeaderProvider;
        private readonly ILogger<DkimClient> _log;

        public DkimClient(IDkimClientConfig config,
            IAuthenticationHeaderProvider authenticationHeaderProvider,
            ILogger<DkimClient> log)
        {
            _config = config;
            _authenticationHeaderProvider = authenticationHeaderProvider;
            _log = log;
        }

        public async Task<DkimHistoryResponse> GetHistory(string domain)
        {
            try
            {
                Dictionary<string, string> headers = await _authenticationHeaderProvider.GetAuthenticationHeaders();

                HttpResponseMessage httpResponseMessage = await _config.DkimApiEndpoint
                    .AllowAnyHttpStatus()
                    .AppendPathSegments(History, domain)
                    .WithHeaders(headers)
                    .GetAsync();

                DkimHistory history = null;

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    history = await httpResponseMessage.ReceiveJson<DkimHistory>();
                }

                return new DkimHistoryResponse(httpResponseMessage.StatusCode, history);
            }
            catch (Exception e)
            {
                _log.LogError($"The error {e.Message} occured retrieving history for {domain} from {_config.DkimApiEndpoint}");
            }

            return new DkimHistoryResponse(HttpStatusCode.BadRequest, null);
        }
    }
}
