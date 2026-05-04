namespace NotificationApp.Interfaces
{
   // force the service to include mentioned methods
    internal interface INotificationInteract
    {
        void CreateUser();
        void SendNotificationToUser();
        void ShowAllUsers();
    }
}