using LIBRARYDALL.Context;
using LIBRARYDALL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LIBRARYDALL.Repositories;

public abstract class AbstractRepository<K, T>:IRepository<K, T> where T : class where K:notnull
{
    // faster access
    protected readonly LibraryContext _context;

    public AbstractRepository(LibraryContext context)
    {
        _context = context;
    }

    // virtual methods to avoid overriding if required
    public virtual T Add(T entity)
    {
        _context.Set<T>().Add(entity);
        _context.SaveChanges();

        return entity;
    }

    public virtual T Delete(K key)
    {
        var entity = GetById(key);

        if (entity == null)
        {
            throw new Exception("Entity not found");
        }

        _context.Set<T>().Remove(entity);
        _context.SaveChanges();

        return entity;
    }

    public virtual ICollection<T> GetAll()
    {
        return _context.Set<T>().ToList();
    }

    // id ambiguous
    public abstract T? GetById(K key);

    public virtual T Update(T entity)
    {
        _context.Set<T>().Update(entity);
        _context.SaveChanges();

        return entity;
    }
}