using LIBRARYDALL.Context;
using LIBRARYMODELS.Models;
using Microsoft.EntityFrameworkCore;

namespace LIBRARYDALL.Repositories;

public class MemberRepository : AbstractRepository<int, Member>
{
    public MemberRepository(LibraryContext context) : base(context)
    {
    }

    public override Member? GetById(int key)
    {
        //LINQ based
        return _context.Members.SingleOrDefault(m => m.MemberId == key);
    }
    public override ICollection<Member> GetAll()
    {
        //eager loading for membership type inclusal
        return _context.Members.Include(m=>m.MembershipType).ToList();
    }

    // RETURN LIST OF MEMBERSHIP TYPES
    public ICollection<MembershipType> GetMembershipTypes()
    {
        return _context.MembershipTypes.ToList();
    }
    
}