namespace MailCheck.Dmarc.Poller.Domain
{
    public enum Unit
    {
        B, //byte default if none spec'ed
        K, 
        M,
        G,
        T,
        Unknown
    }
}