using NotificationApp.Interfaces;
using NotificationApp.Models;
using NotificationApp.Repositories;

namespace NotificationApp.Services
{

    internal class UserService : IUserService
    {
        UserRepository userRepository ;

        public UserService(UserRepository repo)
        {
            userRepository = repo;
        }

        // create user
        public User? CreateUser()
        {
            Console.WriteLine("Enter user name:");
            string name = Console.ReadLine() ?? "";

            Console.WriteLine("Enter email:");
            string email = Console.ReadLine() ?? "";

            Console.WriteLine("Enter phone:");
            string phone = Console.ReadLine() ?? "";
            
            // validating fields by encapsulated methods

            if(!validPhone(phone) || !validEmail(email))
            {
                return null;
            }
            User newuser = new User(0, name, email, phone);

            return userRepository.CreateUser(newuser);
            
        }



        // list all users
        public List<User>? GetUsers()
        {
            return userRepository.GetUsers();
        }

        // get user by id
        public User? GetUser()
        {
            Console.WriteLine("Enter teh User id:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("It is an invalid id");
                return null;
            }

            return userRepository.GetUserById(id);
        }

        // delete user by id
        public User? RemoveUser()
        {
            Console.WriteLine("Enter user id to remove:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("It is an invalid id");
                return null;
            }

            return userRepository.DeleteUser(id);
        }

        // update user by id
        public User? UpdateUser()
        {
            Console.WriteLine("Enter user id to update:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("It is an invalid id");
                return null;
            }

            User? existingUser = userRepository.GetUserById(id);

            if (existingUser == null)
            {
                Console.WriteLine("User not found");
                return null;
            }

            Console.WriteLine("Enter new name:");
            string name = Console.ReadLine() ?? "";

            Console.WriteLine("Enter new email:");
            string email = Console.ReadLine() ?? "";

            Console.WriteLine("Enter new phone:");
            string phone = Console.ReadLine() ?? "";

            // validating by encapsulated methods
            if (!validPhone(phone) || !validEmail(email))
                return null;

            User updatedUser = new User(id, name, email, phone);

            return userRepository.UpdateUser(id, updatedUser);
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
    }
}
