namespace NotificationModels.Exceptions
{
    // custome exception for invalid user details
    public class InvalidUserDetailsException : Exception
    {
        public InvalidUserDetailsException(string issue):base(
            $"You have entered an invalid user detail with {issue}"
        )
        {
            
        }
    }
}