using MailCheck.Common.Util;
using MailCheck.Dmarc.Poller.Domain;
using System;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public class AdkimParserStrategy : ITagParserStrategy
    {
        public Guid Id => Guid.Parse("792035E2-D439-48F4-BB44-9775DD691F1F");

        public Tag Parse(string tag, string value)
        {
            AlignmentType alignmentType;
            if (!value.TryParseExactEnum(out alignmentType))
            {
                alignmentType = AlignmentType.Unknown;
            }

            Adkim adkim = new Adkim(tag, alignmentType);

            if (alignmentType == AlignmentType.Unknown)
            {
                string errorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, Tag, value);
                string markDown = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, Tag, value);
                adkim.AddError(new Error(Id, "mailcheck.dmarc.invalidAdkimValue", ErrorType.Error, errorMessage, markDown));
            }

            return adkim;
        }

        public string Tag => "adkim";

        public int MaxOccurences => 1;
    }
}
