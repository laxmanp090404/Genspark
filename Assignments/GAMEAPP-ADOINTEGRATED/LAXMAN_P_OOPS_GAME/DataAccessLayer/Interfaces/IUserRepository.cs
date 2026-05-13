using GameApp.ModelLayer.Models;

namespace GameApp.DataAccessLayer.Interfaces
{
    internal interface IUserRepository
    {
        User? CreateUser(User item);
        User? GetUserByUserName(string username);
    }
}