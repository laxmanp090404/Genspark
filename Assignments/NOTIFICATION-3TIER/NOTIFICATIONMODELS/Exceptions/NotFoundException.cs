namespace NotificationModels.Exceptions
{
    // custom exception for entity not found
    public class NotFoundException : Exception
    {
        public NotFoundException(string entity) : base($"{entity} not found.")
        {
            
        }
    }
}