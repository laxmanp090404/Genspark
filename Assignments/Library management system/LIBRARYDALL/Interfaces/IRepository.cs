namespace LIBRARYDALL.Interfaces;

public interface IRepository<K, T> where T : class where K : notnull
{
    ICollection<T> GetAll();
    
    T? GetById(K key);

    T Add(T entity);

    T Update(T entity);

    T Delete(K key);
}

