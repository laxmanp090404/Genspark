using LIBRARYDALL.Context;
using LIBRARYMODELS.Models;
using Microsoft.EntityFrameworkCore;
namespace LIBRARYDALL.Repositories;

public class BorrowingRepository : AbstractRepository<int, Borrowing>
{
    public BorrowingRepository(LibraryContext context): base(context)
    {
    }

    // overrided method
    public override Borrowing? GetById(int key)
    {
        // load member and copy details and filter
        return _context.Borrowings
            .Include(b => b.Member)
            .Include(b => b.Copy)
            .SingleOrDefault(b => b.BorrowingId == key);
    }

    // method to get borrowings by member
    public ICollection<Borrowing> GetActiveBorrowingsByMember(int memberId)
    {
        // filter borrowings by member id and also ensure borrow is not returned ie active
        return _context.Borrowings
            .Include(b => b.Copy).ThenInclude(c=>c.Edition).ThenInclude(e=>e.Book)
            .Where(b =>
                b.MemberId == memberId &&
                b.BorrowStatus == "Active")
            .ToList();
    }

    //check if the member has a borrow for the same book ie any edition 
    public bool HasActiveBorrowOfSameBook(int memberId, int bookId)
    {
        return _context.Borrowings
            .Include(b => b.Copy)
            .ThenInclude(c => c.Edition)
            .Any(b =>
                b.MemberId == memberId &&
                b.BorrowStatus == "Active" &&
                b.Copy.Edition.BookId == bookId);
    }
}