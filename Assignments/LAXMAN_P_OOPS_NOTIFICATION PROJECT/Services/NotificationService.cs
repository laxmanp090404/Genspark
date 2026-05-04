using NotificationApp.Interfaces;
using NotificationApp.Models;

namespace NotificationApp.Services
{
    internal class NotificationService : INotificationInteract
    {
        // managing list of users
        List<User> userList = new List<User>();
        static int nextId = 1;


        public void CreateUser()
        {
            // get user details
            Console.WriteLine("Enter user name:");
            string name = Console.ReadLine() ?? "";

            Console.WriteLine("Enter email:");
            string email = Console.ReadLine() ?? "";

            Console.WriteLine("Enter phone:");
            string phone = Console.ReadLine() ?? "";
            
            // validating fields by encapsulated methods

            if(!validPhone(phone) || !validEmail(email))
            {
                return;
            }

            User user = new User(nextId++, name, email, phone);
            userList.Add(user);

            Console.WriteLine($"User created successfully with the folowing user id: {user.Id}");
        }

        // encapsulated helper methods
        private Boolean validEmail(string email)
        {
            if (!email.Contains('@'))
            {
                Console.WriteLine("Email is not in proper format");
                return false;
            }
            return true;
        }
        private Boolean validPhone(string phone)
        {
            if (phone.StartsWith('+'))
            {
                Console.WriteLine("Please Don't include country codes in number");
                return false;
            }
            if(phone.Length !=10)
            {
                Console.WriteLine("Ensure the phone number is correct");
                return false;
            }
            return true;
        }


        // list all users to choose whom to send msg
        public void ShowAllUsers()
        {
            if(userList.Count == 0)
            {
                Console.WriteLine("No users found");
                return;
            }
            // listing users if not empty
            foreach (var user in userList)
            {
                Console.WriteLine(user);
            }
        }

        public void SendNotificationToUser()
        {
            // getting notification details
            Console.WriteLine("Enter user id:");
            int id = int.Parse(Console.ReadLine() ?? "0");

            var user = userList.FirstOrDefault(u => u.Id == id);

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
            notify( message, notif);
        }

        private void notify(string message, Notification notifobj)
        {

            notifobj.Send(message);
        }
    }
}