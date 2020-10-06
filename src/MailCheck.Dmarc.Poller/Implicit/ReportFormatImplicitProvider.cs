using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Implicit
{
    public class ReportFormatImplicitProvider : ImplicitTagProviderStrategyBase<ReportFormat>
    {
        public ReportFormatImplicitProvider() : base(t => ReportFormat.Default){}
    }
}