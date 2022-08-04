using System;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public interface IDmarcUriParser
    {
        DmarcUri Parse(string value);
    }

    public class DmarcUriParser : IDmarcUriParser
    {
        public Guid Id => Guid.Parse("6D898C68-4152-4292-A4DB-59F6E2185CD8");

        public DmarcUri Parse(string value)
        {
            Uri uri = value != null && Uri.IsWellFormedUriString(value, UriKind.Absolute)
                ? new Uri(value, UriKind.Absolute)
                : null;

            DmarcUri dmarcUri = new DmarcUri(uri);

            if (uri == null)
            {
                string errorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, "uri", value);
                string markdown = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, "uri", value);

                dmarcUri.AddError(new Error(Id, "mailcheck.dmarc.invalidUriValue", ErrorType.Error, errorMessage, markdown));
            }

            return dmarcUri;
        }
    }
}