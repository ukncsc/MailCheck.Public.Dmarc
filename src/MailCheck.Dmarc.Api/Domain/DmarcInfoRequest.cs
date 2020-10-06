using System.Collections.Generic;

namespace MailCheck.Dmarc.Api.Domain
{
    public class DmarcInfoListRequest
    {
        public DmarcInfoListRequest()
        {
            HostNames = new List<string>();
        }

        public List<string> HostNames { get; set; }
    }
}
