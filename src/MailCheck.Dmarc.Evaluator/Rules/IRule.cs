using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Rules
{
    public interface IRule<in T>
    {
        Task<List<Message>> Evaluate(T t);
        int SequenceNo { get; }
        bool IsStopRule { get; }
    }
}