namespace NotificationDall.Interfaces
{
    // to enforce a contract of below methods to any entity that need basic crud in this system
    public interface IRepository<K,T> where T:class
    {
        // create
        public T? CreateEntity(T item);
        //read all
        public List<T>? GetAllEntity();
        // read by key
        public T? GetEntityById(K id);
        // update by key
        public T? UpdateEntity(K key,T item);
        // delete by key
        public T? DeleteEntity(K key);


    }
}