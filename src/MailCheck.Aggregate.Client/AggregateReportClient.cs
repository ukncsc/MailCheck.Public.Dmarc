using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using MailCheck.AggregateReport.Client.Config;
using MailCheck.AggregateReport.Client.Domain;
using MailCheck.Common.Api.AuthProviders;
using MailCheck.Common.Api.Utils;
using Microsoft.Extensions.Logging;

namespace MailCheck.AggregateReport.Client
{
    public interface IAggregateReportClient
    {
        Task<AggregateReportSummaryResponse> GetAggregateReportSummary(string domainName, DateTime startDate,
            DateTime endDate);
    }

    internal class AggregateReportClient : IAggregateReportClient
    {
        private const string Summary = "summary";

        private readonly IAggregateReportClientConfig _config;
        private readonly IAuthenticationHeaderProvider _authenticationHeaderProvider;
        private readonly ILogger<AggregateReportClient> _log;

        public AggregateReportClient(IAggregateReportClientConfig config,
            IAuthenticationHeaderProvider authenticationHeaderProvider,
            ILogger<AggregateReportClient> log)
        {
            _config = config;
            _authenticationHeaderProvider = authenticationHeaderProvider;
            _log = log;
        }

        public async Task<AggregateReportSummaryResponse> GetAggregateReportSummary(string domainName, DateTime startDate, DateTime endDate)
        {
            try
            {
                Dictionary<string, string> headers = await _authenticationHeaderProvider.GetAuthenticationHeaders();

                HttpResponseMessage httpResponseMessage = await _config.AggregateReportApiEndpoint
                    .AllowAnyHttpStatus()
                    .AppendPathSegments(Summary, Uri.EscapeDataString(domainName), startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"))
                    .WithHeaders(headers)
                    .GetAsync();

                AggregateReportSummary summary = null;

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    summary = await httpResponseMessage.ReceiveJson<AggregateReportSummary>();
                }

                return new AggregateReportSummaryResponse(httpResponseMessage.StatusCode, summary);
            }
            catch (Exception e)
            {
                _log.LogError($"The error {e.Message} occured retrieving aggregate report summary for {domainName} between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd} from {_config.AggregateReportApiEndpoint}");
            }

            return new AggregateReportSummaryResponse(HttpStatusCode.BadRequest, null);
        }
    }
}
