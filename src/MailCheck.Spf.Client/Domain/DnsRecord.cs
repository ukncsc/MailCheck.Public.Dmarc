using System;

namespace MailCheck.Spf.Client.Domain
{
    public abstract class DnsRecord
    {
        protected DnsRecord(int id, DateTime? mxLastChecked)
        {
            Id = id;
            MxLastChecked = mxLastChecked;
        }

        public int Id { get; }
        public DateTime? MxLastChecked { get; }
    }
}