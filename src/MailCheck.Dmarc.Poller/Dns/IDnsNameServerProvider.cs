using System.Collections.Generic;
using System.Net;

namespace MailCheck.Dmarc.Poller.Dns
{
    public interface IDnsNameServerProvider
    {
        List<IPAddress> GetNameServers();
    }
}