using System;
using System.Text.RegularExpressions;
using MailCheck.Dmarc.Poller.Domain;
using Version = MailCheck.Dmarc.Poller.Domain.Version;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public class VersionParserStrategy : ITagParserStrategy
    {
        private readonly Regex _regex = new Regex("DMARC1$", RegexOptions.IgnoreCase);

        public Guid Id => Guid.Parse("13FFAD1D-1953-4412-8CE6-460E5C3D7A41");

        public Tag Parse(string tag, string value)
        {
            Version version = new Version(tag);
            if (!_regex.IsMatch(value ?? string.Empty))
            {
                string errorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, Tag, value);
                string markdown = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, Tag, value);

                version.AddError(new Error(Id, "mailcheck.dmarc.invalidVersionValue", ErrorType.Error, errorMessage, markdown));
            }
            return version;
        }

        public string Tag => "v";

        public int MaxOccurences => 1;
    }
}