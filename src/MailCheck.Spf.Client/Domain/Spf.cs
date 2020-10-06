using System;
using System.Collections.Generic;

namespace MailCheck.Spf.Client.Domain
{
    public class Spf
    {
        public string Id { get; set; }

        public SpfState Status { get; set; }

        public SpfRecords SpfRecords { get; set; }

        public List<Message> Messages { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}