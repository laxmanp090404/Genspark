namespace UnderstandingOops.Models
{
    public enum AccType
    {
        SavingAccount =1,CurrentAccount=2
    }
    internal class Account
    {

        // c# props

        // public required string AccountNumber {get;set;} = string.Empty;// or we can make it required
        public string AccountNumber { get; set; } = string.Empty;// without empty it can be nullable
        public string NameOnAccount { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public decimal Balance { get; set; }
                public AccType AccountType { get; set; }


        // defaut constructor
        public Account()
        {
            AccountNumber = "0000";
            NameOnAccount = "Unknown";
            Balance = 1000;
        }
        public Account(String accountNumber, string nameOnAccount, DateTime dateOfBirth, string email, string phone, decimal balance)
        {
            AccountNumber = accountNumber;
            NameOnAccount = nameOnAccount;
            DateOfBirth = dateOfBirth;
            Email = email;
            Phone = phone;
            Balance = balance;
        }

        public override string ToString()
        {
            return $"Account Number : {AccountNumber}\nAccount Holder Name : {NameOnAccount}\nPhone Number : {Phone}\n" +
                $"Email : {Email}\nBalance : ₹{Balance}";
        }


    }

    
}