namespace NotificationModels.Models{
    // define types in enums
    public enum NotificationType
    {
        Email = 1,
        SMS = 2
    }

    // has to be abstract as this should not be insantiated
    public abstract class Notification
    {
        // primary key for notification
        public int Id {get;set;}
        // properties of notification
        public string Message { get; set; } = string.Empty;
        public DateTime Sentdate { get; set; } = DateTime.Now;
        public NotificationType NotificationType { get; set; }
        // foreign key
        public int RecipientId {get;set;}
        // navigation property
        public User? Recipient { get; set; }

        public Notification()
        {
           
        }
        public Notification(string message, User recipient)
        {
            Message = message;
            Recipient = recipient;
            Sentdate = DateTime.Now;
        }

        // email and smschild will implement
        public abstract void Send();

        public override string ToString()
        {
            return
                $"--------------------\n" +
                $"Type       : {NotificationType}\n" +
                $"Message    : {Message}\n" +
                $"Sent Date  : {Sentdate:dd-MM-yyyy hh:mm tt}\n" +
                $"Recipient  : {Recipient?.Name}\n" +
                $"Recipient ID : {Recipient?.Id}";
        }
    }
}