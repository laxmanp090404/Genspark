using Npgsql;
namespace NotificationDall.Context
{
    public class DbContext
    {
        // connectionString
        private string connectionString =
            "Host=localhost;Username=laxmanp;Database=notificationdb";

        // return an instance of connection upon call
        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString);
        }
    }
}