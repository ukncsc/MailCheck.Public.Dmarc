using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Records
{
    public class MigrationOnlyOneDmarcRecord : IRule<DmarcRecords>
    {
        public Guid Id => Guid.Parse("28F88964-9B75-42C4-A73B-BE9EE5985C6B");

        public Task<List<Error>> Evaluate(DmarcRecords records)
        {
            List<Error> errors = new List<Error>();

            if (records.IsTld || records.Records.Count == 1) return Task.FromResult(errors);

            errors.Add(records.Records.Count > 1
                ? new Error(Id, "mailcheck.dmarc.onlyOneDmarcRecord", ErrorType.Error, DmarcRulesResource.OnlyOneDmarcRecordErrorMessage, DmarcRulesMarkdownResource.OnlyOneDmarcRecordErrorMessage)
                : new Error(Id, "mailcheck.dmarc.noDmarcRecord", ErrorType.Error, string.Format(DmarcRulesResource.NoDmarcErrorMessage, records.Domain), string.Format(DmarcRulesMarkdownResource.MigrationNoDmarcErrorMessage, records.Domain)));

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}
