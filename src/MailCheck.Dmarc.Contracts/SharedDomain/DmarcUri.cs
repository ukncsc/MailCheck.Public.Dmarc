using System;

namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class DmarcUri
    {
        public DmarcUri(Uri uri, bool valid)
        {
            Uri = uri;
            Valid = valid;
        }

        public Uri Uri { get; }
        public bool Valid { get; }
    }
}