using LIBRARYMODELS.Models;

namespace LIBRARYBL.Interfaces
{
    public interface IFineService
    {
       
        // get all pending fines
        ICollection<Fine> GetPendingFines(int memberId);
        //get total pending fine amount
        decimal GetPendingFineAmount(int memberId);
        //get all fines for a member
        ICollection<Fine> GetMemberFineHistory(int memberId);
    }
}