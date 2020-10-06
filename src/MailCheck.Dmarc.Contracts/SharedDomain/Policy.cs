using System;

namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class Policy : Tag, IEquatable<Policy>
    {
        public Policy(string value, PolicyType policyType, bool valid) : base(TagType.Policy, value, valid)
        {
            PolicyType = policyType;
        }

        public PolicyType PolicyType { get; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(PolicyType)}: {PolicyType}";
        }

        public bool Equals(Policy other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && PolicyType == other.PolicyType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Policy) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int) PolicyType;
            }
        }

        public static bool operator ==(Policy left, Policy right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Policy left, Policy right)
        {
            return !Equals(left, right);
        }
    }
}