using System.Collections.Generic;

namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class DmarcRecord
    {
        public DmarcRecord(string record, List<Tag> tags, List<Message> messages, string domain, string orgDomain, bool isTld, bool isInherited)
        {
            Record = record;
            Tags = tags;
            Domain = domain;
            OrgDomain = orgDomain;
            IsTld = isTld;
            IsInherited = isInherited;
            Messages = messages ?? new List<Message>();
            Tags = tags ?? new List<Tag>();
        }

        public string Record { get; }
        public List<Tag> Tags { get; }
        public string Domain { get; }
        public string OrgDomain { get; }
        public bool IsTld { get; }
        public bool IsInherited { get; }
        public bool IsOrgDomain => string.Equals(Domain, OrgDomain);

        public List<Message> Messages { get; set; }
    }
}
