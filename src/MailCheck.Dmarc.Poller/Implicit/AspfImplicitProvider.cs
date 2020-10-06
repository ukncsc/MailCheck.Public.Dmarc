using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Implicit
{
    public class AspfImplicitProvider : ImplicitTagProviderStrategyBase<Aspf>
    {
        public AspfImplicitProvider() : base(t => Aspf.Default){}
    }
}