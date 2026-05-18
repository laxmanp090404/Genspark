using LIBRARYBL.Interfaces;
using LIBRARYBL.Validators;
using LIBRARYDALL.Context;
using LIBRARYDALL.Repositories;
using LIBRARYMODELS.Models;
using Microsoft.EntityFrameworkCore;

namespace LIBRARYBL.Services;

public class BookService : IBookService
{
    private readonly BookRepository _bookRepository;
    private readonly BookValidator _validator;

    private readonly LibraryContext _context;

    public BookService(BookRepository bookRepository,LibraryContext context)
    {
        _bookRepository = bookRepository;
        _validator = new BookValidator();
        _context = context;
    }

    public Book AddBook(Book book)
    {
        bool isValid = _validator.ValidateBook(book);

        if (!isValid)
        {
            throw new Exception("Invalid book details");
        }

        return _bookRepository.Add(book);
    }
    //add a copy
    public BookCopy AddCopy(BookCopy copy)
    {
        copy.CopyStatus = "Available";
        _context.BookCopies.Add(copy);
        _context.SaveChanges();
        return copy;
    }
    // add a edition
    public BookEdition AddEdition(BookEdition edition)
    {
        _context.BookEditions.Add(edition);
        _context.SaveChanges();
        return edition;
    }

    public ICollection<Book> GetAllBooks()
    {
        return _bookRepository.GetAll();
    }

    public ICollection<BookCopy> GetAllCopies()
    {
        return _bookRepository.GetAllCopies();
    }

    public Book? GetBookById(int bookId)
    {
        return _bookRepository.GetById(bookId);
    }

    public ICollection<BookCategory> GetCategories()
    {
        return _bookRepository.GetCategories();
    }

    public ICollection<BookCopy> GetCopiesByStatus(string status)
    {
       return _bookRepository.GetCopiesByStatus(status);
    }

    public ICollection<BookEdition> GetEditions()
    {
       return _bookRepository.GetEditions();
    }

    // search books by author
    public ICollection<Book> SearchBooksByAuthor(string author)
    {
        return _bookRepository.GetBooksWithCategory().Where(b=>b.Author.ToLower().Contains(author.ToLower())).ToList();
    }
    // search books by category
    public ICollection<Book> SearchBooksByCategory(string category)
    {
        return _bookRepository.GetBooksWithCategory().Where(b=>b.Category.CategoryName.ToLower().Contains(category.ToLower())).ToList();
    }
    // search books by title
    public ICollection<Book> SearchBooksByTitle(string title)
    {
        return _bookRepository.GetBooksWithCategory().Where(b=>b.Title.ToLower().Contains(title.ToLower())).ToList();
    }
    // smart search based on all details
    public ICollection<Book> SmartSearchBooks(string searchText)
    {
        return _bookRepository.GetBooksWithCategory().Where(b=>
        b.Title.ToLower().Contains(searchText.ToLower()) || b.Author.ToLower().Contains(searchText.ToLower()) || b.Category.CategoryName.ToLower().Contains(searchText.ToLower()) 
        ).ToList();
    }
}