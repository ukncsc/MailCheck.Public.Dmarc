using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public class PercentExplainer : BaseTagExplainerStrategy<Percent>
    {
        public override string GetExplanation(Percent tConcrete)
        {
            return string.Format(DmarcExplainerResource.PercentExplanation, tConcrete.PercentValue.Value);
        }
    }
}