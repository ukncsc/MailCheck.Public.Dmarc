using System.Collections.Generic;

namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class ReportUriAggregate : ReportUri
    {
        public ReportUriAggregate(string value, List<UriTag> uris, bool valid) 
            : base(TagType.ReportUriAggregate, value, uris, valid) {}
    }
}