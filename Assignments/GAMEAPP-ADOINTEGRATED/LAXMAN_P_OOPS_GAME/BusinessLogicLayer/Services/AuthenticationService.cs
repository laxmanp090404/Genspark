using GameApp.BusinessLogicLayer.Interfaces;
using GameApp.DataAccessLayer.Repositories;
using GameApp.ModelLayer.Models;
using GameApp.ModelLayer.Exceptions;
using GameApp.DataAccessLayer.Interfaces;

namespace GameApp.BusinessLogicLayer.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _repository;

        public AuthenticationService(IUserRepository repository)
        {
            _repository = repository;
        }

        public User? LoginUser()
        {
            User? loginDetails = GetUserDetailsFromConsole(false);

            if (loginDetails == null)
                return null;

            User? existingUser = _repository.GetUserByUserName(loginDetails.Username);

            // password check
            if (existingUser != null && existingUser.Password == loginDetails.Password)
            {
                Console.WriteLine("Login successful");
                return existingUser;
            }
            else
            {
                Console.WriteLine("Invalid username or password");
            }
            return null;
        }

        public void RegisterUser()
        {
            User? userDetails = GetUserDetailsFromConsole(true);

            if (userDetails == null)
                return;

            User? createdUser = _repository.CreateUser(userDetails);

            if (createdUser != null)
                Console.WriteLine($"User created successfully: {createdUser.Username}");
            else
                Console.WriteLine("Failed to create user");
        }

        private User? GetUserDetailsFromConsole(bool isRegister)
        {
            Console.WriteLine("Please enter your username:");
            string username = Console.ReadLine()?.Trim() ?? "";


            User? existingUser = _repository.GetUserByUserName(username);

            // USER IS REGISTERING AND USERNAME ALREADY EXISTS
            if (isRegister && existingUser != null)
            {
                Console.WriteLine("User already exists with the given username");
                return null;
            }
            // USER IS LOGGING IN BUT USERNAME DOES NOT EXIST
            if (!isRegister && existingUser == null)
            {
               Console.WriteLine("User does not exist with the given username");
               return null;
            }

            Console.WriteLine("Please enter your password (minimum 6 characters):");
            string password = Console.ReadLine() ?? string.Empty;

            if (password.Length < 6)
            {
                Console.WriteLine("Password must be at least 6 characters long");
            }

            return new User(-1, username, password);
        }
    }
}