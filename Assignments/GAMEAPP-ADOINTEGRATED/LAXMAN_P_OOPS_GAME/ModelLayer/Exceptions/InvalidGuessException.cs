namespace GameApp.ModelLayer.Exceptions
{
    internal class InvalidGuessException : Exception
    {
        // get custom message and attaches to virtual Method property
        public InvalidGuessException(string message)
            : base(message)
        {
        }
    }
}