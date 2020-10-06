using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Implicit
{
    public class ReportIntervalImplicitProvider : ImplicitTagProviderStrategyBase<ReportInterval>
    {
        public ReportIntervalImplicitProvider() : base(t => ReportInterval.Default){}
    }
}