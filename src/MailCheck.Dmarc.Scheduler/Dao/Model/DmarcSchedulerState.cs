namespace MailCheck.Dmarc.Scheduler.Dao.Model
{
    public class DmarcSchedulerState
    {
        public DmarcSchedulerState(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
    }
}
