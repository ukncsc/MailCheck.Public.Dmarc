using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Rules.Record
{
    public class SubDomainPolicyShouldNotBeOnNonOrganisationalDomain : IRule<DmarcRecord>
    {
        public Guid Id => Guid.Parse("27DACE1F-F330-4EF2-A365-96A57B87A2E3");

        public Task<List<Error>> Evaluate(DmarcRecord record)
        {
            SubDomainPolicy subDomainPolicy = record.Tags.OfType<SubDomainPolicy>().FirstOrDefault();
            List<Error> errors = new List<Error>();

            if (!record.IsInherited && !string.Equals(record.Domain, record.OrgDomain) && subDomainPolicy != null && !subDomainPolicy.IsImplicit)
            {
                errors.Add(new Error(Id, "mailcheck.dmarc.subDomainPolicyShouldNotBeOnNonOrganisationalDomain", ErrorType.Info,
                                            string.Format(DmarcRulesResource.SubDomainIneffectualErrorMessage, record.Domain),
                                            string.Format(DmarcRulesMarkdownResource.SubDomainIneffectualErrorMessage, record.Domain)));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 8;
        public bool IsStopRule => false;
    }
}
