using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Poller.Dns;
using MailCheck.Dmarc.Poller.Domain;
using MailCheck.Dmarc.Poller.Implicit;
using MailCheck.Dmarc.Poller.Rules;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public interface IDmarcRecordParser
    {
        Task<DmarcRecord> Parse(string domain, DmarcRecordInfo dmarcRecordInfo);
    }

    public class DmarcRecordParser : IDmarcRecordParser
    {
        private const char Separator = ';';
        private readonly ITagParser _tagParser;
        private readonly IEvaluator<DmarcRecord> _evaluator;
        private readonly IImplicitProvider<Tag> _implicitProvider;

        public DmarcRecordParser(ITagParser tagParser,
            IEvaluator<DmarcRecord> evaluator,
            IImplicitProvider<Tag> implicitProvider)
        {
            _tagParser = tagParser;
            _evaluator = evaluator;
            _implicitProvider = implicitProvider;
        }

        public async Task<DmarcRecord> Parse(string domain, DmarcRecordInfo dmarcRecordInfo)
        {
            if (string.IsNullOrEmpty(dmarcRecordInfo.Record))
            {
                return null;
            }

            string[] stringTags = dmarcRecordInfo.Record.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries)
                .Select(_ => _.Trim()).ToArray();

            List<Tag> tags = _tagParser.Parse(stringTags.ToList());

            tags = tags.Concat(_implicitProvider.GetImplicitValues(tags)).ToList();

            DmarcRecord dmarcRecord = new DmarcRecord(dmarcRecordInfo.Record, tags,  domain, dmarcRecordInfo.OrgDomain,
                dmarcRecordInfo.IsTld, dmarcRecordInfo.IsInherited);
            EvaluationResult<DmarcRecord> result = await _evaluator.Evaluate(dmarcRecord);
            dmarcRecord.AddErrors(result.Errors);
            return dmarcRecord;
        }
    }
}
