using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public class ReportIntervalExplainer : BaseTagExplainerStrategy<ReportInterval>
    {
        public override string GetExplanation(ReportInterval tConcrete)
        {
            return string.Format(DmarcExplainerResource.ReportIntervalExplanation,
                tConcrete.Interval.Value, tConcrete.Interval.Value / 3600);
        }
    }
}