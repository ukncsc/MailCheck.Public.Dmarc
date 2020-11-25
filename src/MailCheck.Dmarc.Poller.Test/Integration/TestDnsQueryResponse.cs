using System.Collections.Generic;
using System.Linq;
using DnsClient;
using DnsClient.Protocol;

namespace MailCheck.Dmarc.Poller.Test.Integration
{
    public class TestDnsQueryResponse : IDnsQueryResponse
    {
        public TestDnsQueryResponse(string[] txtRecords)
        {
            Answers = txtRecords
                .Select(x => new TxtRecord(new ResourceRecordInfo("", ResourceRecordType.TXT, QueryClass.IN, 100, 100), new[] {x}, new[] {x}))
                .ToArray();
        }

        public IReadOnlyList<DnsQuestion> Questions { get; }
        public IReadOnlyList<DnsResourceRecord> Additionals { get; }
        public IEnumerable<DnsResourceRecord> AllRecords { get; }
        public IReadOnlyList<DnsResourceRecord> Answers { get; }
        public IReadOnlyList<DnsResourceRecord> Authorities { get; }
        public string AuditTrail { get; }
        public string ErrorMessage { get; }
        public bool HasError { get; }
        public DnsResponseHeader Header { get; }
        public int MessageSize { get; }
        public NameServer NameServer { get; }
        public DnsQuerySettings Settings => throw new System.NotImplementedException();
    }
}