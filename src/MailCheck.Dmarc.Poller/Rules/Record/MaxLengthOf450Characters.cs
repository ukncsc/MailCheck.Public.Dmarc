using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Record
{
    public class MaxLengthOf450Characters : IRule<DmarcRecord>
    {
        private const int MaxRecordLength = 450;

        public Guid Id => Guid.Parse("14F69C15-8B68-481B-8510-CA3EFE01D134");

        public Task<List<Error>> Evaluate(DmarcRecord dmarcRecord)
        {
            int recordLength = dmarcRecord.Record.Length;

            List<Error> errors = new List<Error>();
            if (recordLength > MaxRecordLength)
            {
                errors.Add(new Error(Id, "mailcheck.dmarc.maxLengthOf450Characters",
                    ErrorType.Error,
                    string.Format(DmarcRulesResource.MaxLengthOf450CharactersErrorMessage, MaxRecordLength,
                        recordLength),
                    string.Empty));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 4;

        public bool IsStopRule => false;
    }
}