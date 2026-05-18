using LIBRARYMODELS.Models;

namespace LIBRARYBL.Interfaces;

public interface IReportService
{
    // borrowed books
    ICollection<Borrowing> GetCurrentlyBorrowedBooks();
    // borrowed beyond due
    ICollection<Borrowing> GetOverdueBorrowings();
    // list members with pending fines
    ICollection<Member> GetMembersWithPendingFines();
    // list most borrowed books
    ICollection<Book> GetMostBorrowedBooks();
    // borrowing history for a member
    ICollection<Borrowing> GetBorrowingHistoryByMember(int memberId);
    // get available books based on category
    ICollection<BookCopy> GetAvailableBooksByCategory(string category);
}