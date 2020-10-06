using System;

namespace MailCheck.Dmarc.Poller.Domain
{
    public class Adkim : OptionalDefaultTag
    {
        public static Adkim Default = new Adkim("adkim=r", AlignmentType.R, true);

        public Adkim(string value, AlignmentType alignmentType, bool isImplicit = false) 
            : base(value, isImplicit)
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