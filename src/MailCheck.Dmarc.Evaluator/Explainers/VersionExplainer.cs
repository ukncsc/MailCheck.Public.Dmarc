using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public class VersionExplainer : BaseTagExplainerStrategy<Version>
    {
        public override string GetExplanation(Version tConcrete)
        {
            return DmarcExplainerResource.VersionDmarc1Explanation;
        }
    }
}