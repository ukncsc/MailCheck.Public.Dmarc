namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class MaxReportSize
    {
        public MaxReportSize(ulong? value, Unit unit, bool valid)
        {
            Value = value;
            Unit = unit;
            Valid = valid;
        }
        
        public ulong? Value { get; }
        public Unit Unit { get; }
        public bool Valid { get; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}, {nameof(Unit)}: {Unit}";
        }
    }
}