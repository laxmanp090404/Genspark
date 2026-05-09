using NotificationModels.Models;

namespace NotificationBl.Interfaces{
    public interface IUserService
    {
        // ensuring implementing class contain below CRUD methods
        public User? CreateUser();
        public List<User>? GetUsers();
        public User? GetUser();
        public User? RemoveUser();
        public User? UpdateUser();
    }
}