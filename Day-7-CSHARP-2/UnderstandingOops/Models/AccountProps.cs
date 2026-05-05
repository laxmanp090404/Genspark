// partial class of account
namespace UnderstandingOops.Models
{
    internal partial class Account:IComparable<Account>
    {
         // operator overloading
        public static bool operator ==(Account acc1, Account acc2)
        {
            return acc1.AccountNumber == acc2.AccountNumber;
        }

        // if == overload then != also defined to inform compiler abt else logic
        // operator overloading
        public static bool operator !=(Account acc1, Account acc2)
        {
            return acc1.AccountNumber == acc2.AccountNumber;
        }
                public override string ToString()
        {
            return $"Account Number : {AccountNumber}\nAccount Holder Name : {NameOnAccount}\nPhone Number : {Phone}\n" +
                $"Email : {Email}\nBalance : ₹{Balance}";
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj);

        }

        public int CompareTo(Account? acc)
        {
            return this.AccountNumber.CompareTo(acc?.AccountNumber);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}