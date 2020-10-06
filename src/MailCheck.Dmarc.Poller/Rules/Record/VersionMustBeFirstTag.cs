using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;
using Version = MailCheck.Dmarc.Poller.Domain.Version;

namespace MailCheck.Dmarc.Poller.Rules.Record
{
    public class VersionMustBeFirstTag : IRule<DmarcRecord>
    {
        public Guid Id => Guid.Parse("AB1B3800-A282-45BD-BC26-2895B29CDD79");

        public Task<List<Error>> Evaluate(DmarcRecord record)
        {
            List<Error> errors = new List<Error>();

            Tag firstTag = record.Tags.FirstOrDefault();

            if (!(firstTag is Version))
            {
                errors.Add(new Error(Id, ErrorType.Error,
                    string.Format(DmarcRulesResource.VersionMustBeFirstTagErrorMessage, firstTag?.Value ?? "null"),
                    string.Format(DmarcRulesMarkdownResource.VersionMustBeFirstTagErrorMessage, firstTag?.Value ?? "null")));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}