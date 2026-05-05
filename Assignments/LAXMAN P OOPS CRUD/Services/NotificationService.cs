using NotificationApp.Interfaces;
using NotificationApp.Models;

namespace NotificationApp.Services
{
    internal class NotificationService : INotificationInteract
    {
        IUserService service ;

        public NotificationService(IUserService userService)
        {
            service = userService;
        }

        public void SendNotificationToUser()
        {
            // getting notification details
            Console.WriteLine("Enter user id:");
            int id = int.Parse(Console.ReadLine() ?? "0");
            List<User> userlist = service.GetUsers()??[];
            var user = userlist.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                Console.WriteLine("User not found");
                return;
            }

            Console.WriteLine("Enter message:");
            string message = Console.ReadLine() ?? "";

            Console.WriteLine("Choose notification type: 1.Email 2.SMS");
            int choice = int.Parse(Console.ReadLine() ?? "0");

            Notification notif;

            if (choice == 1)
                notif = new EmailNotification(message, user);
            else if(choice == 2)
                notif = new SmsNotification(message, user);
            else
            {
                Console.WriteLine("It is an invalid choice");
                return;
            }
            // polymorphic behaviour ie if email obj invoke email send and vice versa with sms
            notify( notif);
        }

        private void notify(Notification notifobj)
        {

            notifobj.Send();
        }
    }
}