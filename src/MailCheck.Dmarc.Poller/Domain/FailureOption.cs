using System;
using System.Collections.Generic;

namespace MailCheck.Dmarc.Poller.Domain
{
    public class FailureOption : OptionalDefaultTag
    {
        public static FailureOption Default = new FailureOption("fo=0",
            new List<FailureOptionType>
                {Domain.FailureOptionType.Zero},
            true);

        public FailureOption(string value, List<FailureOptionType> failureOptionTypes, bool isImplicit = false)
            : base(value, isImplicit)
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