namespace MailCheck.Dmarc.Contracts.SharedDomain
{
    public class Percent : OptionalDefaultTag
    {
        public static Percent Default = new Percent("pct=100", 100, true);

        public Percent(string value, int? percentValue, bool allValid, bool isImplicit = false) 
            : base(TagType.Percent, value, allValid, isImplicit)
        {
            PercentValue = percentValue;
        }

        public int? PercentValue { get; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(PercentValue)}: {PercentValue}";
        }
    }
}