using System.Collections;
using GameApp.Exceptions;
using GameApp.Interfaces;
using GameApp.Models;

namespace GameApp.Services
{
    internal class GameService : IGameService
    {
        // dependency objects
        IWordProvider wordProvider;

        IGuessValidator guessValidator;

        IFeedbackGenerator feedbackGenerator;

        int MaxAttempts;

        // dependency injected via constructor
        public GameService(IWordProvider provider, IGuessValidator validator, IFeedbackGenerator generator)
        {
            wordProvider = provider;
            guessValidator = validator;
            feedbackGenerator = generator;
            MaxAttempts = 6;
        }

        public void StartGame()
        {
            // get hidden word
            string hiddenWord = wordProvider.GetRandomHidden();

            bool isWon = false;
            
            // list to store the guesses
            List<GuessResult> guessHistory = new List<GuessResult>();


            // iterating over 6 attempts
            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                Console.WriteLine($"\nAttempt {attempt}/{MaxAttempts}");

                Console.Write("Please enter a 5 letter word: ");

                string guess = Console.ReadLine() ?? "";
                guess = guess.ToUpper();

                //prevent duplicate guesses
                if(guessHistory.Any((g)=>g.Guess == guess))
                {
                    Console.WriteLine("Please don't make the same guess that you already made");
                    // to ensure attempt is not reduced;
                    attempt--;
                    continue;
                }

                // Exception might occur so try block
                try
                {
                    // validate guess based on UD exceptions
                    guessValidator.ValidateGuess(guess);
                    // feedback string is created
                    string feedback = feedbackGenerator.GenerateFeedback(hiddenWord, guess);

                    GuessResult result = new GuessResult(guess, feedback, attempt);

                    guessHistory.Add(result);

                    PrintGuess(guess);
                    PrintColoredFeedBack(feedback);
                    
                    // all letter in correct place then won
                    if (feedback == "GGGGG")
                    {
                        isWon = true;
                        // print 
                        PrintAttemptComment(attempt);

                        Console.WriteLine("\nYou guessed the word correctly!");

                        break;
                    }
                }
                // catch custom exception and handle
                catch (InvalidGuessException ex)
                {
                    Console.WriteLine($"Invalid Guess: {ex.Message}");

                    attempt--;

                    continue;
                }
                // if some unhandled exception occur
                catch(Exception e)
                {
                    Console.WriteLine($"Unexpected error occured . Sorry from our side: {e.Message}");

                    attempt--;

                    continue;
                }
            }

            GameResult gameResult = new GameResult(isWon, guessHistory.Count, hiddenWord);

            Console.WriteLine();

            Console.WriteLine(gameResult);

            if (!isWon)
            {
                Console.WriteLine($"Correct Word: {hiddenWord}");
            }
        }


        // Print colored feedback using the foregroundcolor prop
        // encapsulated private method
        private void PrintColoredFeedBack(string feedback)
        {
            Console.WriteLine();
            foreach(var f in feedback)
            {
                switch (f)
                {
                    case 'G':
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                    case 'Y':
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                    case 'X':
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                }
                Console.Write(f+"   ");

            }
            Console.ResetColor();
            Console.WriteLine();
        }
       
        // private helper method to provide comment based on number of attempts the user has won
        private void PrintAttemptComment(int attempt)
        {
            switch (attempt)
            {
                case 1:
                    Console.WriteLine("Genius!");
                    break;

                case 2:
                    Console.WriteLine("Excellent!");
                    break;

                case 3:
                    Console.WriteLine("Great job!");
                    break;

                case 4:
                    Console.WriteLine("Good work!");
                    break;

                case 5:
                    Console.WriteLine("Nice try!");
                    break;

                case 6:
                    Console.WriteLine("That was close!");
                    break;
            }
        }

        // styled printing format od guess
        private void PrintGuess(string guess)
        {
            Console.WriteLine();
            foreach(var g in guess)
            {
                Console.Write(g+"   ");
            }
            Console.WriteLine();
        }
    }
}