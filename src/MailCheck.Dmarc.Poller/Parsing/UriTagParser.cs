using System;
using System.Linq;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public interface IUriTagParser
    {
        UriTag Parse(string uriString);
    }

    public class UriTagParser : IUriTagParser
    {
        private const char Separator = '!';

        private readonly IDmarcUriParser _dmarcUriParser;
        private readonly IMaxReportSizeParser _maxReportSizeParser;

        public UriTagParser(IDmarcUriParser dmarcUriParser,
            IMaxReportSizeParser maxReportSizeParser)
        {
            _dmarcUriParser = dmarcUriParser;
            _maxReportSizeParser = maxReportSizeParser;
        }

        public Guid Id => Guid.Parse("55761162-D70B-47B9-924C-F9412B32B589");

        public UriTag Parse(string uriString)
        {
            string[] tokens = uriString?.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries).Select(_ => _.Trim()).ToArray() ?? new string[0];

            DmarcUri dmarcUri = _dmarcUriParser.Parse(tokens.ElementAtOrDefault(0));

            MaxReportSize maxReportSize = tokens.Length > 1
                ? _maxReportSizeParser.Parse(tokens[1])
                : null;

            UriTag uriTag = new UriTag(uriString, dmarcUri, maxReportSize);

            if (tokens.Length > 2)
            {
                string unexpectedValues = string.Join(",", tokens.Skip(2));
                string unexpectedValuesErrorMessage = string.Format(DmarcParserResource.UnexpectedValueErrorMessage, unexpectedValues, "uri", unexpectedValues);
                string markDown = string.Format(DmarcParserMarkdownResource.UnexpectedValueErrorMessage, unexpectedValues, "uri", unexpectedValues);

                uriTag.AddError(new Error(Id, ErrorType.Error, unexpectedValuesErrorMessage, markDown));
            }

            return uriTag;
        }
    }
}