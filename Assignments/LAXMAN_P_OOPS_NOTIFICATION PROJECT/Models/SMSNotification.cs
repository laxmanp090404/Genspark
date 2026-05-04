namespace NotificationApp.Models
{
    internal class SmsNotification : Notification
    {
        // fields other than type is instantiated in parent
        public SmsNotification(string message, User recipient) : base(message, recipient)
        {
            // setting type to sms
            NotificationType = NotificationType.SMS;
        }

         // overridden method as per sms
        public override void Send(string message)
        {
            Console.WriteLine("-------Sending SMS-------");
            Console.WriteLine($"To the user number : {Recipient.Phone}");
            Console.WriteLine($"The message texted is {message}");
            Console.WriteLine($"Mail sent at {Sentdate}");
        }
        
    }
}