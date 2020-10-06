using System;

namespace MailCheck.Dmarc.Poller.Domain
{
    public class DmarcUri : DmarcEntity
    {
        public DmarcUri(Uri uri)
        {
            Uri = uri;
        }

        public Uri Uri { get; }

        public override string ToString()
        {
            return $"{nameof(Uri)}: {Uri}";
        }
    }
}