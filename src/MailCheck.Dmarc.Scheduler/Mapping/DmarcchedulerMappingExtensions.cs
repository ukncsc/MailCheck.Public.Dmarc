using MailCheck.Dmarc.Contracts.Scheduler;
using MailCheck.Dmarc.Scheduler.Dao.Model;

namespace MailCheck.Dmarc.Scheduler.Mapping
{
    public static class DmarcchedulerMappingExtensions
    {
        public static DmarcRecordExpired ToDmarcRecordExpiredMessage(this DmarcSchedulerState state) =>
            new DmarcRecordExpired(state.Id);
    }
}
