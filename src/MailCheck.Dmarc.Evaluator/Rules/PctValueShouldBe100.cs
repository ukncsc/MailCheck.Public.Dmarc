using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts;
using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Evaluator.Rules
{
    public class PctValueShouldBe100 : IRule<DmarcRecord>
    {
        public Guid Id => Guid.Parse("5577A64F-11AF-475D-9040-4C573780852C");

        public Task<List<Message>> Evaluate(DmarcRecord record)
        {
            Percent percent = record.Tags.OfType<Percent>().FirstOrDefault();

            List<Message> messages = new List<Message>();

            //when pct value is null there will be parser error so dont add more errors
            if (percent?.PercentValue != null && percent.PercentValue != 100)
            {
                string errorMessage = string.Format(DmarcRulesResource.PctValueShouldBe100ErrorMessage, percent.PercentValue);
                string markDown = DmarcRulesMarkDownResource.PctValueShouldBe100ErrorMessage;
                messages.Add(new Message(Id, "mailcheck.dmarc.pctValueShouldBe100", MessageSources.DmarcEvaluator, MessageType.warning, errorMessage, markDown));
            }

            return Task.FromResult(messages);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}