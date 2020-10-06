using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Record
{
    public abstract class TagShouldBeMailTo<T> : IRule<DmarcRecord>
        where T : ReportUri
    {
        private readonly string _errorFormatString;
        private readonly string _markdownString;
        private const string Prefix = "mailto:";

        protected TagShouldBeMailTo(string errorFormatString, string markdownString)
        {
            _errorFormatString = errorFormatString;
            _markdownString = markdownString;
        }

        public Guid Id => Guid.Parse("1AC94C22-DE18-40E4-B60C-54FB19963D2A");

        public Task<List<Error>> Evaluate(DmarcRecord record)
        {
            List<Error> errors = new List<Error>();

            T t = record.Tags.OfType<T>().FirstOrDefault();

            //ignore null uri schemes as these will already have parsing error.
            if (!(t == null || t.Uris.All(_ => string.IsNullOrWhiteSpace(_.Value)) || t.Uris.Select(_ => _.Value.ToLower()).All(_ => _.StartsWith(Prefix))))
            {
                errors.Add(new Error(Id, ErrorType.Warning, _errorFormatString, _markdownString));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}