using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules
{
    public interface IRule<in T>
    {
        Task<List<Error>> Evaluate(T t);
        int SequenceNo { get; }
        bool IsStopRule { get; }
    }
}