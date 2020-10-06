using System;
using System.Collections.Generic;
using System.Text;

namespace MailCheck.Dmarc.Poller.Exceptions
{
    public class DmarcPollerException : System.Exception
    {
        public DmarcPollerException()
        {
        }

        public DmarcPollerException(string formatString, params object[] values)
            : base(string.Format(formatString, values))
        {
        }

        public DmarcPollerException(string message)
            : base(message)
        {
        }
    }
}
