using UnderstandingOops.Exceptions;
using UnderstandingOops.Interfaces;
using UnderstandingOops.Models;
using UnderstandingOops.Repositories;
using UnderstandingOops.Services;

namespace UnderstandingOops
{
    internal class Program
    {
        ICustomerInteract customerInteract;
        IRepository<string, Account> accountRepoInteract;
        public Program()
        {
            customerInteract = new CustomerService();
            accountRepoInteract = new AccountRepository();
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

                while (!int.TryParse(Console.ReadLine(), out menuChoice) || menuChoice < 1 || menuChoice > 4)
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
                        accNum = Console.ReadLine() ?? "";
                        customerInteract.PrintAccountDetailsUsingAccountNumber(accNum);
                        break;
                    case 3:
                        Console.WriteLine("Please enter the account you like see");
                        accNum = Console.ReadLine() ?? "";
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
            // // new Program().DoBanking();
            // Account acc1 = new Account("1234", "Laxman", new DateTime(), "", "9876543210", 5000.5m);
            // Account acc2 = new Account("1234", "Laxman", new DateTime(), "", "9876543210", 5000.5m);
            // if (acc1 == acc2)
            //     Console.WriteLine("Same");
            // else
            //     Console.WriteLine("Not same");
            // try
            // {
                
            // checked
            // {
            //     int num1 = int.MaxValue;
            //     num1--; num1++;
            //     Console.WriteLine("The updated value is " + num1);
            //     Console.WriteLine("Now you can enter a number");
            //     num1 = Convert.ToInt32(Console.ReadLine());
            //     Console.WriteLine("Please enter the dinominator");
            //     int num2 = Convert.ToInt32(Console.ReadLine());
            //     var result = num1 / num2;
            //     Console.WriteLine("The final result is " + result);
            // }
            // }
            // catch (OverflowException ofe)
            // {
            //     // for developer
            //     Console.WriteLine(ofe.Message);
            //     //for user
            //     Console.WriteLine("Sorry data couldnt save");
            // }
            // catch(FormatException fe)
            // {
            //     // for developer
            //     Console.WriteLine(fe.Message);
            //     //for user
            //     Console.WriteLine("The input given by you is not a number");
            // }
            // catch(DivideByZeroException de)
            // {
            //     // for developer
            //     Console.WriteLine(de.Message);
            //     //for user
            //     Console.WriteLine("Cant divide a number by the denominator you provided");
            // }
            // catch(Exception e)
            // {
            //     // for developer
            //     Console.WriteLine(e.Message);
            //     //for user
            //     Console.WriteLine("An unexpected issue occur . Contact support");
            // }
            // Console.WriteLine("After the line that caused exception other lines will not be executed\nBye");

            IRepository<string,Account> accountRepo = new AccountRepository();

            try
            {
                var account = accountRepo.Create(new Account("","Laxman",new DateTime(),"","",1000.0m));
                Console.WriteLine(account);
            }
            catch (FormatException fe)
            {
                // for developer
                Console.WriteLine(fe.Message);
                //for user
                Console.WriteLine("the input for account details were not proper");
            }
            catch(OverflowException oe)
            {
                // for developer
                Console.WriteLine(oe.Message);
                //for user
                Console.WriteLine("Sorry data couldnt save");
            }
            catch(InvalidPhoneNumberException pe)
            {
                // for developer
                Console.WriteLine(pe.Message);
                //for user
                Console.WriteLine("Sorry data couldnt save");
            }
        }
    }
}