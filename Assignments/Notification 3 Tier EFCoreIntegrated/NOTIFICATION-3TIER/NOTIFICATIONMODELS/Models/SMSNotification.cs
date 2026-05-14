namespace NotificationModels.Models
{
    public class SmsNotification : Notification
    {
        public SmsNotification()
        {
            
        }
        // fields other than type is instantiated in parent
        public SmsNotification(string message, User recipient) : base(message, recipient)
        {
            // setting type to sms
            NotificationType = NotificationType.SMS;
        }

         // overridden method as per sms
        public override void Send()
        {
            Console.WriteLine("-------Sending SMS-------");
            Console.WriteLine($"To the user number : {Recipient.Phone}");
            Console.WriteLine($"The message texted is {Message}");
            Console.WriteLine($"SMS sent at {Sentdate}");
        }
        
    }
}