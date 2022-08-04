using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Rules
{
    public class MigrationRuaTagsShouldContainDmarcServiceMailBox : IRule<DmarcRecord>
    {
        private readonly Uri _dmarcMailbox;
        private readonly string _allowedRuaDomain;
        private readonly string _dmarcMailboxAddress;
        private readonly Uri _dmarcVerificationMailbox;
        private readonly string _dmarcVerificationMailboxAddress;

        public MigrationRuaTagsShouldContainDmarcServiceMailBox()
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
            string recordText = !record.Record.EndsWith(";") ? $"{record.Record};" : record.Record;

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
                string template = DmarcRulesMarkDownResource.MigrationRuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage;

                Regex ruaRegex = new Regex("rua=[^;]+;");

                string currentRuaTag = ruaRegex.Match(recordText).Value;
                string suggestedRuaTag = currentRuaTag.Replace(" ", "");

                foreach (Uri uri in mailCheckUris)
                {
                    suggestedRuaTag = suggestedRuaTag.Replace(uri.OriginalString, "");
                }

                suggestedRuaTag = suggestedRuaTag.Replace(",,", ",");
                suggestedRuaTag = suggestedRuaTag.Replace(",;", ";");
                suggestedRuaTag = suggestedRuaTag.Replace("=,", "=");

                string delimiter = reportUris.Count > mailCheckUris.Count ? "," : "";
                string mailcheckSuggestedRuaTag = suggestedRuaTag.Replace("rua=", $"rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk{delimiter}");
                string myncscSuggestedRuaTag = suggestedRuaTag.Replace("rua=", $"rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk{delimiter}");

                string mailcheckSuggestedRecord = recordText;
                string myncscSuggestedRecord = recordText;
                if (!string.IsNullOrEmpty(mailcheckSuggestedRecord) && !string.IsNullOrEmpty(myncscSuggestedRecord) && !string.IsNullOrEmpty(currentRuaTag) && !string.IsNullOrEmpty(mailcheckSuggestedRuaTag) && !string.IsNullOrEmpty(myncscSuggestedRuaTag))
                {
                    mailcheckSuggestedRecord = mailcheckSuggestedRecord.Replace(currentRuaTag, mailcheckSuggestedRuaTag);
                    myncscSuggestedRecord = myncscSuggestedRecord.Replace(currentRuaTag, myncscSuggestedRuaTag);
                }

                string markdown = string.Format(template, mailcheckSuggestedRecord, myncscSuggestedRecord, record.Domain);

                return new Message(Guid.Parse("030F78E8-EAA9-48EE-9458-93080AAB57B0"),
                    "mailcheck.dmarc.ruaTagShouldNotHaveMisconfiguredMailCheckMailbox",
                    MessageSources.DmarcEvaluator,
                    MessageType.error,
                    string.Format(
                        DmarcRulesResource.RuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage,
                        _dmarcVerificationMailboxAddress,
                        _dmarcVerificationMailbox.OriginalString),
                    markdown
                );
            }

            if (!mailCheckUris.Any() && otherAllowedRuaCount == 0)
            {
                string dmarcRecord = recordText;
                string mailcheckDmarcRecord;
                string myncscDmarcRecord;

                dmarcRecord = NeatenRecord(dmarcRecord);

                if (dmarcRecord.Contains("rua="))
                {
                    mailcheckDmarcRecord = dmarcRecord.Replace("rua=", $"rua={DmarcRulesResource.VerificationRuaMailbox},");
                    myncscDmarcRecord = dmarcRecord.Replace("rua=", $"rua={DmarcRulesResource.RuaMailbox},");
                }
                else
                {
                    mailcheckDmarcRecord = $"{dmarcRecord}rua={DmarcRulesResource.VerificationRuaMailbox};";
                    myncscDmarcRecord = $"{dmarcRecord}rua={DmarcRulesResource.RuaMailbox};";
                }

                dmarcRecord = dmarcRecord.TrimEnd();

                return new Message(Guid.Parse("6F1C3B66-CFB4-4CEB-BF6B-DC81637FC2B0"),
                    "mailcheck.dmarc.ruaTagsShouldContainDmarcServiceMailBox",
                    MessageSources.DmarcEvaluator,
                    MessageType.info,
                    string.Format(DmarcRulesResource.RuaTagsShouldContainDmarcServiceMailBoxErrorMessage,
                        _dmarcVerificationMailboxAddress,
                        _dmarcVerificationMailbox.OriginalString),
                    string.Format(DmarcRulesMarkDownResource.MigrationRuaTagsShouldContainDmarcServiceMailBoxErrorMessage,
                        mailcheckDmarcRecord,
                        myncscDmarcRecord,
                        record.Domain)
                );
            }

            if (reportUris.GroupBy(_ => _.OriginalString).Any(_ => _.Count() > 1))
            {
                return new Message(Guid.Parse("EBB3FE7C-E80A-48C0-B0F1-79C8E88F0F12"),
                    "mailcheck.dmarc.ruaTagShouldNotContainDuplicateUris",
                    MessageSources.DmarcEvaluator,
                    MessageType.warning,
                    DmarcRulesResource.RuaTagShouldNotContainDuplicateUrisErrorMessage,
                    DmarcRulesMarkDownResource.RuaTagShouldNotContainDuplicateUrisErrorMessage
                );
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
