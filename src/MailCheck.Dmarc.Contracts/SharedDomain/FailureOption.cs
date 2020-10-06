using System;
using System.Collections.Generic;

namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class FailureOption : OptionalDefaultTag
    {
        public static FailureOption Default =
            new FailureOption("fo=0", new List<FailureOptionType> {FailureOptionType.Zero}, true);

        public FailureOption(string value, List<FailureOptionType> failureOptionTypes, bool valid,
            bool isImplicit = false)
            : base(TagType.FailureOption, value, valid, isImplicit)
        {
            FailureOptionTypes = failureOptionTypes;
        }

        public List<FailureOptionType> FailureOptionTypes { get; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(FailureOptionTypes)}: {String.Join(",", FailureOptionTypes)}";
        }
    }
}