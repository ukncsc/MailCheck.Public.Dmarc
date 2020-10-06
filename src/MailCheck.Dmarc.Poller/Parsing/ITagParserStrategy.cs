using Tag = MailCheck.Dmarc.Poller.Domain.Tag;

using MailCheck.Dmarc.Contracts.SharedDomain;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public interface ITagParserStrategy
    {
        Tag Parse(string tag, string value);
        string Tag { get; }
        int MaxOccurences { get; }
    }
}