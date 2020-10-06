using MailCheck.Dmarc.Poller.Domain;
using System;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public class ReportIntervalParserStrategy : ITagParserStrategy
    {
        public Guid Id => Guid.Parse("DE2C1EFD-4B07-4D5B-BD04-731893D4E5D6");

        public Domain.Tag Parse(string tag, string value)
        {
            uint candidateInterval;
            uint? interval = null;
            if (uint.TryParse(value, out candidateInterval))
            {
                interval = candidateInterval;
            }

            ReportInterval reportInterval = new ReportInterval(tag, interval);

            if (!interval.HasValue)
            {
                string errorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, Tag, value);
                string markDown = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, Tag, value);
                reportInterval.AddError(new Error(Id, ErrorType.Error, errorMessage, markDown));
            }

            return reportInterval;
        }

        public string Tag => "ri";

        public int MaxOccurences => 1;
    }
}