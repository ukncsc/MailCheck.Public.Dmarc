namespace MailCheck.Dmarc.Poller.Domain
{
    public enum PolicyType
    {
        None,
        Quarantine,
        Reject,
        Unknown
    }
}