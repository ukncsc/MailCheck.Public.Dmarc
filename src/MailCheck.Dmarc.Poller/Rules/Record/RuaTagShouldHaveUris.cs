using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Record
{
    public class RuaTagShouldHaveUris : IRule<DmarcRecord>
    {
        private readonly string _zeroUrisErrorMessage;
        private readonly string _zeroUrisMarkdownMessage;

        public RuaTagShouldHaveUris(string zeroUrisErrorMessage, string zeroUrisMarkdownMessage)
        {
            _zeroUrisErrorMessage = zeroUrisErrorMessage;
            _zeroUrisMarkdownMessage = zeroUrisMarkdownMessage;
        }

        public Guid Id => Guid.Parse("037CE95E-B1D3-44F8-A9ED-6B8E1EB037C1");

        public Task<List<Error>> Evaluate(DmarcRecord record)
        {
            List<Error> errors = new List<Error>();
            ReportUriAggregate t = record.Tags.OfType<ReportUriAggregate>().FirstOrDefault();

            //ignore null uri schemes as these will already have parsing error.
            if(t != null && !t.Uris.Any())
            {
                errors.Add(new Error(Id, ErrorType.Warning, _zeroUrisErrorMessage, _zeroUrisMarkdownMessage));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 4;

        public bool IsStopRule => false;
    }
}
