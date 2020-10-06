using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dmarc.Poller.Domain;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public interface ITagParser
    {
        List<Tag> Parse(List<string> stringTags);
    }

    public class TagParser : ITagParser
    {
        private const char Separator = '=';
        private readonly Dictionary<string, ITagParserStrategy> _strategies;

        public TagParser(IEnumerable<ITagParserStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(x => x.Tag);
        }

        public List<Tag> Parse(List<string> stringTags)
        {
            List<Tag> tags = new List<Tag>();
            foreach (string stringTag in stringTags)
            {
                Parse(stringTag, tags);
            }
            return tags;
        }

        private void Parse(string stringTag, ICollection<Tag> tags)
        {
            if (string.IsNullOrEmpty(stringTag))
            {
                UnknownTag unknownTag = GetUnknownTag(string.Empty, string.Empty);
                tags.Add(unknownTag);
                return;
            }

            string[] tokens = stringTag.Split(Separator, 2, StringSplitOptions.RemoveEmptyEntries);
            string tagName = tokens.ElementAtOrDefault(0)?.ToLower() ?? string.Empty;
            string tagValue = tokens.ElementAtOrDefault(1) ?? string.Empty;

            if (_strategies.TryGetValue(tagName, out ITagParserStrategy strategy))
            {
                Tag tag = strategy.Parse(stringTag, tagValue);

                int tagOccurences = tags.Count(x => x.GetType() == tag.GetType());

                if (tagOccurences >= strategy.MaxOccurences)
                {
                    string message = string.Format(DmarcParserResource.TagShouldOccurNoMoreThanErrorMessage, strategy.Tag, GetOccurrences(strategy.MaxOccurences), tagOccurences + 1);
                    string markdown = string.Format(DmarcParserMarkdownResource.TagShouldOccurNoMoreThanErrorMessage, strategy.Tag, GetOccurrences(strategy.MaxOccurences), tagOccurences + 1);
                    tag.AddError(new Error(Guid.Parse("DA770F12-AFFD-4A64-A08C-C7747F915F63"), ErrorType.Error, message, markdown));
                }

                if (tag.AllValid && tagValue.Contains(Separator))
                {
                    string message = string.Format(DmarcParserResource.UnexpectedValueErrorMessage, tagValue, "term", stringTag);
                    string markdown = string.Format(DmarcParserMarkdownResource.UnexpectedValueErrorMessage, tagValue, "term", stringTag);
                    tag.AddError(new Error(Guid.Parse("96C64B4A-E857-45F0-A8D4-F2F502FDBA85"), ErrorType.Error, message, markdown));
                }

                tags.Add(tag);
            }
            else
            {
                UnknownTag unknownTag = GetUnknownTag(tokens.ElementAtOrDefault(0), tokens.ElementAtOrDefault(1));
                tags.Add(unknownTag);
            }
        }

        private static UnknownTag GetUnknownTag(string tag, string value)
        {
            UnknownTag unknownTag = new UnknownTag(tag, value);
            string message = string.Format(DmarcParserResource.UnknownTagErrorMessage, tag ?? "<null>", value ?? "<null>");
            unknownTag.AddError(new Error(Guid.Parse("D9DB4907-15B3-41FB-8929-2CFBF82DF71F"), ErrorType.Error, message, string.Empty));
            return unknownTag;
        }

        public string GetOccurrences(int occurrences)
        {
            return occurrences == 1 ? "once" : (occurrences == 2 ? "twice" : $"{occurrences} times");
        }
    }
}