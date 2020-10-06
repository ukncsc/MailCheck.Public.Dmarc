using System;
using System.Threading.Tasks;
using Louw.PublicSuffix;

namespace MailCheck.Dmarc.Poller.OrgDomain
{
    public interface IOrganisationalDomainProvider
    {
        Task<OrganisationalDomain> GetOrganisationalDomain(string domain);
    }

    public class OrganisationDomainProvider : IOrganisationalDomainProvider
    {
        private readonly DomainParser _domainParser;

        public OrganisationDomainProvider()
        {
            WebTldRuleProvider tldRuleProvider = new WebTldRuleProvider(timeToLive: TimeSpan.FromDays(7));

            _domainParser = new DomainParser(tldRuleProvider);
        }

        public async Task<OrganisationalDomain> GetOrganisationalDomain(string domain)
        {
            domain = domain.Trim().TrimEnd(new[] {'.'});

            DomainInfo domainInfo = await _domainParser.ParseAsync(domain);

            return domainInfo == null ?
                new OrganisationalDomain(null, domain, true) : 
                new OrganisationalDomain(domainInfo.RegistrableDomain, domain);
        }
    }
}
