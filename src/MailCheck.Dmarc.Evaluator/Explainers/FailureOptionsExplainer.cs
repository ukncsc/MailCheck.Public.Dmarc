using System;
using System.Text;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Explainers
{
    public class FailureOptionsExplainer : BaseTagExplainerStrategy<FailureOption>
    {
        public override string GetExplanation(FailureOption tConcrete)
        {
            StringBuilder sb = new StringBuilder();

            foreach (FailureOptionType failureOptionType in tConcrete.FailureOptionTypes)
            {
                if (sb.Length > 0)
                {
                    sb.Append(Environment.NewLine);
                }

                switch (failureOptionType)
                {
                    case FailureOptionType.Zero:
                        sb.Append(DmarcExplainerResource.FailureOptionsZeroExplanation);
                        break;
                    case FailureOptionType.One:
                        sb.Append(DmarcExplainerResource.FailureOptionsOneExplanation);
                        break;
                    case FailureOptionType.D:
                        sb.Append(DmarcExplainerResource.FailureOptionsDExplanation);
                        break;
                    case FailureOptionType.S:
                        sb.Append(DmarcExplainerResource.FailureOptionsSExplanation);
                        break;
                    default:
                        throw new ArgumentException(
                            $"Unexpected {nameof(FailureOptionType)}: {String.Join(",", tConcrete.FailureOptionTypes)}");
                }
            }

            return sb.ToString();
        }
    }
}