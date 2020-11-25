using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using MailCheck.Dmarc.Poller.Config;
using MailCheck.Dmarc.Poller.Dns;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Exceptions;
using MailCheck.Dmarc.Poller.Parsing;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<DmarcProcessor> _log;

        public DmarcProcessor(
            IDnsClient dnsClient,
            IDmarcRecordsParser dmarcRecordsParser, 
            IDmarcPollerConfig config,
            ILogger<DmarcProcessor> log)
        {
            _dnsClient = dnsClient;
            _dmarcRecordsParser = dmarcRecordsParser;
            _config = config;
            _log = log;
        }

        public Guid Id => Guid.Parse("9FB68BF8-1232-4707-A456-E126C0186011");

        public async Task<DmarcPollResult> Process(string domain)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            DnsResult<List<DmarcRecordInfo>> dmarcDnsRecords = await _dnsClient.GetDmarcRecords(domain);

            if (dmarcDnsRecords.IsErrored)
            {
                if (_config.AllowNullResults)
                {
                    return new DmarcPollResult(domain,
                        new Error(Id, ErrorType.Error,
                            $"Failed DMARC record query for {domain} with error {dmarcDnsRecords.Error}", string.Empty));
                }

                throw new DmarcPollerException($"Error occurred attempting to retrieve DMARC records for {domain}: {dmarcDnsRecords.Error}");
            }

            if (dmarcDnsRecords.Value.Count == 0 || dmarcDnsRecords.Value.TrueForAll(x => string.IsNullOrWhiteSpace(x.Record)))
            {
                string message = $"DMARC records missing or empty for {domain}.";

                if (!_config.AllowNullResults)
                {
                    throw new DmarcPollerException(message);
                }

                _log.LogDebug(message);
            }

            DmarcRecords records =
                await _dmarcRecordsParser.Parse(domain, dmarcDnsRecords.Value, dmarcDnsRecords.MessageSize);

            DmarcPollResult pollResult = new DmarcPollResult(records, stopwatch.Elapsed);

            return pollResult;
        }
    }
}
