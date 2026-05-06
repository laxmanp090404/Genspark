using GameApp.Interfaces;
using GameApp.Services;

namespace GameApp
{
    internal class Program
    {
        private readonly IGameService gameService;

        public Program()
        {

            IWordProvider wordProvider = new WordProviderService();

            IGuessValidator guessValidator = new GuessValidator();

            IFeedbackGenerator feedbackGenerator = new FeedbackGenerator();

            // dependency injection
            gameService = new GameService(wordProvider, guessValidator, feedbackGenerator);
        }

        public void Run()
        {

            gameService.StartGame();
        }

        static void Main(string[] args)
        {

            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Welcome to word guessing Game !");
            Console.ResetColor();
            while (true)
            {

                Program program = new Program();


                program.Run();
                Console.WriteLine("Continue to Play ? (Y/N)");
                string gameInput = "";
                while (gameInput != "Y" && gameInput != "N")
                {
                    gameInput = Console.ReadLine() ?? "";
                    gameInput = gameInput.ToUpper();


                }
                if (gameInput == "N")
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Thanks for playing");
                    Console.ResetColor();

                    break;
                }

            }

        }
    }
}