using NotificationBl.Interfaces;
using NotificationModels.Models;
using NotificationDall.Repositories;
using NotificationBl.Validators;
using NotificationModels.Exceptions;

namespace NotificationBl.Services
{

    public class UserService : IUserService
    {
        public UserRepository userRepository;

        //dependency injection
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
            // validating fields by extension methods that throws appropriate exceptions
            email.ValidateEmail();

            Console.WriteLine("Enter phone:");
            string phone = Console.ReadLine() ?? "";

            // validating fields by extension methods that throws appropriate exceptions
            phone.ValidatePhone();

            User newuser = new User(0, name, email, phone);

            return userRepository.CreateEntity(newuser);

        }



        // list all users
        public List<User>? GetUsers()
        {
            return userRepository.GetAllEntity();
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

            return userRepository.GetEntityById(id);
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

            return userRepository.DeleteEntity(id);
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

            User? existingUser = userRepository.GetEntityById(id);

            if (existingUser == null)
            {
                throw new NotFoundException("user");
                
            }

            Console.WriteLine("Enter new name:");
            string name = Console.ReadLine() ?? "";

            Console.WriteLine("Enter new email:");
            string email = Console.ReadLine() ?? "";
            // validating fields by extension methods that throws appropriate exceptions
            email.ValidateEmail();
            Console.WriteLine("Enter new phone:");
            string phone = Console.ReadLine() ?? "";

            // validating fields by extension methods that throws appropriate exceptions
            phone.ValidatePhone();

            User updatedUser = new User(id, name, email, phone);

            return userRepository.UpdateEntity(id, updatedUser);
        }



    }
}
