using System.Collections.Generic;

namespace MailCheck.Dmarc.Poller.Dns
{
    public class DmarcRecordInfo
    {
        public DmarcRecordInfo(List<string> recordParts, string orgDomain, bool isTld, bool isInherited)
        {
            RecordParts = recordParts;
            Record = string.Join(string.Empty, recordParts);
            OrgDomain = orgDomain;
            IsTld = isTld;
            IsInherited = isInherited;
        }

        public string Record { get; }
        public List<string> RecordParts { get; }
        public string OrgDomain { get; }
        public bool IsTld { get; }
        public bool IsInherited { get; }

        protected bool Equals(DmarcRecordInfo other)
        {
            return string.Equals(Record, other.Record) && string.Equals(OrgDomain, other.OrgDomain) &&
                   IsTld == other.IsTld && IsInherited == other.IsInherited;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DmarcRecordInfo)obj);
        }

        public override int GetHashCode()
        {
            return Record != null ? Record.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return $"{nameof(Record)}: {Record}";
        }
    }
}