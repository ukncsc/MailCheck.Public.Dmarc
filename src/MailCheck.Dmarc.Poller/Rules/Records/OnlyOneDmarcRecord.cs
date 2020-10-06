using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Records
{
    public class OnlyOneDmarcRecord : IRule<DmarcRecords>
    {
        public Guid Id => Guid.Parse("59B3403F-90EE-4B2A-A96B-FD5932D95AF0");

        public Task<List<Error>> Evaluate(DmarcRecords records)
        {
            List<Error> errors = new List<Error>();

            if (records.IsTld || records.Records.Count == 1) return Task.FromResult(errors);

            errors.Add(records.Records.Count > 1
                ? new Error(Id, ErrorType.Error, DmarcRulesResource.OnlyOneDmarcRecordErrorMessage, DmarcRulesMarkdownResource.OnlyOneDmarcRecordErrorMessage)
                : new Error(Id, ErrorType.Error, string.Format(DmarcRulesResource.NoDmarcErrorMessage, records.Domain), string.Format(DmarcRulesMarkdownResource.NoDmarcErrorMessage, records.Domain)));

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}
