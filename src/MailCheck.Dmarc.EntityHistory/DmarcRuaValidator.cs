using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace MailCheck.Dmarc.EntityHistory
{
    public interface IDmarcRuaValidator
    {
        RuaResult Validate(string record);
    }

    public class RuaResult
    {
        public RuaResult(bool valid, List<string> tokens)
        {
            Valid = valid;
            Tokens = tokens;
        }

        public bool Valid { get; }
        public List<string> Tokens { get;  }
    }

    public class DmarcRuaValidator : IDmarcRuaValidator
    {
        private const string VALID_RUA_EMAIL = "@dmarc-rua.mailcheck.service.ncsc.gov.uk";

        private readonly Regex _regex = new Regex("rua=*(.+?)(?:;|$)");

        public RuaResult Validate(string record)
        {
            List<string> validTokens = new List<string>();

            Match matches = _regex.Match(record.Trim());

            if (matches.Success)
            {
                string ruaValue = matches.Value.Substring(4);

                string[] emails = ruaValue.Split("mailto:");

                foreach (string email in emails)
                {
                    if (string.IsNullOrWhiteSpace(email)) continue;
                    
                    string token = email.TrimStart().TrimEnd(',', ';', ' ', '\t');
                    
                    if (token.ToLower().EndsWith(VALID_RUA_EMAIL))
                    {
                        token = token.Replace(VALID_RUA_EMAIL, string.Empty);

                        if(token.Length == 11){
                            validTokens.Add(token);
                        }
                    }
                }
            }

            return new RuaResult(validTokens.Count > 0, validTokens);
        }
    }
}
