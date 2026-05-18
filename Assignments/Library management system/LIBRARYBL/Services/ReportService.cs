using LIBRARYBL.Interfaces;
using LIBRARYDALL.Context;
using LIBRARYMODELS.Models;
using Microsoft.EntityFrameworkCore;

namespace LIBRARYBL.Services;

public class ReportService : IReportService
{
    private readonly LibraryContext _context;

    public ReportService(LibraryContext context)
    {
        _context = context;
    }

     // borrowed books   
    public ICollection<Borrowing> GetCurrentlyBorrowedBooks()
    {
        return _context.Borrowings
            .Include(b => b.Member)
            .Include(b => b.Copy)
            .ThenInclude(c => c.Edition)
            .ThenInclude(e => e.Book)
            .Where(b => b.BorrowStatus == "Active")
            .ToList();
    }
    // borrowed beyond due
    public ICollection<Borrowing> GetOverdueBorrowings()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        return _context.Borrowings
            .Include(b => b.Member)
            .Include(b => b.Copy)
            .ThenInclude(c => c.Edition)
            .ThenInclude(e => e.Book)
            .Where(b => b.BorrowStatus == "Active" && b.DueDate < today)
            .ToList();
    }
    // list members with pending fines
    public ICollection<Member> GetMembersWithPendingFines()
    {
        return _context.Members
            .Include(m => m.Fines)
            .Where(m => m.Fines.Any(f => f.FineStatus == "pending"))
            .ToList();
    }
    // list most borrowed books
    public ICollection<Book> GetMostBorrowedBooks()
    {
        return _context.Books
            .Include(b => b.BookEditions)
            .ThenInclude(e => e.BookCopies)
            .ThenInclude(c => c.Borrowings)
            .OrderByDescending(b =>
                b.BookEditions
                    .SelectMany(e => e.BookCopies)
                    .SelectMany(c => c.Borrowings)
                    .Count())
            .Take(5)
            .ToList();
    }
    // borrowing history for a member
    public ICollection<Borrowing> GetBorrowingHistoryByMember(int memberId)
    {
        return _context.Borrowings
            .Include(b => b.Copy)
            .ThenInclude(c => c.Edition)
            .ThenInclude(e => e.Book)
            .Where(b => b.MemberId == memberId)
            .ToList();
    }
    // get available books based on category
    public ICollection<BookCopy> GetAvailableBooksByCategory(string category)
    {
        return _context.BookCopies
            .Include(c => c.Edition)
            .ThenInclude(e => e.Book)
            .ThenInclude(b => b.Category)
            .Where(c =>
                (c.CopyStatus == "Available" || c.CopyStatus == "Damaged_Usable") &&
                c.Edition.Book.Category.CategoryName.ToLower().Contains(category.ToLower()))
            .ToList();
    }
}