
using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Spf.Client.Config
{
    public interface ISpfClientConfig
    {
        string SpfApiEndpoint { get; }
    }

    internal class SpfClientConfig : ISpfClientConfig
    {
        public SpfClientConfig(IEnvironment environment)
        {
            SpfApiEndpoint = environment.GetEnvironmentVariable("SpfApiEndpoint");
        }

        public string SpfApiEndpoint { get; }
    }
}