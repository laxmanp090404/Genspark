using NotificationModels.Models;

namespace NotificationDall.Repositories
{
    // data access for user
    public class UserRepository : AbstractRepository<int, User>
    {
        public UserRepository()
        {
            // dictionary for efficient storage and retrieval based on key
            entitylist = new Dictionary<int,User>();
        }


        //indexes for array like access
        public User this[int index]
        {
            get{return entitylist[index];}
            set{entitylist[index] = value;}
        }
        // simulating user id 
        static int nextUserId = 1;
        public override User CreateEntity(User user)
        {
            user.Id = nextUserId;
            entitylist[nextUserId++] = user;
            return user;
        }
    }
}