using NotificationDall.Interfaces;
using NotificationBl.Interfaces;
using NotificationDall.Repositories;
using NotificationBl.Services;

namespace NotificationApp
{
    public class Program
    {
        // service instances
        INotificationService notificationService;
        IUserService userService;

        public Program()
        {
            // initialising repos and services
            UserRepository repo = new UserRepository();

            userService = new UserService(repo);
            NotificationRepository notifrepo = new NotificationRepository();
            notificationService = new NotificationService(notifrepo, userService);
        }

        // app run method
        void Run()
        {
            bool exitMenu = false;

            while (!exitMenu)
            {
                // looping menu
                Console.WriteLine("1. Create User");
                Console.WriteLine("2. Show All Users");
                Console.WriteLine("3. View User By ID");
                Console.WriteLine("4. Update User");
                Console.WriteLine("5. Delete User");
                Console.WriteLine("6. Send Notification");
                Console.WriteLine("7. View All Notifications");
                Console.WriteLine("8.View Notification for a particular User by his/her Id : ");
                Console.WriteLine("9. Exit");
                Console.Write("Enter your choice: ");

                int choice;
                // prompt user to enter valid choice
                while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 9)
                {
                    Console.WriteLine("Please enter a valid choice : ");

                }

                try
                {

                    switch (choice)
                    {
                        case 1:
                            var createdUser = userService.CreateUser();

                            if (createdUser != null)
                                Console.WriteLine($"User Created Successfully : {createdUser}");
                            else
                                Console.WriteLine("User not created .Please Try again");

                            break;

                        case 2:
                            var users = userService.GetUsers();

                            if (users == null || users.Count == 0)
                            {
                                Console.WriteLine("No users found.");
                            }
                            else
                            {
                                Console.WriteLine("\nAll Users:");
                                users.ForEach(Console.WriteLine);
                            }
                            break;

                        case 3:
                            var foundUser = userService.GetUser();

                            if (foundUser == null)
                                Console.WriteLine("User not found.");
                            else
                                Console.WriteLine(foundUser);
                            break;

                        case 4:
                            var updatedUser = userService.UpdateUser();

                            if (updatedUser == null)
                                Console.WriteLine("Update failed.Please try again");
                            else
                            {
                                Console.WriteLine("User Updated Successfully:");
                                Console.WriteLine(updatedUser);
                            }
                            break;

                        case 5:
                            var deletedUser = userService.RemoveUser();

                            if (deletedUser == null)
                                Console.WriteLine("Delete failed.Please try again");
                            else
                            {
                                Console.WriteLine("User Deleted Successfully:");
                                Console.WriteLine(deletedUser);
                            }
                            break;

                        case 6:

                            notificationService.SendNotificationToUser();
                            break;

                        case 7:
                            var notifications = notificationService.ViewAllNotifications();
                            if (notifications == null || notifications.Count == 0)
                            {
                                Console.WriteLine("No notifications sent yet");
                            }
                            else
                            {
                                foreach (var notif in notifications)
                                {
                                    Console.WriteLine(notif);
                                }
                            }
                            break;
                        case 8:
                            Console.WriteLine("Please enter Id of User whose notification you wish to see");
                            int id;
                            while (!int.TryParse(Console.ReadLine(), out id) || id < 1)
                            {
                                Console.WriteLine("Please enter a valid Id");
                            }

                            var notificationsbyid = notificationService.ViewNotificationsByUserId(id);
                            if (notificationsbyid == null || notificationsbyid.Count == 0)
                            {
                                Console.WriteLine("No notifications for the user yet");

                            }
                            else
                            {

                                foreach (var notif in notificationsbyid)
                                {
                                    Console.WriteLine(notif);
                                }
                            }
                            break;
                        case 9:
                            exitMenu = true;
                            break;
                    }

                }
                // catch all custom exceptions and display to user
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Console.WriteLine("\n\n");
            }
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}