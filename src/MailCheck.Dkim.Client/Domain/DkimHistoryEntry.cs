using System.Collections.Generic;

namespace MailCheck.Dkim.Client.Domain
{
    public class DkimHistoryEntry
    {
        public DkimHistoryEntry(string selector, List<string> dnsRecords)
        {
            Selector = selector;
            DnsRecords = dnsRecords ?? new List<string>();
        }

        public string Selector { get; }
        public List<string> DnsRecords { get; }
    }
}