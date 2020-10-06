using System.Collections.Generic;

namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class ReportUriForensic : ReportUri
    {
        public ReportUriForensic(string value, List<UriTag> uris, bool valid)
            : base(TagType.ReportUriForensic, value, uris, valid) { }
    }
}