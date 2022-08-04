using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class DmarcRecords : IEquatable<DmarcRecords>
    {
        public DmarcRecords(string domain, List<DmarcRecord> records, List<Message> messages, int messageSize,
            string orgDomain = null, bool isTld = false, bool isInherited = false)
        {
            Records = records ?? new List<DmarcRecord>();
            Messages = messages ?? new List<Message>();
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
        public List<Message> Messages { get; set; }

        public bool Equals(DmarcRecords other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(OrgDomain, other.OrgDomain) && IsTld == other.IsTld &&
                   IsInherited == other.IsInherited && string.Equals(Domain, other.Domain) &&
                   MessageSize == other.MessageSize && Equals(Records, other.Records) &&
                   Equals(Messages, other.Messages);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DmarcRecords) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (OrgDomain != null ? OrgDomain.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsTld.GetHashCode();
                hashCode = (hashCode * 397) ^ IsInherited.GetHashCode();
                hashCode = (hashCode * 397) ^ (Domain != null ? Domain.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MessageSize;
                hashCode = (hashCode * 397) ^ (Records != null ? Records.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Messages != null ? Messages.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(DmarcRecords left, DmarcRecords right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DmarcRecords left, DmarcRecords right)
        {
            return !Equals(left, right);
        }
    }
}
