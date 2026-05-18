using LIBRARYMODELS.Models;

namespace LIBRARYBL.Validators;

public class BookValidator
{
    public bool ValidateBook(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(book.Author))
        {
            return false;
        }

        return true;
    }
}