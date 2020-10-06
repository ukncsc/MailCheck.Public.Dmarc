using System;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Record
{
    public class RufTagShouldNotHaveMoreThanTwoUris : TagShouldNotHaveMoreThanTwoUris<ReportUriForensic>
    {
        public RufTagShouldNotHaveMoreThanTwoUris()
            : base(DmarcRulesResource.RufTagShouldNotHaveMoreThanTwoUrisErrorMessage, DmarcRulesMarkdownResource.RufTagShouldNotHaveMoreThanTwoUrisErrorMessage)
        {
        }

        public override Guid Id => Guid.Parse("31981DD9-5C94-4F4B-985D-1E5EABD4AA3A");
    }
}
