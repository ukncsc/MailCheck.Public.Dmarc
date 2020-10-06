using MailCheck.Dmarc.Poller.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Dmarc.Poller.Parsing
{
    public class FailureOptionsParserStrategy : ITagParserStrategy
    {
        public Guid Id => Guid.Parse("5CFA18E9-D482-4769-B5B3-0C01764A466C");

        public Tag Parse(string tag, string value)
        {
            string[] foValues = value?.Split(":");

            List<FailureOptionType> failureOptionType = new List<FailureOptionType>();

            if (foValues == null)
            {
                failureOptionType.Add(FailureOptionType.Unknown);
            }
            else
            {
                foreach (var foValue in foValues)
                {
                    switch (foValue?.ToLower())
                    {
                        case "0":
                            failureOptionType.Add(FailureOptionType.Zero);
                            break;
                        case "1":
                            failureOptionType.Add(FailureOptionType.One);
                            break;
                        case "d":
                            failureOptionType.Add(FailureOptionType.D);
                            break;
                        case "s":
                            failureOptionType.Add(FailureOptionType.S);
                            break;
                        default:
                            failureOptionType.Add(FailureOptionType.Unknown);
                            break;
                    }
                }
            }

            FailureOption failureOption = new FailureOption(tag, failureOptionType);
            
            if (failureOptionType.Any(x => x == FailureOptionType.Unknown))
            {
                string errorMessage = string.Format(DmarcParserResource.InvalidValueErrorMessage, Tag, value);
                string markDown = string.Format(DmarcParserMarkdownResource.InvalidValueErrorMessage, Tag, value);
                failureOption.AddError(new Error(Id, ErrorType.Error, errorMessage, markDown));
            }

            return failureOption;
        }

        public string Tag => "fo";

        public int MaxOccurences => 1;
    }
}