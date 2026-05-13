using GameApp.DataAccessLayer.Context;
using GameApp.DataAccessLayer.Interfaces;
using GameApp.ModelLayer.Models;
using Npgsql;

namespace GameApp.DataAccessLayer.Repositories
{
    internal class UserRepository : IUserRepository
    {
        DbContext context;
        public UserRepository()
        {
            context = new DbContext();
        }
        public User? CreateUser(User item)
        {
            //created connection
            NpgsqlConnection connection = context.GetConnection();
            // parameterised for preventing sql injection
            string createQuery = @"
                                insert into users
                                (username,password)
                                values
                                (@username,@password)
                                returning id;
                                ";

            NpgsqlCommand command =
            new NpgsqlCommand(createQuery, connection);

            command.Parameters.AddWithValue
            ("@username", item.Username);

            command.Parameters.AddWithValue
            ("@password", item.Password);
            try
            {
                connection.Open();
                int id = Convert.ToInt32(command.ExecuteScalar());
                item.Id = id;
                if (id > 0) return item;

            }
            catch (Exception e)
            {

                System.Console.WriteLine("Error while creating user " + e.Message);
            }
            finally
            {
                connection?.Close();
            }
            return null;
        }

        public User? GetUserByUserName(string username)
        {
            NpgsqlConnection connection = context.GetConnection();
            // filter only active users and matching username
            string query = @"
                            select *
                            from users
                            where username = @username
                            and is_active = true;
                            ";

            NpgsqlCommand command =
            new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue
            ("@username", username);
            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new User(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Erro while getting user by his/her username " + e.Message);
            }
            finally
            {
                connection?.Close();
            }
            return null;
        }
    }
}