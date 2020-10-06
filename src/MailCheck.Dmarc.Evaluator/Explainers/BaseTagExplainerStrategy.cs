using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public abstract class BaseTagExplainerStrategy<TConcrete> : BaseExplainerStrategy<Tag, TConcrete>
        where TConcrete : Tag
    {
        public override bool TryExplain(Tag t, out string explanation)
        {
            TConcrete concrete = ToTConcrete(t);

            if (concrete.Valid)
            {
                explanation = GetExplanation(concrete);
                return true;
            }
            explanation = null;
            return false;
        }

        public abstract string GetExplanation(TConcrete tConcrete);
    }
}