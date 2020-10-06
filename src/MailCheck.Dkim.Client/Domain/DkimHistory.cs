using System.Collections.Generic;
using Newtonsoft.Json;

namespace MailCheck.Dkim.Client.Domain
{
    public class DkimHistory
    {
        public DkimHistory(string id, List<DkimHistoryItem> history)
        {
            Id = id;
            History = history ?? new List<DkimHistoryItem>();
        }

        public string Id { get; }

        [JsonProperty("records")]
        public List<DkimHistoryItem> History { get; }
    }
}