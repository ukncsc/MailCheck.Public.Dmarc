using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public class ReportUriForensicParserStrategy : ITagParserStrategy
    {
        private const char Separator = ',';
        private readonly IUriTagParser _uriTagParser;

        public ReportUriForensicParserStrategy(IUriTagParser uriTagParser)
        {
            _uriTagParser = uriTagParser;
        }

        public Tag Parse(string tag, string value)
        {
            string[] tokens = value.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries).Select(_ => _.Trim()).ToArray();
            List<UriTag> uris = tokens.Select(_uriTagParser.Parse).ToList();

            return new ReportUriForensic(tag, uris);
        }

        public string Tag => "ruf";

        public int MaxOccurences => 1;
    }
}