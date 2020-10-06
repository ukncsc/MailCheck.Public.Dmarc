using MailCheck.Dmarc.Poller.Implicit;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Implicit
{
    public class FailureOptionsImplicitProvider : ImplicitTagProviderStrategyBase<FailureOption>
    {
        public FailureOptionsImplicitProvider() : base(t => FailureOption.Default){}
    }
}