using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Poller.Domain
{
    public class MaxReportSize : DmarcEntity
    {
        public MaxReportSize(ulong? value, Unit unit)
        {
            Value = value;
            Unit = unit;
        }
        
        public ulong? Value { get; }
        public Unit Unit { get; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}, {nameof(Unit)}: {Unit}";
        }
    }
}