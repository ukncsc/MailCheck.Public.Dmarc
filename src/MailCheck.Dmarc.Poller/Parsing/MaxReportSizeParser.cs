using System;
using System.Text.RegularExpressions;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public interface IMaxReportSizeParser
    {
        MaxReportSize Parse(string value);
    }

    public class MaxReportSizeParser : IMaxReportSizeParser
    {
        private readonly Regex _regex = new Regex(@"^(?<size>[0-9]+)(?<unit>[kmgt]{0,1})", RegexOptions.IgnoreCase);

        public MaxReportSize Parse(string value)
        {
            if (value != null)
            {
                Match match = _regex.Match(value);
                if (match.Success)
                {
                    string sizeToken = match.Groups["size"].Value.ToLower();
                    string unitToken = match.Groups["unit"].Value;

                    ulong? size = ulong.TryParse(sizeToken, out ulong candidateSize)
                        ? candidateSize
                        : (ulong?) null;

                    if (!Enum.TryParse(unitToken, true, out Unit unit))
                    {
                        unit = Unit.B; //byte is default if no unit specified.
                    }

                    MaxReportSize maxReportSize = new MaxReportSize(size, unit);

                    if (!size.HasValue)
                    {
                        Guid Error1Id = Guid.Parse("BCF95FA3-B4EA-461D-9E70-87809BBD70BF");

                        string maxSizeErrorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage,
                            "max size", sizeToken);

                        string markDown1 = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage,
                            "max size", sizeToken);

                        maxReportSize.AddError(new Error(Error1Id, ErrorType.Error, maxSizeErrorMessage, markDown1));
                    }

                    if (!string.IsNullOrEmpty(unitToken) && unit == Unit.Unknown)
                    {
                        Guid Error2Id = Guid.Parse("A409E6B2-527C-4809-9BC3-AEE735E75F66");

                        string unitErrorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, "unit",
                            unitToken);

                        string markDown2 = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, "unit",
                         unitToken);

                        maxReportSize.AddError(new Error(Error2Id, ErrorType.Error, unitErrorMessage, markDown2));
                    }
                    return maxReportSize;
                }
            }

            Guid Error3Id = Guid.Parse("9E5EC0B9-8454-4A94-B6F3-C6D9FC140AB7");

            MaxReportSize invalidMaxReportSize = new MaxReportSize(null, Unit.Unknown);
            string maxReportSizeErrorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, "max report size", value);
            string markDown = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, "max report size", value);

            invalidMaxReportSize.AddError(new Error(Error3Id, ErrorType.Error, maxReportSizeErrorMessage, markDown));
            return invalidMaxReportSize;
        }
    }
}