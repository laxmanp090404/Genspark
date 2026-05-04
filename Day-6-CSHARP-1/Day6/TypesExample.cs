using System.Numerics;

namespace Day6
{
    internal class TypesExample
    {
        internal void StringInterpolation()
        {
            System.Console.WriteLine("Please enter your name: ");
            String name = Console.ReadLine()??"No name given dude"; // null coalescing operator
            System.Console.WriteLine("Please enter your age");
            int age = Convert.ToInt32(Console.ReadLine());
            System.Console.WriteLine($"Hi {name} .\n I can't beleove your age is {age}");
            // System.Console.WriteLine("Hi name "+".\n I can't beleove your age is"+ "age"); default old way of concatenation
        }
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


// Primitive types vs Reference types
// parseInt vs ConvertTo vs TryParse
// unboxing boxing
// checked in.c#
// nullable vs nullcoalescing operator
// Datatype cycling /overflow
// internal access modifiers in csharp
// string replace , trim , contains 
// datatyes their size and range (signed and unsigned with generic formula)
// explicit type casting or implicit type casting
// why decimal is needed ? proper justification
//foreach loop
//var in csharp
// collections in csharp List<T>
// index operator
// indexing [start..end+1] type 
// spread operator in c#
// linq - language integrated query
//IEnumerable
// types of constructor in csharp (is there something called primary constructor)
// abstract class / types of classes in csharp
// to string method of all objects


// React framework
// Soul.md Agents.md Skills.md
// context window, tokens
// Prompt injections
//guardrails
// openclaw , n8n , zapier, langchain , langraph

// eval ai agents :Function eval , Cost Eval , Safety Eval
//langsmith and ragas are tools for evaluating ai agents