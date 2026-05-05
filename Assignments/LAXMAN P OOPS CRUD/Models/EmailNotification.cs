namespace NotificationApp.Models
{
    internal class EmailNotification : Notification
    {
        // other fields set using parent class constructor
        public EmailNotification(string message, User recipient) : base(message, recipient)
        {
            // setting type to mail enum
            NotificationType = NotificationType.Email;
        }


        // overridden method as per email
        public override void Send()
        {
            Console.WriteLine("-------Sending Email-------");
            Console.WriteLine($"To the user email : {Recipient.Email}");
            Console.WriteLine($"The content of mail is {Message}");
            Console.WriteLine($"Mail sent at {Sentdate}");
        }
       
    }
}