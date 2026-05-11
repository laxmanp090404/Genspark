using System.Globalization;
using NotificationModels.Models;
using Npgsql;

namespace NotificationDall.Repositories
{
    // data access for notifications
    public class NotificationRepository
        : AbstractRepository<int, Notification>
    {
        public override Notification CreateEntity(Notification notification)
        {

            // get connection instance
            NpgsqlConnection connection = context.GetConnection();

            // formatting date for inserting into postgres db
            string formattedDate =notification.Sentdate.ToString("yyyy-MM-dd HH:mm:ss");
            string createQuery = $"insert into notifications (message,notificationtype,sentdate,recipientid) values('{notification.Message}','{notification.NotificationType}','{formattedDate}','{notification.Recipient.Id}')";
            NpgsqlCommand command = new NpgsqlCommand(createQuery, connection);

            try
            {
                // open connection and execute 
                connection.Open();
                command.ExecuteNonQuery();

            }
            catch (Exception e)
            {

                Console.WriteLine("Error while creating notification " + e.Message);
            }
            finally
            {
                // close connection
                connection?.Close();
            }
            return notification;
        }


        public override List<Notification>? GetAllEntity()
        {
            // list to manage notifications
            List<Notification> notifications = new();
                        // get connection instance
            NpgsqlConnection connection = context.GetConnection();
            string selectallquery = $"select n.message,n.sentdate,n.notificationtype,u.id,u.name,u.email,u.phone,u.isdeleted from notifications n left join users u on n.recipientid = u.id";

            NpgsqlCommand command = new NpgsqlCommand(selectallquery, connection);
            try
            {
                                // open connection and execute 

                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string username =
                        Convert.ToBoolean(reader["isdeleted"])
                        ? "Deleted User"
                        : reader["name"].ToString() ?? "";

                    User user = new User(
                        Convert.ToInt32(reader["id"]),
                        username,
                        reader["email"].ToString() ?? "",
                        reader["phone"].ToString() ?? ""
                    );

                    Notification notification;

                    string type =
                        reader["notificationtype"].ToString() ?? "";

                    if (type == "Email")
                    {
                        notification =
                            new EmailNotification(reader["message"].ToString() ?? "", user);
                    }
                    else
                    {
                        notification = new SmsNotification(reader["message"].ToString() ?? "", user);
                    }

                    notifications.Add(notification);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error while fetching all notifications " + e.Message);
            }
            finally
            {
                                // close connection

                connection?.Close();
            }
            return notifications;
        }



        public override Notification? GetEntityById(int id)
        {
            // dont allow viewing notification by its id
            return null;
        }

        public override Notification? UpdateEntity(int id, Notification item)
        {
            // dont allow update notification
            return null;
        }
        public override Notification? DeleteEntity(int id)
        {
            // dont allow delete notification
            return null;
        }

    }
}