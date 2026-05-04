namespace NotificationApp.Models
{
    // define types in enums
    public enum NotificationType
    {
        Email = 1,
        SMS = 2
    }

    internal class Notification
    {
        // properties of notification
        public string Message { get; set; }
        public DateTime Sentdate { get; set; }
        public User Recipient { get; set; }
        public NotificationType NotificationType { get; set; }

        public Notification(string message, User recipient)
        {
            Message = message;
            Recipient = recipient;
            Sentdate = DateTime.Now;
        }

        // base method which will be override in email and smschild
        public virtual void Send(string message)
        {
                Console.WriteLine($"Sending {NotificationType} to {Recipient.Name}: {message}");
        }
    }
}