using MailCheck.Common.Util;
using MailCheck.Dmarc.Poller.Domain;
using System;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public class SubDomainPolicyParserStrategy : ITagParserStrategy
    {
        public Guid Id => Guid.Parse("3BD325D9-00F2-46B1-AEC4-297FB811F7B1");

        public Tag Parse(string tag, string value)
        {
            if (!value.TryParseExactEnum(out PolicyType policyType))
            {
                policyType = PolicyType.Unknown;
            }

            SubDomainPolicy policy = new SubDomainPolicy(tag, policyType);

            if (policyType == PolicyType.Unknown)
            {
                string errorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, Tag, value);
                string markDown = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, Tag, value);

                policy.AddError(new Error(Id, ErrorType.Error, errorMessage, markDown));
            }

            return policy;
        }

        public string Tag => "sp";

        public int MaxOccurences => 1;
    }
}
