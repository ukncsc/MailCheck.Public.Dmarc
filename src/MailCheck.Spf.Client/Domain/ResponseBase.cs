using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MailCheck.Spf.Client.Domain
{
    public class ResponseBase
    {
        public ResponseBase(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
