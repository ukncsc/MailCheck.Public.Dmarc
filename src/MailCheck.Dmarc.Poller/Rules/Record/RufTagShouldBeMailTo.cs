using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Record
{
    public class RufTagShouldBeMailTo : TagShouldBeMailTo<ReportUriForensic>
    {
        public RufTagShouldBeMailTo()
            : base(DmarcRulesResource.RufTagShouldBeMailToErrorMessage, DmarcRulesMarkdownResource.RufTagShouldBeMailToErrorMessage)
        {
        }
    }
}
