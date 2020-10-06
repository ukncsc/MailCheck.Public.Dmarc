using System;
using System.Collections.Generic;
using System.Text;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dmarc.Contracts.Scheduler
{
    public class DmarcRecordExpired : Message
    {
        public DmarcRecordExpired(string id)
            : base(id) { }
    }
}
