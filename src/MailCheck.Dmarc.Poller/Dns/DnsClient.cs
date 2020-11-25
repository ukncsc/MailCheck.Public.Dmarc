using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using MailCheck.Common.Util;
using MailCheck.Dmarc.Poller.OrgDomain;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dmarc.Poller.Dns
{
    public interface IDnsClient
    {
        Task<DnsResult<List<DmarcRecordInfo>>> GetDmarcRecords(string domain);
    }

    public class DnsClient : IDnsClient
    {
        private const string SERV_FAILURE_ERROR = "Server Failure";
        private const string NON_EXISTENT_DOMAIN_ERROR = "Non-Existent Domain";
        private readonly ILookupClient _lookupClient;
        private readonly IOrganisationalDomainProvider _organisationalDomainProvider;
        private readonly ILogger<IDnsClient> _log;

        public DnsClient(ILookupClient lookupClient, IOrganisationalDomainProvider organisationalDomainProvider, ILogger<IDnsClient> log)
        {
            _lookupClient = lookupClient;
            _organisationalDomainProvider = organisationalDomainProvider;
            _log = log;
        }

        public async Task<DnsResult<List<DmarcRecordInfo>>> GetDmarcRecords(string domain)
        {
            IDnsQueryResponse response = await _lookupClient.QueryAsync(FormatQuery(domain), QueryType.TXT);

            OrganisationalDomain organisationalDomain =
                await _organisationalDomainProvider.GetOrganisationalDomain(domain);

            string orgDomain = organisationalDomain.OrgDomain;

            bool isTld = organisationalDomain.IsTld;

            List<DmarcRecordInfo> dnsRecords = GetDmarcRecords(response, orgDomain, isTld);

            if (!dnsRecords.Any())
            {
                if (!organisationalDomain.IsOrgDomain && !organisationalDomain.IsTld)
                {
                    response = await _lookupClient.QueryAsync(FormatQuery(orgDomain), QueryType.TXT);
                    dnsRecords = GetDmarcRecords(response, orgDomain, isTld, true);
                }
            }

            if (response.HasError && response.ErrorMessage != NON_EXISTENT_DOMAIN_ERROR && response.ErrorMessage != SERV_FAILURE_ERROR)
            {
                return new DnsResult<List<DmarcRecordInfo>>(response.ErrorMessage);
            }

            return new DnsResult<List<DmarcRecordInfo>>(dnsRecords, response.MessageSize);
        }

        private List<DmarcRecordInfo> GetDmarcRecords(IDnsQueryResponse response, string orgDomain = null, bool isTld = false, bool isInherited = false)
        {
            return response.Answers.OfType<TxtRecord>()
                .Where(_ => _.Text.FirstOrDefault()?.StartsWith("v=dmarc", StringComparison.OrdinalIgnoreCase) ?? false)
                .Select(_ => CreateRecordInfo(_, orgDomain, isTld, isInherited))
                .ToList();
        }

        private DmarcRecordInfo CreateRecordInfo(TxtRecord recordTxt, string orgDomain, bool isTld, bool isInherited)
        {
            List<string> recordParts = recordTxt.Text.Select(_ => _.Escape()).ToList();
            return new DmarcRecordInfo(recordParts, orgDomain, isTld, isInherited);
        }

        protected string FormatQuery(string domain)
        {
            return $"_dmarc.{domain}";
        }
    }
}