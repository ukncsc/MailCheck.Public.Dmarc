using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public class AdkimTagExplainer : BaseTagExplainerStrategy<Adkim>
    {
        public override string GetExplanation(Adkim tConcrete)
        {
            return tConcrete.AlignmentType == AlignmentType.S
                ? DmarcExplainerResource.AdkimStrictExplanation
                : DmarcExplainerResource.AdkimRelaxedExplanation;
        }
    }
}