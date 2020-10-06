using System.Collections.Generic;

namespace MailCheck.Dmarc.Entity.Entity.Notifiers
{
    public class MessageEqualityComparer : IEqualityComparer<Contracts.SharedDomain.Message>
    {
        public bool Equals(Contracts.SharedDomain.Message x, Contracts.SharedDomain.Message y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(Contracts.SharedDomain.Message obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}