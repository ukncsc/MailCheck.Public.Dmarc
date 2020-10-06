using System.Collections.Generic;
using Newtonsoft.Json;

namespace MailCheck.Spf.Client.Domain
{
    public class SpfHistory
    {
        public SpfHistory(string id, List<SpfHistoryItem> history)
        {
            Id = id;
            History = history;
        }

        public string Id { get; }

        [JsonProperty("SpfHistory")]
        public List<SpfHistoryItem> History { get; }
    }
}