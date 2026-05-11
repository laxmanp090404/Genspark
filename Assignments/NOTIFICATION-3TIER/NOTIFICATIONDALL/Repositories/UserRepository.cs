using NotificationModels.Models;
using Npgsql;

namespace NotificationDall.Repositories
{
    // data access for user
    public class UserRepository : AbstractRepository<int, User>
    {

        public override User CreateEntity(User user)
        {
            // get connection instance

            NpgsqlConnection connection = context.GetConnection();
            string createQuery = $"insert into users(name,email,phone) values('{user.Name}','{user.Email}','{user.Phone}')";
            NpgsqlCommand command = new NpgsqlCommand(createQuery, connection);
            try
            {
                // open connection and execute 

                connection.Open();
                //executescalar to get the generatedid
                int generatedid = Convert.ToInt32(command.ExecuteScalar());
                user.Id = generatedid;

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception at creating user " + e.Message);
            }
            finally
            {
                // close connection

                connection?.Close();
            }
            return user;
        }

        public override User? DeleteEntity(int id)
        {
            User? existinguser = GetEntityById(id);
            if (existinguser == null)
            {
                return null;
            }
            // get connection instance

            NpgsqlConnection connection = context.GetConnection();
            string deleteQuery = $"update users set isdeleted = true where id = {id}";
            NpgsqlCommand command = new NpgsqlCommand(deleteQuery, connection);
            try
            {
                // open connection and execute 

                connection.Open();
                int rowsaffected = command.ExecuteNonQuery();
                if (rowsaffected > 0) return existinguser;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error while deleting user" + e.Message);
            }
            finally
            {
                // close connection

                connection?.Close();
            }


            return null;
        }

        public override List<User>? GetAllEntity()
        {
            List<User> users = new();
            // get connection instance

            NpgsqlConnection connection = context.GetConnection();
            string listallusersQuery = $"select * from users where isdeleted = false";
            NpgsqlCommand command = new NpgsqlCommand(listallusersQuery, connection);
            try
            {
                // open connection and execute 

                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User user = new User(Convert.ToInt32(reader["id"]), reader["name"].ToString() ?? "", reader["email"].ToString() ?? "", reader["phone"].ToString() ?? "");
                    users.Add(user);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while getting all users " + e.Message);
            }
            finally
            {
                // close connection

                connection?.Close();
            }
            return users;
        }

        public override User? GetEntityById(int id)
        {
            // get connection instance

            NpgsqlConnection connection = context.GetConnection();
            string getbyidQuery = $"select * from users where id = '{id}' and isdeleted=false";
            NpgsqlCommand command = new NpgsqlCommand(getbyidQuery, connection);
            User? user = null;
            try
            {
                // open connection and execute 

                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user = new User(Convert.ToInt32(reader["id"]), reader["name"].ToString() ?? "", reader["email"].ToString() ?? "", reader["phone"].ToString() ?? "");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error while getting user by id " + e.Message);
            }
            finally
            {
                connection?.Close();
            }
            return user;

        }

        public override User? UpdateEntity(int id, User item)
        {
            User? existinguser = GetEntityById(id);
            if (existinguser == null) return null;
            // get connection instance

            NpgsqlConnection connection = context.GetConnection();
            string updateQuery = $"update users set name ='{item.Name}',email = '{item.Email}',phone = '{item.Phone}' where id = {id}";
            NpgsqlCommand command = new NpgsqlCommand(updateQuery, connection);
            try
            {
                // open connection and execute 

                connection.Open();
                int rowsaffected = command.ExecuteNonQuery();
                if (rowsaffected > 0) return item;
            }
            catch (Exception e)
            {

                Console.WriteLine("Error while updating user " + e.Message);
            }
            finally
            {
                // close connection

                connection?.Close();
            }
            return null;
        }
    }
}