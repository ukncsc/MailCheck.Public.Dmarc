using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Record
{
    public abstract class TagShouldNotHaveMoreThanTwoUris<T> : IRule<DmarcRecord>
        where T : ReportUri
    {
        private readonly string _moreThan2UrisErrorFormatString;
        private readonly string _moreThan2UrisMarkDownFormatString;

        protected TagShouldNotHaveMoreThanTwoUris(string moreThan2UrisErrorFormatString, string moreThan2UrisMarkDownFormatString)
        {
            _moreThan2UrisErrorFormatString = moreThan2UrisErrorFormatString;
            _moreThan2UrisMarkDownFormatString = moreThan2UrisMarkDownFormatString;
        }

        public abstract Guid Id { get; }
        public abstract string Name { get; }

        public Task<List<Error>> Evaluate(DmarcRecord record)
        {
            List<Error> errors = new List<Error>();

            T t = record.Tags.OfType<T>().FirstOrDefault();
            if (t != null && t.Uris.Count > 2)
            {
                errors.Add(new Error(Id, Name, ErrorType.Error, string.Format(_moreThan2UrisErrorFormatString, t.Uris.Count), string.Format(_moreThan2UrisMarkDownFormatString, t.Uris.Count)));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 7;
        public bool IsStopRule => false;
    }
}