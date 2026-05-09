namespace NotificationModels.Exceptions
{
    public class InvalidMessageException : Exception
    {

        // custom exception for message issues
        public InvalidMessageException(string issue) : base(
            $"You have entered an invalid message with {issue}"
        )
        {

        }
    }
}