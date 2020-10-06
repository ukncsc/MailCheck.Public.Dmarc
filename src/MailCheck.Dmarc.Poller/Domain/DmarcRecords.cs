using System.Collections.Generic;
using System.Linq;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Poller.Domain
{
    public class DmarcRecords : DmarcEntity
    {
        public DmarcRecords(string domain, List<DmarcRecord> records, int messageSize, string orgDomain = null, bool isTld = false, bool isInherited = false)
        {
            Records = records ?? new List<DmarcRecord>();
            Domain = domain;
            MessageSize = messageSize;
            IsTld = isTld;
            IsInherited = isInherited;
            OrgDomain = orgDomain;
        }

        public string OrgDomain { get; }
        public bool IsTld { get; }
        public bool IsInherited { get; }
        public string Domain { get; }
        public int MessageSize { get; }


        public List<DmarcRecord> Records { get; }

        public override int AllErrorCount => Records.Sum(_ => _.AllErrorCount) + ErrorCount;

        public override IReadOnlyList<Error> AllErrors => Records.SelectMany(_ => _.AllErrors).Concat(Errors).ToArray();
    }
}
