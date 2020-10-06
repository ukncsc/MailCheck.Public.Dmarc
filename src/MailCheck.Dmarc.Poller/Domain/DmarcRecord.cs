using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Poller.Domain
{
    public class DmarcRecord : DmarcEntity
    {
        public DmarcRecord(string record, List<Tag> tags, string domain, string orgDomain, bool isTld, bool isInherited)
        {
            Record = record;
            Tags = tags;
            Domain = domain;
            OrgDomain = orgDomain;
            IsTld = isTld;
            IsInherited = isInherited;
            Messages = new List<Message>();
        }

        public string Record { get; }
        public List<Tag> Tags { get; }
        public string Domain { get; }
        public string OrgDomain { get; }
        public bool IsTld { get; }
        public bool IsInherited { get; }

        public override int AllErrorCount => Tags.Sum(x => x.AllErrorCount) + ErrorCount;

        public override IReadOnlyList<Error> AllErrors => Tags.SelectMany(_ => _.AllErrors).Concat(Errors).ToArray();
        public List<Message> Messages { get; set; }

        public bool IsOrgDomain => string.Equals(Domain, OrgDomain);

        public override string ToString()
        {
            return $"{nameof(Record)}: {Record}{Environment.NewLine}" +
                   $"{string.Join(Environment.NewLine, Tags)}{Environment.NewLine}" +
                   $"{(AllValid ? "Valid" : $"Invalid{Environment.NewLine}{string.Join(Environment.NewLine, Errors)}")}";
        }
    }
}
