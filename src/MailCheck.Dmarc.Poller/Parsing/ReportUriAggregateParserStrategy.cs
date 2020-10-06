using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public class ReportUriAggregateParserStrategy : ITagParserStrategy
    {
        private const char Separator = ',';
        private readonly IUriTagParser _uriTagParser;

        public ReportUriAggregateParserStrategy(IUriTagParser uriTagParser)
        {
            _uriTagParser = uriTagParser;
        }

        public Tag Parse(string tag, string value)
        {
            string[] tokens = value.Split(new [] { Separator }, StringSplitOptions.RemoveEmptyEntries).Select(_ => _.Trim()).ToArray();
            List<UriTag> uris = tokens.Select(_uriTagParser.Parse).ToList();

            return new ReportUriAggregate(tag, uris);
        }

        public string Tag => "rua";

        public int MaxOccurences => 1;
    }
}