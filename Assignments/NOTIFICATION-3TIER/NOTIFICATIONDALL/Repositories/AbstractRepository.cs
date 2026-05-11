using NotificationDall.Interfaces;
using NotificationModels.Models;
using NotificationDall.Context;

namespace NotificationDall.Repositories
{
    // interface is first implemented in this abstract class
    // to ensure that methods common to concrete children is abstracted
    // also new repository can easily use the parent abstract class methods
    public abstract class AbstractRepository<K,T> : IRepository<K, T> where T :class where K:notnull
    {
        // db context
        protected DbContext context;
        public AbstractRepository()
        {
            // initialising with dbcontext
            context = new DbContext();
        }

        
        // all methods are abstract as queries are ambigous at this stage
        public abstract T CreateEntity(T entity);

        // get all entities
        public abstract List<T>? GetAllEntity();

        // get entity by id ie the key
        // return null if entity not found
        public abstract T? GetEntityById(K id);

        // update entity by id
        // return null if entity not found or else return updated entity
        public abstract T? UpdateEntity(K id,T item);
        public abstract T? DeleteEntity(K id);
    }
}