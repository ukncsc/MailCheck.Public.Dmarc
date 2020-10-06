using System.Net;

namespace MailCheck.AggregateReport.Client.Domain
{
    public class AggregateReportSummaryResponse
    {
        public AggregateReportSummaryResponse(HttpStatusCode statusCode, AggregateReportSummary aggregateReportSummary)
        {
            StatusCode = statusCode;
            AggregateReportSummary = aggregateReportSummary;
        }

        public HttpStatusCode StatusCode { get; }
        public AggregateReportSummary AggregateReportSummary { get; }
    }
}