namespace MailCheck.Dmarc.Entity.Entity.Notifiers
{
    public interface IChangeNotifier
    {
        void Handle(DmarcEntityState state, Common.Messaging.Abstractions.Message message);
    }
}