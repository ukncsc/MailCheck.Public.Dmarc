using System.Net;

namespace MailCheck.Dkim.Client.Domain
{
    public class DkimHistoryResponse
    {
        public DkimHistoryResponse(HttpStatusCode statusCode, DkimHistory history)
        {
            StatusCode = statusCode;
            History = history;
        }

        public HttpStatusCode StatusCode { get; }
        public DkimHistory History { get; }
    }
}