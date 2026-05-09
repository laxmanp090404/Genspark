using NotificationModels.Models;
namespace NotificationBl.Interfaces
{
    // contract for services to be provided for Notification
    public interface INotificationService
    {
        void SendNotificationToUser();

        List<Notification>? ViewAllNotifications();

        List<Notification>? ViewNotificationsByUserId(int userId);
    }
}