using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Records
{
    public class TldDmarcRecordBehaviourIsWeaklyDefined : IRule<DmarcRecords>
    {
        public Guid Id => Guid.Parse("4CA94601-9748-4CA9-A878-6D98460C9026");

        public Task<List<Error>> Evaluate(DmarcRecords records)
        {
            List<Error> errors = new List<Error>();

            if (records.Records.Any() && records.IsTld)
            {
                errors.Add(new Error(Id, ErrorType.Warning,
                    string.Format(DmarcRulesResource.TldDmarcWeaklyDefinedMessage, records.Domain),
                    string.Format(DmarcRulesMarkdownResource.TldDmarcWeaklyDefinedMessage, records.Domain)));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 2;
        public bool IsStopRule => false;
    }
}
