using MailCheck.Common.Util;
using MailCheck.Dmarc.Poller.Domain;
using System;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public class ReportFormatParserStrategy : ITagParserStrategy
    {
        public Guid Id => Guid.Parse("C31C8110-F70D-4EEB-A4EE-135991A41C9A");

        public Tag Parse(string tag, string value)
        {
            ReportFormatType reportFormatType;
            if (!value.TryParseExactEnum(out reportFormatType))
            {
                reportFormatType = ReportFormatType.Unknown;
            }

            ReportFormat reportFormat = new ReportFormat(tag, reportFormatType);

            if (reportFormatType == ReportFormatType.Unknown)
            {
                string errorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, Tag, value);
                string markDown = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, Tag, value);

                reportFormat.AddError(new Error(Id, "mailcheck.dmarc.invalidReportFormatValue", ErrorType.Error, errorMessage, markDown));
            }

            return reportFormat;
        }

        public string Tag => "rf";

        public int MaxOccurences => 1;
    }
}
