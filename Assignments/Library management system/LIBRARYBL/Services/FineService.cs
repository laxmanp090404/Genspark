using LIBRARYBL.Interfaces;
using LIBRARYDALL.Context;
using LIBRARYDALL.Repositories;
using LIBRARYMODELS.Models;
using Microsoft.EntityFrameworkCore;

namespace LIBRARYBL.Services;

public class FineService : IFineService
{
    private readonly FineRepository _fineRepository;
    private readonly LibraryContext _context;

    public FineService
    (
        FineRepository fineRepository,
        LibraryContext context
    )
    {
        _fineRepository = fineRepository;
        _context = context;
    }

   public decimal GetPendingFineAmount(int memberId)
{
    decimal result = _context.Database.SqlQueryRaw<decimal>(  "SELECT calculate_member_fine({0})",memberId
).First();

   

    return result;
}

    public ICollection<Fine> GetPendingFines(int memberId)
    {
        return _fineRepository
            .GetPendingFines(memberId);
    }

    public Fine PayFine(int fineId, string paymentMode, string? remarks)
    {
        Fine? fine = _fineRepository.GetById(fineId);

        if (fine == null)
        {
            throw new Exception("Fine not found");
        }

        if (fine.FineStatus.ToLower() == "paid")
        {
            throw new Exception("Fine already paid");
        }

        if (fine.FineStatus.ToLower() == "waived")
        {
            throw new Exception("Waived fines cannot be paid");
        }

        using var transaction = _context.Database.BeginTransaction();

        try
        {
            decimal totalAmount = fine.DaysOverdue * fine.FinePerDay;

            FinePayment payment = new FinePayment();

            payment.FineId = fine.FineId;

            payment.AmountPaid = totalAmount;

            payment.PaymentMode = paymentMode;

            payment.Remarks = remarks;

            _context.FinePayments.Add(payment);

            _context.Database.ExecuteSqlInterpolated($"call pay_fine({fineId})");

            _context.SaveChanges();

            transaction.Commit();

            Console.WriteLine("Fine paid successfully");

            return fine;
        }
        catch (Exception e)
        {
            transaction.Rollback();

            Console.WriteLine("Payment failed : " + e.Message);

            throw;
        }
    }

    // get member fine history 
    public ICollection<Fine> GetMemberFineHistory(int memberId)
    {
        return _fineRepository.GetFinesByMember(memberId);
    }
}