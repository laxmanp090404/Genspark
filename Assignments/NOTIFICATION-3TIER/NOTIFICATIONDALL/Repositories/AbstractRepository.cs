using NotificationDall.Interfaces;
using NotificationModels.Models;

namespace NotificationDall.Repositories
{
    // interface is first implemented in this abstract class
    // to ensure that methods common to concrete children is abstracted
    // also new repository can easily use the parent abstract class methods
    public abstract class AbstractRepository<K,T> : IRepository<K, T> where T :class where K:notnull
    {
        // Dictinony to store userid : userdata value pairs
        protected Dictionary<K,T> entitylist ;

        //create logic requires the key type 
        // key type is ambiguous at this point hence abstracted
        public abstract T CreateEntity(T entity);

        // get all entities
        public List<T>? GetAllEntity()
        {
            if(entitylist.Count == 0) return null;
            return entitylist.Values.ToList();
        }

        // get entity by id ie the key
        // return null if entity not found
        public T? GetEntityById(K id)
        {
            if(!entitylist.ContainsKey(id)) return null;
            else return entitylist[id];
        }

        // update entity by id
        // return null if entity not found or else return updated entity
        public T? UpdateEntity(K id,T item)
        {
            if(!entitylist.ContainsKey(id)) return null;

            entitylist[id] = item;
            return entitylist[id];   
        }
        public T? DeleteEntity(K id)
        {
            if(!entitylist.ContainsKey(id)) return null;
            T item = entitylist[id];
            entitylist.Remove(id);
            return item;
        }

    }
}