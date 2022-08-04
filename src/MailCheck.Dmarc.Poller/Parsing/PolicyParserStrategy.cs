using MailCheck.Common.Util;
using MailCheck.Dmarc.Poller.Domain;
using System;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public class PolicyParserStrategy : ITagParserStrategy
    {
        public Guid Id => Guid.Parse("5FE8FC81-6A3B-43AC-853F-ABD5BFACB52D");

        public Tag Parse(string tag, string value)
        {
            if (!value.TryParseExactEnum(out PolicyType policyType))
            {
                policyType = PolicyType.Unknown;
            }

            Policy policy = new Policy(tag, policyType);

            if (policyType == PolicyType.Unknown)
            {
                string errorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, Tag, value);
                string markDown = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, Tag, value);
                policy.AddError(new Error(Id, "mailcheck.dmarc.invalidPolicyValue", ErrorType.Error, errorMessage, markDown));
            }

            return policy;
        }

        public string Tag => "p";

        public int MaxOccurences => 1;
    }
}
