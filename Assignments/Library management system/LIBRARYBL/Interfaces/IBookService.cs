using LIBRARYMODELS.Models;

namespace LIBRARYBL.Interfaces;

public interface IBookService
{
    //add a book
    Book AddBook(Book book);
    // get all books (irrespective of status)
    ICollection<Book> GetAllBooks();
    // search book by any detail
    ICollection<Book> SmartSearchBooks(string searchText);
    // search book by title
    ICollection<Book> SearchBooksByTitle(string title);
    // search book by category
    ICollection<Book> SearchBooksByCategory(string category);
    //search book by author
    ICollection<Book> SearchBooksByAuthor(string author);
    //get copies by their status like available
    ICollection<BookCopy> GetCopiesByStatus(string status);
   
    //get books by Id
    Book? GetBookById(int bookId);

    // edition and copy Management
    //add an edition
    BookEdition AddEdition(BookEdition edition);
    //add copies
    BookCopy AddCopy(BookCopy copy);
    //get categories
    ICollection<BookCategory> GetCategories();
    //get all editions
    ICollection<BookEdition> GetEditions();
    // get all copies
    ICollection<BookCopy> GetAllCopies();
    
}