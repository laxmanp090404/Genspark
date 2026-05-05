namespace NotificationApp.Interfaces
{
   // force the service to include mentioned methods
    internal interface INotificationInteract
    {
        // only sendnotification handled by Notification
        void SendNotificationToUser();
        
    }
}