using System;

namespace MailCheck.Spf.Client.Domain
{
    public class SpfRecord : DnsRecord
    {
        public SpfRecord(int id, string record, DateTime? mxLastChecked)
            : base(id, mxLastChecked)
        {
            Record = record;
        }

        public string Record { get; }
    }
}