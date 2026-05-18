using LIBRARYDALL.Context;
using LIBRARYMODELS.Models;
using Microsoft.EntityFrameworkCore;
namespace LIBRARYDALL.Repositories;

public class BookRepository : AbstractRepository<int, Book>
{
    public BookRepository(LibraryContext context) : base(context)
    {
    }

    // Get books by theri id
    public override Book? GetById(int key)
    {
        // also eager loading of category and editions
        return _context.Books
                       .Include(b => b.Category)
                       .Include(b => b.BookEditions)
                       .SingleOrDefault(b => b.BookId == key);
    }
    public override ICollection<Book> GetAll()
    {
        return _context.Books
        .Include(b => b.Category)
        .Include(b => b.BookEditions)
        .ThenInclude(e => e.BookCopies)
        .ToList();
    }

    public ICollection<Book> SearchBooks(string searchText)
    {
        // retreives books which either matches title, category and author
        return _context.Books
            .Include(b => b.Category)
            .Where(b =>
                b.Title.ToLower().Contains(searchText.ToLower()) ||
                b.Author.ToLower().Contains(searchText.ToLower()) ||
                b.Category.CategoryName.ToLower().Contains(searchText.ToLower()))
            .ToList();
    }

    // public ICollection<Book> GetAvailableBooks()
    // {
    //     // return all copies of that are Available status
    //     return _context.Books
    //         .Include(b => b.BookEditions)
    //         .ThenInclude(e => e.BookCopies)
    //         .Where(b => b.BookEditions.Any(e =>
    //             e.BookCopies.Any(c => c.CopyStatus == "Available")))
    //         .ToList();
    // }
    // eager loads category
    public ICollection<Book> GetBooksWithCategory()
   {
    return _context.Books
        .Include(b => b.Category)
        .ToList();
   }

   // get all categories
   public ICollection<BookCategory> GetCategories()
    {
        return _context.BookCategories.ToList();
    }
    //get all editions
    public ICollection<BookEdition> GetEditions()
    {
        return _context.BookEditions.Include(e=>e.Book).ToList();
    }

    // get all copies
    public ICollection<BookCopy> GetAllCopies()
    {
        return _context.BookCopies.Include(c=>c.Edition).ThenInclude(e=>e.Book).ToList();
    }

    public ICollection<BookCopy> GetCopiesByStatus(string status)
{
    return _context.BookCopies
        .Include(c => c.Edition)
        .ThenInclude(e => e.Book)
        .Where(c =>c.CopyStatus.ToLower()==status.ToLower()).ToList();
}
}
