using System;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public interface IExplainerStrategy<in T> : IExplainer<T>
    {
        Type Type { get; }
    }
}