using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Record
{
    public class RuaTagsShouldBeMailTo : TagShouldBeMailTo<ReportUriAggregate>
    {
        public RuaTagsShouldBeMailTo()
            : base(DmarcRulesResource.RuaTagsShouldBeMailToErrorMessage, DmarcRulesMarkdownResource.RuaTagsShouldBeMailToErrorMessage)
        {
        }
    }
}
