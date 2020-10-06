using MailCheck.Common.Environment;
using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Spf.Client.Config
{
    public interface ISpfApiKeyConfig
    {
        string SpfClaimsName { get; }
    }

    internal class SpfApiKeyConfig : ISpfApiKeyConfig
    {
        public SpfApiKeyConfig(IEnvironment environment)
        {
            SpfClaimsName = environment.GetEnvironmentVariable("SpfClaimsName");
        }

        public string SpfClaimsName { get; }
    }
}