using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NotificationDall.Context;
using NotificationModels.Models;
using Npgsql;

namespace NotificationDall.Repositories
{
    // data access for notifications
    public class NotificationRepository
        : AbstractRepository<int, Notification>
    {
        public NotificationRepository(NotificationContext _context) : base(_context)
        {
            
        }
        public override Notification? CreateEntity(Notification notification)
        {
            try
            {
                // add notification to the dbset and save changes
                context.Notifications.Add(notification);
                // save changes and automatically adds Id to notification
                context.SaveChanges();
                // return notification
                return notification;
            }
            catch (Exception e)
            {

                Console.WriteLine("Error while creating notification " + e.Message);
            }
            return null;
        }


        public override List<Notification>? GetAllEntity()
        {
            // list to manage notifications
            List<Notification> notifications = new();

            try
            {
                //get notifications from dbset 
                //eager loading 
                notifications = context.Notifications
                                                   .Include(n => n.Recipient).ToList();// eager loading the recipient

                //renaming deleted user for UI
                foreach (var notification in notifications)
                {
                    if (notification.Recipient!.IsDeleted)
                    {
                        notification.Recipient.Name = "Deleted User";
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error while fetching all notifications " + e.Message);
            }
            // returning the list
            return notifications;
        }



        public override Notification? GetEntityById(int id)
        {
            // dont allow viewing notification by its id ie intentionally restricted as my business logic
            return null;
        }

        public override Notification? UpdateEntity(int id, Notification item)
        {
            // dont allow update notification ie intentionally restricted as my business logic
            return null;
        }
        public override Notification? DeleteEntity(int id)
        {
            // dont allow delete notification ie intentionally restricted as my business logic
            return null;
        }

    }
}