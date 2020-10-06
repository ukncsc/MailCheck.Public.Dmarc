using System;

namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class Aspf : OptionalDefaultTag
    {
        public static Aspf Default = new Aspf("aspf=r", AlignmentType.R, true);

        public Aspf(string value, AlignmentType alignmentType, bool valid, bool isImplicit =false)
            : base(TagType.Aspf, value, valid, isImplicit)
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