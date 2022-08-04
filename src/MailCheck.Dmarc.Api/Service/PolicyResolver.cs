using System.Collections.Generic;
using System.Linq;
using MailCheck.Dmarc.Api.Domain;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Api.Service
{
    public interface IPolicyResolver
    {
        string Resolve(DmarcInfoResponse response);
    }

    public class PolicyResolver : IPolicyResolver
    {
        public string Resolve(DmarcInfoResponse response)
        {
            if (response == null || 
                response.DmarcRecords == null || 
                response.DmarcRecords.Records?.FirstOrDefault()?.Tags == null)
            {
                return null;
            }

            List<Tag> tags = response.DmarcRecords.Records[0].Tags;
            if (tags == null) return null;

            Tag policyTag = tags.FirstOrDefault(x => x.TagType == TagType.Policy);

            if (policyTag == null) return null;

            PolicyType policy  = ((Policy) policyTag).PolicyType;

            switch (policy)
            {
                case PolicyType.None:
                    return "none";
                case PolicyType.Quarantine:
                    return "quarantine";
                case PolicyType.Reject:
                    return "reject";
                default:
                    return "unknown";
            }
        }
    }
}