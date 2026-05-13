namespace GameApp.ModelLayer.Exceptions
{
    internal class InvalidAppOperationException : Exception
    {
        // get custom message and attaches to virtual Method property
        public InvalidAppOperationException(string message)
            : base(message)
        {
        }
    }
}