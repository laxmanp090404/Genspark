namespace NotificationApp.Models
{
    // define types in enums
    public enum NotificationType
    {
        Email = 1,
        SMS = 2
    }

    // has to be abstract as this should not be insantiated
    internal abstract class Notification
    {
        // properties of notification
        public string Message { get; set; } = string.Empty;
        public DateTime Sentdate { get; set; } = DateTime.Now;
        public User Recipient { get; set; }
        public NotificationType NotificationType { get; set; }

        public Notification(string message, User recipient)
        {
            Message = message;
            Recipient = recipient;
            Sentdate = DateTime.Now;
        }

        // email and smschild will implement
        public abstract void Send();
    }
}