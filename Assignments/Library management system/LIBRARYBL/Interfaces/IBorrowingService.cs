using LIBRARYMODELS.Models;

namespace LIBRARYBL.Interfaces
{
    public interface IBorrowingService
    {
        // books and pinpointed using copyid
        //borrow book method with memberid and copyid
        Borrowing BorrowBook(int memberId,int copyId);
        //return a book
        Borrowing ReturnBook(int borrowingId,string returnType,string? damagedescription =null);
        //get borrowings for a particular member
        ICollection<Borrowing> GetMemberBorrowings(int memberId);
    }
}