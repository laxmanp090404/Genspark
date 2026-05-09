using NotificationBl.Interfaces;
using NotificationModels.Models;
using NotificationModels.Exceptions;
using NotificationDall.Repositories;
using NotificationBl.Validators;

namespace NotificationBl.Services
{
    public class NotificationService : INotificationService
    {
        // dependencies
        NotificationRepository repository;
        IUserService userService;

        // dependency injection via constructor
        public NotificationService(NotificationRepository _repo, IUserService _userService)
        {
            repository = _repo;
            userService = _userService;


        }

        public void SendNotificationToUser()
        {
            // if they want to see user id to send message
            Console.WriteLine("Do you want to view Users to enter their Id ?(Y/N):");
            string userViewChoice = (Console.ReadLine()??"").Trim().ToUpper();
            while (userViewChoice != "Y" && userViewChoice != "N")
            {
                Console.WriteLine("Please enter a valid option");
                userViewChoice = (Console.ReadLine()??"").Trim().ToUpper();

            }
            // if they want to view users
            if (userViewChoice == "Y")
            {
                var userlist = userService.GetUsers();
                if (userlist == null || userlist.Count == 0)
                {
                    Console.WriteLine("There are no users yet!");
                    return;
                }
                foreach (var individualUser in userlist)
                {
                    Console.WriteLine($"Id: {individualUser.Id} Name: {individualUser.Name}");
                }
            }
            Console.WriteLine("Please Enter user Id : ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id) || id <= 0)
            {
                Console.WriteLine("Please enter a valid id");
            }
            List<User> users = userService.GetUsers() ?? [];
            if (users.Count == 0)
            {
                Console.WriteLine("There are no users yet!");
                return;
            }
            var user = users.FirstOrDefault((u) => u.Id == id);
            if (user == null)
                throw new NotFoundException("user");

            Console.WriteLine("Enter message:");
            string message = Console.ReadLine() ?? "";

            // validate message with extension method that throws exception
            message.ValidateMessage();

            Console.WriteLine("Choose notification type: 1.Email 2.SMS");
            int choice = int.Parse(Console.ReadLine() ?? "0");


            Notification notif;

            if (choice == 1)
                notif = new EmailNotification(message, user);
            else if (choice == 2)
            {
                // if a sms then the extension method is called
                message.ValidateSMS();
                notif = new SmsNotification(message, user);
            }
            else
            {
                throw new NotFoundException("Notification type");
            }
            notif.Send();
            repository.CreateEntity(notif);
            Console.WriteLine("Notification stored Successfully!");
        }

        // view all notification
        public List<Notification>? ViewAllNotifications()
        {
            return repository.GetAllEntity();
        }

        // view notification by Recipient
        public List<Notification>? ViewNotificationsByUserId(int userId)
        {
            List<Notification>? allNotifications = repository.GetAllEntity();

            if (allNotifications == null)
                return null;

            return allNotifications
                    .Where(n => n.Recipient.Id == userId)
                    .ToList();
        }
    }
}