using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Config;
using MailCheck.Dmarc.Poller.Dns;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Exceptions;
using MailCheck.Dmarc.Poller.Parsing;

namespace MailCheck.Dmarc.Poller
{
    public interface IDmarcProcessor
    {
        Task<DmarcPollResult> Process(string domain);
    }

    public class DmarcProcessor : IDmarcProcessor
    {
        private readonly IDnsClient _dnsClient;
        private readonly IDmarcRecordsParser _dmarcRecordsParser;
        private readonly IDmarcPollerConfig _config;

        public DmarcProcessor(IDnsClient dnsClient,
            IDmarcRecordsParser dmarcRecordsParser, IDmarcPollerConfig config)
        {
            _dnsClient = dnsClient;
            _dmarcRecordsParser = dmarcRecordsParser;
            _config = config;
        }

        public Guid Id => Guid.Parse("9FB68BF8-1232-4707-A456-E126C0186011");

        public async Task<DmarcPollResult> Process(string domain)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            DnsResult<List<DmarcRecordInfo>> dmarcDnsRecords = await _dnsClient.GetDmarcRecords(domain);

            if (!_config.AllowNullResults && (dmarcDnsRecords.IsErrored ||
                dmarcDnsRecords.Value.TrueForAll(x => string.IsNullOrWhiteSpace(x.Record))))
            {
                throw new DmarcPollerException($"Unable to retrieve dmarc records for {domain}.");
            }

            if (dmarcDnsRecords.IsErrored)
            {
                return new DmarcPollResult(domain,
                    new Error(Id, ErrorType.Error,
                        $"Failed Dmarc record query for {domain} with error {dmarcDnsRecords.Error}", string.Empty));
            }

            DmarcRecords records =
                await _dmarcRecordsParser.Parse(domain, dmarcDnsRecords.Value, dmarcDnsRecords.MessageSize);

            DmarcPollResult pollResult = new DmarcPollResult(records, stopwatch.Elapsed);

            return pollResult;
        }
    }
}
