using System.Collections.Generic;

namespace MailCheck.Dmarc.Poller.Implicit
{
    public interface IImplicitProviderStrategy<T>
    {
        bool TryGetImplicitTag(List<T> ts, out T t);
    }
}