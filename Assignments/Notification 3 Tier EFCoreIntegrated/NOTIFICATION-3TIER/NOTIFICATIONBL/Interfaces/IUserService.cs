using NotificationModels.Models;

namespace NotificationBl.Interfaces{
    public interface IUserService
    {
        // ensuring implementing class contain below CRUD methods
        // create a user
        public User? CreateUser();
        // get all users 
        public List<User>? GetUsers();
        // get individual user
        public User? GetUser();
        // remove user
        public User? RemoveUser();
        // update user details
        public User? UpdateUser();
    }
}