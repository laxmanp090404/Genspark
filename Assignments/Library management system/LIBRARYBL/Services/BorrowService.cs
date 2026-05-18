using LIBRARYBL.Interfaces;
using LIBRARYDALL.Context;
using LIBRARYDALL.Repositories;
using LIBRARYMODELS.Models;
using Microsoft.EntityFrameworkCore;
namespace LIBRARYBL.Services;

public class BorrowingService : IBorrowingService
{
    private readonly BorrowingRepository _borrowingRepository;
    private readonly MemberRepository _memberRepository;
    private readonly FineRepository _fineRepository;
    private readonly LibraryContext _context;

    public BorrowingService
    (
        BorrowingRepository borrowingRepository,
        MemberRepository memberRepository,
        FineRepository fineRepository,
        LibraryContext context
    )
    {
        _borrowingRepository = borrowingRepository;
        _memberRepository = memberRepository;
        _fineRepository = fineRepository;
        _context = context;
    }

    public Borrowing BorrowBook(int memberId, int copyId)
    {
        Member? member = _context.Members.Include(m=>m.MembershipType).SingleOrDefault(m=>m.MemberId == memberId);

        if (member == null)
        {
            throw new Exception("Member not found");
        }

        if (member.IsActive != true)
        {
            throw new Exception("Member account inactive");
        }

        decimal pendingFine =
            _fineRepository.GetTotalPendingFine(memberId);

        if (pendingFine > 500)
        {
            throw new Exception("Pending fine exceeds limit");
        }

        int activeBorrowCount =
            _borrowingRepository
            .GetActiveBorrowingsByMember(memberId)
            .Count;

        int allowedBorrowLimit =
            member.MembershipType.MaxActiveBorrows;

        if (activeBorrowCount >= allowedBorrowLimit)
        {
            throw new Exception("Borrowing limit reached");
        }

        BookCopy? copy =
            _context.BookCopies
            .Include(c=>c.Edition)
            .ThenInclude(e=>e.Book)
            .FirstOrDefault(c => c.CopyId == copyId);

        if (copy == null)
        {
            throw new Exception("Book copy not found");
        }

        if (copy.CopyStatus.ToLower() != "available" && copy.CopyStatus.ToLower() != "damaged_usable")
        {
            throw new Exception("Book copy unavailable");
        }
        if(copy.CopyStatus.ToLower() == "damaged_usable")
        {
            System.Console.WriteLine("Warning : This book is a little bit damaged but usable");
        }

        int bookId = copy.Edition.BookId;

        bool alreadyBorrowed =
            _borrowingRepository
            .HasActiveBorrowOfSameBook(memberId, bookId);

        if (alreadyBorrowed)
        {
            throw new Exception
            (
                "Member already borrowed this book"
            );
        }

        using var transaction =
            _context.Database.BeginTransaction();

        try
        {
            Borrowing borrowing = new Borrowing();

            borrowing.MemberId = memberId;
            borrowing.CopyId = copyId;
            borrowing.BorrowedOn = DateOnly.FromDateTime(DateTime.Now);

            borrowing.DueDate =
                DateOnly.FromDateTime(DateTime.Now.AddDays
                (
                    member.MembershipType.MaxBorrowDays
                ));

            borrowing.BorrowStatus = "Active";

            _context.Borrowings.Add(borrowing);

            copy.CopyStatus = "Borrowed";

            _context.BookCopies.Update(copy);

            _context.SaveChanges();

            transaction.Commit();

            Console.WriteLine("Book borrowed successfully");

            return borrowing;
        }
        catch (Exception e)
        {
            transaction.Rollback();

            Console.WriteLine
            (
                "Borrowing failed : " + e.Message
            );

            throw;
        }
    }

    public ICollection<Borrowing> GetMemberBorrowings(int memberId)
    {
        return _borrowingRepository
            .GetActiveBorrowingsByMember(memberId);
    }

    
    public Borrowing ReturnBook(int borrowingId, string returnType, string? damagedescription = null)
    {
       Borrowing? borrowing = _borrowingRepository.GetById(borrowingId);

       if(borrowing == null)
        {
            throw new Exception("Borrowing record not found with "+borrowingId);
        }
        if(borrowing.BorrowStatus == "Returned")
        {
            throw new Exception("Book already returned");
        }
        // using transaction
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            borrowing.ReturnedOn = DateOnly.FromDateTime(DateTime.Now);
            BookCopy? copy = _context.BookCopies.FirstOrDefault(c=>c.CopyId == borrowing.CopyId);

            if(copy == null)
            {
                throw new Exception("Copy not found");
            }
            decimal extraDamagefine = 0;

            // damage status
            switch (returnType.ToLower())
            {
                case "normal":
                copy.CopyStatus = "Available";
                break;

                case "damaged_usable":
                copy.CopyStatus = "Damaged_Usable";
                extraDamagefine = 100;
                CreateDamageLog(copy.CopyId,borrowing.MemberId,damagedescription,"Minor","Damaged_Usable",extraDamagefine);
                break;

                case "damaged_unusable":
                copy.CopyStatus = "Damaged_Unusable";
                extraDamagefine = 300;
                CreateDamageLog(copy.CopyId,borrowing.MemberId,damagedescription,"Severe","Damaged_Unusable",extraDamagefine);
                break;

                case "lost":
                copy.CopyStatus = "Lost";
                borrowing.BorrowStatus = "Lost";
                extraDamagefine = 1000;
                CreateDamageLog(copy.CopyId,borrowing.MemberId,damagedescription,"Severe","Lost",extraDamagefine);
                break;

                default :
                throw new Exception("Invalid return type of borrowing");
            }
            _context.BookCopies.Update(copy);
            int delayDays = borrowing.ReturnedOn.Value.DayNumber - borrowing.DueDate.DayNumber;

            if(delayDays > 0 || extraDamagefine > 0)
            {
                Fine fine = new Fine();
                fine.MemberId = borrowing.MemberId;
                fine.BorrowingId = borrowing.BorrowingId;
                fine.DaysOverdue = (short)Math.Max(delayDays,1);
                fine.FinePerDay = 10;

                MembershipType membershipType = _context.MembershipTypes.First(m=>m.MembershipTypeId == borrowing.Member.MembershipTypeId);

                // special offer for studnets
                if(membershipType.TypeName.ToLower() == "student")
                {
                    fine.FineStatus = "waived";
                    fine.WaiverReason = "Student Membership Waiver offer";
                }
                else
                {
                    fine.FineStatus = "pending";                    
                }
                _context.Fines.Add(fine);
            }
            _context.Borrowings.Update(borrowing);
            _context.SaveChanges();
            transaction.Commit();
            System.Console.WriteLine("Book returned sucessfully");
            return borrowing;

        }
        catch (System.Exception e)
        {
            
            transaction.Rollback();
            Console.WriteLine("Return failed :"+e.Message);
            throw;
        }
    }

    private void CreateDamageLog(int copyId, int memberId, string? damagedescription, string severity, string resultingstatus, decimal fineamount)
    {
        BookDamageLog log = new BookDamageLog();
        log.CopyId = copyId;
        log.MemberId = memberId;
        log.DamageDescription = damagedescription??"No description provided";
        log.DamageSeverity = severity;

        log.ResultingStatus = resultingstatus;
        log.FineApplied = fineamount;
        _context.BookDamageLogs.Add(log);
    }
}