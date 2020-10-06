using System;
using System.Collections.Generic;
using System.Text;
using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dkim.Client.Config
{
    public interface IDkimClientConfig
    {
        string DkimApiEndpoint { get; }
    }

    internal class DkimClientConfig : IDkimClientConfig
    {
        public DkimClientConfig(IEnvironment environment)
        {
            DkimApiEndpoint = environment.GetEnvironmentVariable("DkimApiEndpoint");
        }
        public string DkimApiEndpoint { get; }
    }
}
