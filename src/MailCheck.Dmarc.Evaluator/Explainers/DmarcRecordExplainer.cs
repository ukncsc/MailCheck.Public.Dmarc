using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public interface IDmarcRecordExplainer
    {
        void Explain(DmarcRecord record);
    }

    public class DmarcRecordExplainer : IDmarcRecordExplainer
    {
        private readonly IExplainer<Tag> _tagExplainer;

        public DmarcRecordExplainer(IExplainer<Tag> tagExplainer)
        {
            _tagExplainer = tagExplainer;
        }

        public void Explain(DmarcRecord record)
        {
            foreach (Tag tag in record.Tags)
            {
                if (_tagExplainer.TryExplain(tag, out string termExplanation))
                {
                    tag.Explanation = termExplanation;
                }
            }
        }
    }
}
