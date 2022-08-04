using System;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Record
{
    public class RuaTagShouldNotHaveMoreThanTwoUris : TagShouldNotHaveMoreThanTwoUris<ReportUriAggregate>
    {
        public RuaTagShouldNotHaveMoreThanTwoUris()
            : base(DmarcRulesResource.RuaTagShouldNotHaveMoreThanTwoUrisErrorMessage, DmarcRulesMarkdownResource.RuaTagShouldNotHaveMoreThanTwoUrisErrorMessage)
        {
        }

        public override Guid Id => Guid.Parse("E87EA025-0BB9-4B79-AE15-846A8E73A77B");
        public override string Name => "mailcheck.dmarc.ruaTagShouldNotHaveMoreThanTwoUris";
    }
}
