using System.Collections.Generic;
using System.Net;

namespace MailCheck.Spf.Client.Domain
{
    public class SpfListResponse : ResponseBase
    {
        public SpfListResponse(HttpStatusCode statusCode, List<Spf> spfs) 
            : base(statusCode)
        {
            Spfs = spfs;
        }

        public List<Spf> Spfs { get; }
    }
}