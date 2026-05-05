using NotificationApp.Interfaces;
using NotificationApp.Models;

namespace NotificationApp.Repositories
{
    // interface is first implemented in this abstract class
    // to ensure that methods common to concrete children is abstracted
    // also new repository can easily use the parent abstract class methods
    internal abstract class AbstractRepository<K,T> : IRepository<K, T> where T :class
    {
        // Dictinony to store userid : userdata value pairs
        protected Dictionary<K,T> userlist ;

        //create logic requires the key type 
        // key type is ambiguous at this point hence abstracted
        public abstract T CreateUser(T user);

        // get all users
        public List<T>? GetUsers()
        {
            if(userlist.Count == 0) return null;
            return userlist.Values.ToList();
        }

        // get user by id ie the key
        // return null if user not found
        public T? GetUserById(K id)
        {
            if(!userlist.ContainsKey(id)) return null;
            else return userlist[id];
        }

        // update user by id
        // return null if user not found or else return updated user
        public T? UpdateUser(K id,T item)
        {
            if(!userlist.ContainsKey(id)) return null;

            userlist[id] = item;
            return userlist[id];   
        }
        public T? DeleteUser(K id)
        {
            if(!userlist.ContainsKey(id)) return null;
            T item = userlist[id];
            userlist.Remove(id);
            return item;
        }

    }
}