using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Dmarc.Poller.Domain
{
    public class DmarcPollResult
    {
        public DmarcPollResult(string id, params Error[] errors)
            : this(id, null, null, errors.ToList())
        {
        }

        public DmarcPollResult(DmarcRecords records, TimeSpan elapsed)
            : this(records.Domain, records, elapsed, null)
        {
        }

        private DmarcPollResult(string id, DmarcRecords records, TimeSpan? elapsed, List<Error> errors)
        {
            Records = records ?? new DmarcRecords(id, new List<DmarcRecord>(), 0);
            Elapsed = elapsed;
            Errors = errors ?? new List<Error>();
        }

        public DmarcRecords Records { get; }
        public TimeSpan? Elapsed { get; }
        public List<Error> Errors { get; }
    }
}