using System.Data;
using Npgsql;

namespace GameApp.DataAccessLayer.Context
{
    public class DbContext
    {
        private readonly string connectionString ="Host=localhost;Username=laxmanp;Database=WordgameDatabase";

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString);
        }
    }
}