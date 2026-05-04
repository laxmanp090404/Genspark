using UnderstandingOops.Interfaces;
using UnderstandingOops.Services;

namespace UnderstandingOOops
{
    internal class Program
    {
        ICustomerInteract customerInteract;
        public Program()
        {
            customerInteract = new CustomerService();
        }
        void DoBanking()
        {
            var account = customerInteract.OpensAccount();
            Console.WriteLine(account);
            
            bool exitMenu = false;
            while (!exitMenu)
            {
                int menuChoice;
                Console.WriteLine("------------------------------");
                Console.WriteLine("Please provide your choice of service based on below ");
                Console.WriteLine("1.Add Account \n2.Get Account By Account Number \n3.Get Account By Mobile Number \n4. Exit");
                
                while(!int.TryParse(Console.ReadLine(),out menuChoice) || menuChoice<1 || menuChoice > 4)
                {
                    Console.WriteLine("Please provide a valid option");
                }
                string accNum;
                switch (menuChoice)
                {
                    case 1:
                    customerInteract.OpensAccount();
                    break;
                    case 2:
                    Console.WriteLine("Please enter the account you like see");
                    accNum = Console.ReadLine()??"";
                    customerInteract.PrintAccountDetailsUsingAccountNumber(accNum);
                    break;
                    case 3:
                    Console.WriteLine("Please enter the account you like see");
                    accNum = Console.ReadLine()??"";
                    customerInteract.PrintAccountDetailsUsingMobile(accNum);
                    break;
                    case 4:
                    exitMenu = true;
                    break;
                }
                Console.WriteLine("------------------------------");
            }
            
            
            // Console.WriteLine("Please enter the account you like see");
            // string accNum = Console.ReadLine()??"";
            // customerInteract.PrintAccountDetailsUsingAccountNumber(accNum);
            
        }
        static void Main(string[] args)
        {
                Console.WriteLine("Welcome to the banking system.\n Please Open an account following the instructions below :");
            new Program().DoBanking();
        }
    }
}