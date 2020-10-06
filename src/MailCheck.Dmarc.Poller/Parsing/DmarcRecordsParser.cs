using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Dns;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Rules;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public interface IDmarcRecordsParser
    {
        Task<DmarcRecords> Parse(string domain, List<DmarcRecordInfo> dmarcRecords, int messageSize);
    }

    public class DmarcRecordsParser : IDmarcRecordsParser
    {
        private readonly IDmarcRecordParser _recordParser;
        private readonly IEvaluator<DmarcRecords> _dmarcRecordsEvaluator;

        public DmarcRecordsParser(IDmarcRecordParser recordParser, IEvaluator<DmarcRecords> dmarcRecordsEvaluator)
        {
            _recordParser = recordParser;
            _dmarcRecordsEvaluator = dmarcRecordsEvaluator;
        }

        public async Task<DmarcRecords> Parse(string domain, List<DmarcRecordInfo> dmarcRecords, int messageSize)
        {
            DmarcRecords dmarcRecords1;

            if (dmarcRecords.Any())
            {
                string orgDomain = dmarcRecords.First().OrgDomain;
                bool isTld = dmarcRecords.First().IsTld;
                bool isInherited = dmarcRecords.First().IsInherited;

                DmarcRecord[] records =
                    await Task.WhenAll(dmarcRecords.Select(_ => _recordParser.Parse(domain, _)));

                dmarcRecords1 = new DmarcRecords(domain, records.Where(_ => _ != null).ToList(), messageSize, orgDomain, isTld, isInherited);
            }
            else
            {
                dmarcRecords1 = new DmarcRecords(domain, null, messageSize);
            }

            EvaluationResult<DmarcRecords> results =
                await _dmarcRecordsEvaluator.Evaluate(dmarcRecords1);

            dmarcRecords1.AddErrors(results.Errors);

            return dmarcRecords1;
        }
    }
}
