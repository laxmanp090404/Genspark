using System.Numerics;

namespace Day6
{
    internal class TypesExample
    {
        internal void Run()
        {
            System.Console.WriteLine("From typesexample");
            System.Console.WriteLine("Please Enter a number");
            int num = Convert.ToInt32(Console.ReadLine()); // unboxing ie reference to value type
            System.Console.WriteLine();
            // int num1;
            // int num2 = num1;
            // BigInteger num1=null;
            // BigInteger num2 = num1;
        }

        internal void showLimits()
        {
            int? num1 = null; // nullable int allow null to be assignedto int

            int ans = num1 ?? 0 + 100;// null coalscing operator if num is null use zero there
            int intmax = int.MaxValue;
            int intmin = int.MinValue;

            // converto handle null value by equating base values
            // if parse used then nullexception thrown

            //checked in c# 

            System.Console.WriteLine("The max value of int is " + intmax);
            System.Console.WriteLine("The min value of int is " + intmin);
            System.Console.WriteLine("The max value incremented will give cycling " + ++intmax);

            System.Console.WriteLine("The same abvove cycling can be stopped and thrown as exception using checked");



            

            intmax = int.MaxValue;
           
           
            System.Console.WriteLine("Before increment: " + intmax);

            checked
            {
                intmax++; // Exception happens here
            }
            System.Console.WriteLine("The max value incremented will give cycling " + intmax);


        }
    }
}