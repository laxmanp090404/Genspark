
using NotificationModels.Exceptions;
namespace NotificationBl.Validators
{
    public static class MessageValidator
    {
        // extension methods
        public static void ValidateMessage(this string message)
        {
            // genereal exceptions in messages
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new InvalidMessageException("Empty message");
            }
            if (message.Length < 5)
            {
                throw new InvalidMessageException("Message shorter than 5 characters");
            }
           
        }
        
        // check sms length
        public static void ValidateSMS(this string sms)
        {
             if(sms.Length > 160)
            {
                throw new InvalidMessageException("SMS too long");
            }
        }
        
    }
}