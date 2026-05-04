using NotificationApp.Interfaces;
using NotificationApp.Services;

namespace NotificationApp
{
    internal class Program
    {
        INotificationInteract notificationInteract;

        public Program()
        {
            notificationInteract = new NotificationService();
        }

        void Run()
        {
            bool exitMenu = false;

            while (!exitMenu)
            {
                int choice;

                Console.WriteLine("1.Create User");
                Console.WriteLine("2.Show All Users");
                Console.WriteLine("3.Send Notification");
                Console.WriteLine("4.Exit");
                Console.WriteLine("Please enter your choice of service :");

                while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 4)
                {
                    Console.WriteLine("Enter valid option");
                }

                switch (choice)
                {
                    case 1:
                        notificationInteract.CreateUser();
                        break;
                    case 2:
                        notificationInteract.ShowAllUsers();
                        break;
                    case 3:
                        notificationInteract.SendNotificationToUser();
                        break;
                    case 4:
                        exitMenu = true;
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}