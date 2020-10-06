using System;
using System.Collections.Generic;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Implicit
{
    public abstract class ImplicitTagProviderStrategyBase<TConcrete> 
        : ImplicitProviderStrategyBase<Tag, TConcrete>
        where TConcrete : Tag
    {
        protected ImplicitTagProviderStrategyBase(Func<List<Tag>, TConcrete> defaultValueFactory)
            :base(defaultValueFactory)
        {}   
    }
}