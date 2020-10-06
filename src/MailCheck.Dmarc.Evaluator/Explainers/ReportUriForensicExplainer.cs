using System;
using System.Linq;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public class ReportUriForensicExplainer : BaseTagExplainerStrategy<ReportUriForensic>
    {
        public override string GetExplanation(ReportUriForensic tConcrete)
        {
            string uris = string.Join(Environment.NewLine, tConcrete.Uris.Select(_ => $"{_.Uri.Uri.UserInfo}@{_.Uri.Uri.Host}"));
            return string.Format(DmarcExplainerResource.ReportUriForensicExplanation, uris);
        }
    }
}