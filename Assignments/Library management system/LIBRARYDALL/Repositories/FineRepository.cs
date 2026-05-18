using LIBRARYMODELS.Models;
using LIBRARYDALL.Context;
using Microsoft.EntityFrameworkCore;
namespace LIBRARYDALL.Repositories;

public class FineRepository : AbstractRepository<int, Fine>
{
    public FineRepository(LibraryContext context) : base(context)
    {

    }
    //get fine by id
    public override Fine? GetById(int id)
    {
        return _context.Fines.SingleOrDefault(f => f.FineId == id);
    }

    //total pending fine by member
        public decimal GetTotalPendingFine(int memberId)
        {
            return _context.Fines.Where(f => f.MemberId == memberId && f.FineStatus == "pending")
            .Sum(f => f.DaysOverdue * f.FinePerDay);
        }
    // get all pending fines by member id
    public ICollection<Fine> GetPendingFines(int memberId)
    {
        return _context.Fines.Where(f => f.MemberId == memberId && f.FineStatus == "pending").ToList();
    }
    // get all fines for a member
    public ICollection<Fine> GetFinesByMember(int memberId)
    {
        return _context.Fines
            .Include(f => f.Borrowing)
            .Where(f =>
                f.MemberId == memberId)
            .ToList();
    }

}
