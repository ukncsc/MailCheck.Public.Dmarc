using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public class ReportFormatExplainer : BaseTagExplainerStrategy<ReportFormat>
    {
        public override string GetExplanation(ReportFormat tConcrete)
        {
            return DmarcExplainerResource.ReportFormatAFRFExplanation;
        }
    }
}