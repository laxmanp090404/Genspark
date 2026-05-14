using NotificationModels.Models;
namespace NotificationBl.Interfaces
{
    // contract for services to be provided for Notification
    public interface INotificationService
    {
        // send Notification to user
        void SendNotificationToUser();

        // view all notifications
        List<Notification>? ViewAllNotifications();

        // view notifications sent for a user
        List<Notification>? ViewNotificationsByUserId(int userId);
    }
}