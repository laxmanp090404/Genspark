namespace UnderstandingOops.Exceptions
{
    internal class InvalidPhoneNumberException : Exception
    {
        private string message;


        public override string Message => message;
        public InvalidPhoneNumberException()
        {
            message = "Account was not created as your Phone number format was not right";
        }

        public InvalidPhoneNumberException(string phone)
        {
            message = $"Phone {phone} is not in the proper format";
        }
    }
}