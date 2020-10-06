using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Evaluator.Explainers;
using MailCheck.Dmarc.Evaluator.Rules;

namespace MailCheck.Dmarc.Evaluator
{
    public interface IDmarcEvaluationProcessor
    {
        Task Process(DmarcRecords dmarcRecords);
    }

    public class DmarcEvaluationProcessor : IDmarcEvaluationProcessor
    {
        private readonly IEvaluator<DmarcRecord> _evaluator;
        private readonly IDmarcRecordExplainer _recordExplainer;

        public DmarcEvaluationProcessor(IEvaluator<DmarcRecord> evaluator,
            IDmarcRecordExplainer recordExplainer)
        {
            _evaluator = evaluator;
            _recordExplainer = recordExplainer;
        }

        public async Task Process(DmarcRecords dmarcRecords)
        {
            foreach (DmarcRecord dmarcRecord in dmarcRecords.Records)
            {
                EvaluationResult<DmarcRecord> evaluationResult = await _evaluator.Evaluate(dmarcRecord);
                dmarcRecord.Messages.AddRange(evaluationResult.Errors);
                _recordExplainer.Explain(dmarcRecord);
            }
        }
    }
}