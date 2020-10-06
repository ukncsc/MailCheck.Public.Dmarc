using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public class AspfTagExplainer : BaseTagExplainerStrategy<Aspf>
    {
        public override string GetExplanation(Aspf aspf)
        {
            return aspf.AlignmentType == AlignmentType.S
                ? DmarcExplainerResource.AspfStrictExplanation
                : DmarcExplainerResource.AspfRelaxedExplanation;
        }
    }
}