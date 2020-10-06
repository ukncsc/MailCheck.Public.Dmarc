using System.Net;

namespace MailCheck.Spf.Client.Domain
{
    public class SpfResponse : ResponseBase
    {
        public SpfResponse(HttpStatusCode statusCode, Spf spf) 
            : base(statusCode)
        {
            Spf = spf;
        }

        public Spf Spf { get; }
    }
}