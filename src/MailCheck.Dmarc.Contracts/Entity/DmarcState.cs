namespace MailCheck.Dmarc.Contracts.Entity
{
    public enum DmarcState
    {
        Created,
        PollPending,
        EvaluationPending,
        Unchanged,
        Evaluated
    }
}