using System.Net;

namespace MailCheck.Spf.Client.Domain
{
    public class SpfHistoryResponse : ResponseBase
    {
        public SpfHistoryResponse(HttpStatusCode statusCode, SpfHistory spfHistory) 
            : base(statusCode)
        {
            SpfHistory = spfHistory;
        }

        public SpfHistory SpfHistory { get; }
    }
}