using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Rules
{
    public class RuaTagsShouldContainDmarcServiceMailBox : IRule<DmarcRecord>
    {
        private readonly Uri _dmarcMailbox;
        private readonly string _allowedRuaDomain;
        private readonly string _dmarcMailboxAddress;
        private readonly Uri _dmarcVerificationMailbox;
        private readonly string _dmarcVerificationMailboxAddress;

        public RuaTagsShouldContainDmarcServiceMailBox()
        {
            _dmarcMailbox = new Uri(DmarcRulesResource.RuaMailbox);
            _dmarcVerificationMailbox = new Uri(DmarcRulesResource.VerificationRuaMailbox);
            _allowedRuaDomain = DmarcRulesResource.AllowedRuaDomain;
            _dmarcMailboxAddress = $"{_dmarcMailbox.UserInfo}@{_dmarcMailbox.Host}";
            _dmarcVerificationMailboxAddress = $"{_dmarcVerificationMailbox.UserInfo}@{_dmarcVerificationMailbox.Host}";
        }

        public Task<List<Message>> Evaluate(DmarcRecord t)
        {
            List<Message> messages = new List<Message>();
            Message message = GetMessage(t);
            if (message != null)
            {
                messages.Add(message);
            }
            return Task.FromResult(messages);
        }

        public Message GetMessage(DmarcRecord record)
        {
            List<ReportUriAggregate> reportUriAggregates = record.Tags.OfType<ReportUriAggregate>().ToList();

            // If we have duplicate entries for the same tag
            // There is already an error so disable this rule
            if (reportUriAggregates.Count > 1)
            {
                return null;
            }

            List<Uri> reportUris = reportUriAggregates.FirstOrDefault()?.Uris?
                                       .Select(_ => _.Uri.Uri).Where(_ => _ != null).ToList() ?? new List<Uri>();

            List<Uri> mailCheckUris = reportUris.Where(_ => _.Authority == _dmarcMailbox.Authority).ToList();
            List<Uri> unexpectedMailCheckUris = mailCheckUris.Where(_ => _?.UserInfo != _dmarcMailbox.UserInfo).ToList();
            int otherAllowedRuaCount = reportUris.Count(_ => _.Authority == _allowedRuaDomain);

            if (unexpectedMailCheckUris.Any() && otherAllowedRuaCount == 0)
            {
                string template = DmarcRulesMarkDownResource.RuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage;

                Regex ruaRegex = new Regex("rua=[^;]+;");

                string currentRuaTag = ruaRegex.Match(record.Record).Value;
                string suggestedRuaTag = currentRuaTag.Replace(" ", "");

                foreach (Uri uri in mailCheckUris)
                {
                    suggestedRuaTag = suggestedRuaTag.Replace(uri.OriginalString, "");
                }

                suggestedRuaTag = suggestedRuaTag.Replace(",,", ",");
                suggestedRuaTag = suggestedRuaTag.Replace(",;", ";");
                suggestedRuaTag = suggestedRuaTag.Replace("=,", "=");

                string delimiter = reportUris.Count > mailCheckUris.Count ? "," : "";
                suggestedRuaTag = suggestedRuaTag.Replace("rua=", $"rua=mailto:{_dmarcVerificationMailboxAddress}{delimiter}");

                string suggestedRecord = record.Record;
                if (!string.IsNullOrEmpty(suggestedRecord) && !string.IsNullOrEmpty(currentRuaTag) && !string.IsNullOrEmpty(suggestedRuaTag))
                {
                    suggestedRecord = suggestedRecord.Replace(currentRuaTag, suggestedRuaTag);
                }

                string markdown = string.Format(template, suggestedRecord, record.Domain);

                return new Message(Guid.Parse("85DB50C5-E3DA-479A-A9CE-1B07C81C3747"), MessageSources.DmarcEvaluator, MessageType.error, string.Format(
                        DmarcRulesResource.RuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage,
                        _dmarcVerificationMailboxAddress,
                        _dmarcVerificationMailbox.OriginalString), markdown
                    );
            }

            if (!mailCheckUris.Any() && otherAllowedRuaCount == 0)
            {
                string dmarcRecord = record.Record;

                dmarcRecord = NeatenRecord(dmarcRecord);

                if (dmarcRecord.Contains("rua="))
                {
                    dmarcRecord = dmarcRecord.Replace("rua=", $"rua={DmarcRulesResource.VerificationRuaMailbox},");
                }
                else
                {
                    dmarcRecord = $"{dmarcRecord}rua={DmarcRulesResource.VerificationRuaMailbox};";
                }

                dmarcRecord = dmarcRecord.TrimEnd();

                return new Message(Guid.Parse("045C9D62-8771-4D8F-B981-EAC70C7B74A2"), MessageSources.DmarcEvaluator, MessageType.warning, string.Format(DmarcRulesResource.RuaTagsShouldContainDmarcServiceMailBoxErrorMessage,
                        _dmarcVerificationMailboxAddress,
                        _dmarcVerificationMailbox.OriginalString),
                    string.Format(DmarcRulesMarkDownResource.RuaTagsShouldContainDmarcServiceMailBoxErrorMessage, dmarcRecord, record.Domain));

            }

            if (reportUris.GroupBy(_ => _.OriginalString).Any(_ => _.Count() > 1))
            {
                return new Message(Guid.Parse("354DDFCE-B9DE-4C3B-93BD-A120408ED94A"), MessageSources.DmarcEvaluator, MessageType.warning,
                    DmarcRulesResource.RuaTagShouldNotContainDuplicateUrisErrorMessage, DmarcRulesMarkDownResource.RuaTagShouldNotContainDuplicateUrisErrorMessage);
            }

            return null;
        }

        private string NeatenRecord(string dmarcRecord)
        {
            string[] elements = dmarcRecord.Split(";", StringSplitOptions.RemoveEmptyEntries);

            IEnumerable<string> neatElements = elements
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim());

            string neatRecord = string.Join("; ", neatElements);

            return $"{neatRecord}; ";
        }

        public int SequenceNo => 5;
        public bool IsStopRule => false;
    }
}
