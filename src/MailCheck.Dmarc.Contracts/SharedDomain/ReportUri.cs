using System;
using System.Collections.Generic;

namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public abstract class ReportUri : Tag
    {
        protected ReportUri(TagType tagType, string value, List<UriTag> uris, bool valid) : base(tagType, value, valid)
        {
            Uris = uris;
        }

        public List<UriTag> Uris { get; }

        public override string ToString()
        {
            string stringUris = string.Join(Environment.NewLine, Uris);
            return $"{base.ToString()}, {nameof(Uris)}:{Environment.NewLine}{stringUris}";
        }
    }
}