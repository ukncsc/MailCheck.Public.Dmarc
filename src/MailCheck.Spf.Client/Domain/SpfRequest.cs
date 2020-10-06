using System.Collections.Generic;

namespace MailCheck.Spf.Client.Domain
{
    public class SpfRequest
    {
        public SpfRequest(List<string> hostNames)
        {
            HostNames = hostNames;
        }

        public List<string> HostNames { get; }
    }
}