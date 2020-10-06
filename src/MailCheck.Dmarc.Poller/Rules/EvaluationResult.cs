using System.Collections.Generic;
using System.Linq;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules
{
    public class EvaluationResult<T>
    {
        public EvaluationResult(T item, params Error[] errors)
        {
            Item = item;
            Errors = errors.ToList();
        }

        public EvaluationResult(T item, List<Error> errors)
        {
            Item = item;
            Errors = errors;
        }

        public T Item { get; }

        public List<Error> Errors { get; }
    }
}