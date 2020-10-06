using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Implicit
{
    public class PercentImplicitProvider : ImplicitTagProviderStrategyBase<Percent>
    {
        public PercentImplicitProvider() : base(t => Percent.Default){}
    }
}