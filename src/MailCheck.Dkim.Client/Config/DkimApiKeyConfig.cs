using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dkim.Client.Config
{
    public interface IDkimApiKeyConfig
    {
        string DkimClaimsName { get; }
    }

    internal class DkimApiKeyConfig : IDkimApiKeyConfig
    {
        public DkimApiKeyConfig(IEnvironment environment)
        {
            DkimClaimsName = environment.GetEnvironmentVariable("DkimClaimsName");
        }

        public string DkimClaimsName { get; }
    }
}