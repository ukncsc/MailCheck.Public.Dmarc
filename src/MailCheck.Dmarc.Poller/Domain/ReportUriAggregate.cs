using System.Collections.Generic;

namespace MailCheck.Dmarc.Poller.Domain
{
    public class ReportUriAggregate : ReportUri
    {
        public ReportUriAggregate(string value, List<UriTag> uris) 
            : base(value, uris){}
    }
}