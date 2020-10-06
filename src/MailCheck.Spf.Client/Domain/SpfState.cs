namespace MailCheck.Spf.Client.Domain
{
    public enum SpfState
    {
        Created,
        PollPending,
        EvaluationPending,
        Unchanged,
        Evaluated
    }
}