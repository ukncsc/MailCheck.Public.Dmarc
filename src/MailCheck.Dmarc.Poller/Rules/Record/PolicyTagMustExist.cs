using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Record
{
    public class PolicyTagMustExist : IRule<DmarcRecord>
    {
        public Guid Id => Guid.Parse("FA746F21-FB2E-4BB2-933A-7C714532F0EB");

        public Task<List<Error>> Evaluate(DmarcRecord record)
        {
            List<Error> errors = new List<Error>();

            if (!record.Tags.OfType<Policy>().Any())
            {
                errors.Add(new Error(Id, "mailcheck.dmarc.policyTagMustExist", ErrorType.Error, DmarcRulesResource.PolicyTagMustExistErrorMessage, DmarcRulesMarkdownResource.PolicyTagMustExistErrorMessage));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}