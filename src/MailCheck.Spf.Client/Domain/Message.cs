namespace MailCheck.Spf.Client.Domain
{
    public class Message
    {
        public string Source { get; set; }

        public MessageType MessageType { get; set; }

        public string Text { get; set; }
    }
}