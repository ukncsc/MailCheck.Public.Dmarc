using System;
using System.Collections.Generic;
using System.Text;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dmarc.Contracts.History
{
    public class DmarcRecordChanged : Message
    {
        public DmarcRecordChanged(string id) : base(id)
        {
        }
    }
}
