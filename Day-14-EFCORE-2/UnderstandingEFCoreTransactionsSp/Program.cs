using Microsoft.EntityFrameworkCore;
using UnderstandingEFCoreTransactionsSp.Models;

namespace UnderstandingEFCoreTransactionsSp;

public class Program
{
    readonly Day4Context _day4Context;

    public Program()
    {
     _day4Context = new Day4Context();  
    }
    public void AddTransactionWithTransactionInDatabase(int fromaccno,int toaccno,decimal amount)
    {
        Account? fromacc = _day4Context.Accounts.FirstOrDefault(acc=>acc.Accno==fromaccno);
        Account? toacc = _day4Context.Accounts.FirstOrDefault(acc=>acc.Accno==toaccno);

        if(fromacc == null)
        {
            System.Console.WriteLine("From account not found");
            return;
        }
          if(toacc == null)
        {
            System.Console.WriteLine("To account not found");
            return;
        }
        // starting transaction
        using var transaction = _day4Context.Database.BeginTransaction();
        try
        {
         if(fromacc.Balance -amount <0) throw new Exception("Insufficient balance") ;
         _day4Context.Database.ExecuteSqlInterpolated($"call add_transaction({fromaccno},{toaccno},{amount})");
         _day4Context.Database.ExecuteSqlInterpolated($"call update_account({fromaccno},{fromacc.Balance-amount})");  
         _day4Context.Database.ExecuteSqlInterpolated($"call update_account({toaccno},{toacc.Balance+amount})");  
         transaction.Commit();
         System.Console.WriteLine("transaction sucesss");
        }
        catch (System.Exception e)
        {
            
            transaction.Rollback();
            System.Console.WriteLine("error occured at "+e.Source+" "+e.Message);
        }

    }
    public void AddAccount(int accno,decimal? balance)
    {
         _day4Context.Database.ExecuteSqlInterpolated($"call add_account({accno},{balance??0})");
    }
    static void Main(string[] args)
    {
         Account acc = new Account();
        acc.Accno = 5;
        acc.Balance = 3800;
        
       Program program = new Program();
    //    program.AddAccount(acc.Accno,acc.Balance);
    program.AddTransactionWithTransactionInDatabase(2,1,500);
    }
}
