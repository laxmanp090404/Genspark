using UnderstandingOops.Models;

namespace UnderstandingOops.Interfaces
{
    internal interface ICustomerInteract
    {
        public Account OpensAccount();
        public void PrintAccountDetailsUsingAccountNumber(string accountNumber);
        public void PrintAccountDetailsUsingMobile(string mobileNumber);
    }
}