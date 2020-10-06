using System.Linq;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Util
{
    public static class DmarcRecordExtensionMethods
    {
        public static Policy GetEffectivePolicy(this DmarcRecord record)
        {
            Policy policy = record.Tags.OfType<Policy>().FirstOrDefault();
            SubDomainPolicy subPolicy = record.Tags.OfType<SubDomainPolicy>().FirstOrDefault();

            return record.IsInherited && subPolicy != null
                ? new Policy(subPolicy.Value, subPolicy.PolicyType, subPolicy.Valid)
                : policy;
        }
    }
}
