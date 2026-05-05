namespace NotificationApp.Interfaces
{
    // to enforce a contract of below methods
    internal interface IRepository<K,T> where T:class
    {
        
        public T CreateUser(T item);
        public List<T>? GetUsers();

        public T? GetUserById(K id);

        public T? UpdateUser(K key,T item);
        public T? DeleteUser(K key);


    }
}