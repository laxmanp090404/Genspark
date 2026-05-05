using NotificationApp.Interfaces;
using NotificationApp.Repositories;
using NotificationApp.Services;

namespace NotificationApp
{
    internal class Program
    {
        INotificationInteract notificationInteract;
        IUserService userService;

        public Program()
        {
            // initialising repos and services
            UserRepository repo = new UserRepository();

            userService = new UserService(repo);
            notificationInteract = new NotificationService(userService);
        }

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
                Console.WriteLine("7. Exit");
                Console.Write("Enter your choice: ");

                int choice;
                // prompt user to enter valid choice
                while (!int.TryParse(Console.ReadLine(), out choice))
                {
                    Console.WriteLine("Please enter a valid choice : ");
                    
                }

                switch (choice)
                {
                    case 1:
                        var createdUser = userService.CreateUser();

                        if (createdUser != null)
                            Console.WriteLine("User Created Successfully : {createdUser}");
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
                        notificationInteract.SendNotificationToUser();
                        break;

                    case 7:
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