using System;

namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class Adkim : OptionalDefaultTag
    {
        public static Adkim Default = new Adkim("adkim=r", AlignmentType.R, true);

        public Adkim(string value, AlignmentType alignmentType, bool valid, bool isImplicit = false) 
            : base(TagType.Adkim, value, valid, isImplicit)
        {
            AlignmentType = alignmentType;
        }

        public AlignmentType AlignmentType { get; }

        public override string ToString()
        {
            return $"{base.ToString()},{Environment.NewLine}{nameof(AlignmentType)}: {AlignmentType}";
        }
    }
}