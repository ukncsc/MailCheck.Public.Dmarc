using System;

namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public abstract class Tag : IEquatable<Tag>
    {
        protected Tag(TagType tagType, string value, bool valid)
        {
            TagType = tagType;
            Valid = valid;
            Value = value;
        }

        protected Tag(TagType tagType, string value, string explanation)
        {
            Value = value;
            TagType = tagType;
            Explanation = explanation;
        }

        public string Value { get; }

        public string Explanation { get; set; }
        public TagType TagType { get; }
        public bool Valid { get; set; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}{Environment.NewLine}{nameof(Explanation)}: {Explanation}";
        }

        public bool Equals(Tag other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value) && string.Equals(Explanation, other.Explanation) && TagType == other.TagType && Valid == other.Valid;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Tag) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Explanation != null ? Explanation.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) TagType;
                hashCode = (hashCode * 397) ^ Valid.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Tag left, Tag right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Tag left, Tag right)
        {
            return !Equals(left, right);
        }
    }
}