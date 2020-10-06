using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Implicit
{
    public class AdkimImplicitProvider : ImplicitTagProviderStrategyBase<Adkim>
    {
        public AdkimImplicitProvider() : base(t => Adkim.Default){}
    }
}