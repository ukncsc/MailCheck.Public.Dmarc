using MailCheck.Dmarc.Poller.Domain;
using System;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public class PercentParserStrategy : ITagParserStrategy
    {
        public Guid Id => Guid.Parse("EBCE2AEC-09D2-41E0-B3D6-95F95734C530");

        public Tag Parse(string tag, string value)
        {
            int? percentValue = null;
            if (int.TryParse(value, out int candidatePercentValue) && candidatePercentValue <= 100 && candidatePercentValue >= 0)
            {
                percentValue = candidatePercentValue;
            }

            Percent percent = new Percent(tag, percentValue);

            if (!percentValue.HasValue)
            {
                string errorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, Tag, value);
                string markDown = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, Tag, value);
                percent.AddError(new Error(Id, "mailcheck.dmarc.invalidPercentValue", ErrorType.Error, errorMessage, markDown));
            }

            return percent;
        }

        public string Tag => "pct";

        public int MaxOccurences => 1;
    }
}