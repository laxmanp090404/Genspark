using NotificationApp.Models;

namespace NotificationApp.Repositories
{
    internal class UserRepository : AbstractRepository<int, User>
    {
        public UserRepository()
        {
            // dictionary for efficient storage and retrieval based on key
            userlist = new Dictionary<int,User>();
        }


        //indexes for array like access
        public User this[int index]
        {
            get{return userlist[index];}
            set{userlist[index] = value;}
        }
        // simulating user id 
        static int nextUserId = 1;
        public override User CreateUser(User user)
        {
            user.Id = nextUserId;
            userlist[nextUserId++] = user;
            return user;
        }
    }
}