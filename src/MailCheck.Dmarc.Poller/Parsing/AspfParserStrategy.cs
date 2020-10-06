using MailCheck.Common.Util;
using MailCheck.Dmarc.Poller.Domain;
using System;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public class AspfParserStrategy : ITagParserStrategy
    {
        public Guid Id => Guid.Parse("C47BAF2E-0E6F-4325-B93A-2B98A624A5FD");

        public Tag Parse(string tag, string value)
        {
            AlignmentType alignmentType;
            if (!value.TryParseExactEnum(out alignmentType))
            {
                alignmentType = AlignmentType.Unknown;
            }

            Aspf aspf = new Aspf(tag, alignmentType);

            if (alignmentType == AlignmentType.Unknown)
            {
                string errorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, Tag, value);
                string markdown = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, Tag, value);
                aspf.AddError(new Error(Id, ErrorType.Error, errorMessage, markdown));
            }

            return aspf;
        }

        public string Tag => "aspf";

        public int MaxOccurences => 1;
    }
}
