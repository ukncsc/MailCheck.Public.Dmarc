using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Rules
{
    public class RufTagShouldNotContainDmarcServiceMailBox : IRule<DmarcRecord>
    {
        private readonly Uri _dmarcMailbox;

        public RufTagShouldNotContainDmarcServiceMailBox()
        {
            _dmarcMailbox = new Uri(DmarcRulesResource.RuaMailbox);
        }

        public Task<List<Message>> Evaluate(DmarcRecord record)
        {
            List<ReportUriForensic> reportUriForensics = record.Tags.OfType<ReportUriForensic>().ToList();

            List<Message> messages = new List<Message>();

            // If we have duplicate entries for the same tag
            // There is already an error so disable this rule
            if (reportUriForensics.Count == 1)
            {
                List<Uri> reportUris = reportUriForensics.FirstOrDefault()?.Uris?
                                           .Select(_ => _.Uri.Uri).Where(_ => _ != null).ToList() ?? new List<Uri>();

                if (reportUris.Any(_ => _.Authority == _dmarcMailbox.Authority))
                {
                    messages.Add(new Message(Guid.Parse("019e94a9-143c-4041-8bc5-98a0339cf674"),
                        "mailcheck.dmarc.rufTagShouldNotContainDmarcServiceMailBox",
                        MessageSources.DmarcEvaluator,
                        MessageType.info,
                        DmarcRulesResource.RufTagShouldNotContainDmarcServiceMailBoxErrorMessage,
                        null)
                    );
                }
            }

            return Task.FromResult(messages);
        }

        public int SequenceNo => 6;
        public bool IsStopRule => false;
    }
}
