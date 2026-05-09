using NotificationModels.Models;

namespace NotificationDall.Repositories
{
    // data access for notifications
    public class NotificationRepository 
        : AbstractRepository<int, Notification>
    {
        static int nextId = 1;

        public NotificationRepository()
        {
            entitylist = new Dictionary<int, Notification>();
        }

        public override Notification CreateEntity(Notification notification)
        {
            entitylist[nextId++] = notification;
            return notification;
        }
    }
}