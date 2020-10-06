using System.Collections.Generic;

namespace MailCheck.Spf.Client.Domain
{
    public class SpfRecords
    {
        public List<SpfRecord> Records { get; set; }

        public int PayloadSizeBytes { get; set; }

        public List<Message> Messages { get; set; }
    }
}