using System.Collections.Generic;

namespace MailCheck.Dmarc.Poller.Domain
{
    public class ReportUriForensic : ReportUri
    {
        public ReportUriForensic(string value, List<UriTag> uris)
            : base(value, uris) { }
    }
}