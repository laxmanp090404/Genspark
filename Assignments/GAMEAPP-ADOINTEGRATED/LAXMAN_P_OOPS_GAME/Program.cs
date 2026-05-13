using GameApp.BusinessLogicLayer.Interfaces;
using GameApp.BusinessLogicLayer.Services;
using GameApp.DataAccessLayer.Interfaces;
using GameApp.DataAccessLayer.Repositories;
using GameApp.ModelLayer.Models;

namespace GameApp
{
    internal class Program
    {
        // business logic for game
        private readonly IGameService gameService;
        // business logic for Authentication
        private readonly IAuthenticationService authenticationService;

        // track loggedin user
        public User? currentUser = null;

        // flag to track menu status
        public static bool exitMenu = false;
        public Program()
        {
            IHiddenWordRepository hiddenWordRepository = new HiddenWordRepository();
            IWordProvider wordProvider = new WordProviderService(hiddenWordRepository);
            IGuessValidator guessValidator = new GuessValidator();
            IFeedbackGenerator feedbackGenerator = new FeedbackGenerator();
            IUserRepository userRepository = new UserRepository();
            IGameResultRepository gameRepository = new GameResultRepository();
            IGuessRepository guessRepository = new GuessRepository();
            // dependency injection
            gameService = new GameService(wordProvider, guessValidator, feedbackGenerator, gameRepository, guessRepository);
            authenticationService = new AuthenticationService(userRepository);
        }

        public void Run()
        {

            while (!exitMenu)
            {
                if (currentUser != null)
                {
                    showGameMenu();
                }
                else
                {
                    showAuthenticationMenu();
                }
                System.Console.WriteLine();
            }
        }

        // menu for logged in users to play game
        private void showGameMenu()
        {
            Console.WriteLine("1.Play Game\n2.View My Games\n3.View Leaderboard\n4.Logout\n5.Exit Game");
            Console.WriteLine("Please Enter your choice of service:");
            int choice = getIntInput(5);
            switch (choice)
            {
                case 1:
                    gameService.StartGame(currentUser!);
                    break;

                case 2:
                    gameService.ViewMyGames(currentUser!.Id);
                    break;

                case 3:
                    gameService.ViewLeaderboard();
                    break;
                case 4:
                    LogOut();
                    break;
                case 5:
                    exitMenu = true;
                    break;
            }
        }

        // menu for guest users
        private void showAuthenticationMenu()
        {
            Console.WriteLine("1.Login\n2.Register\n3.Exit Game");
            Console.WriteLine("Please Enter your choice of service:");
            int choice = getIntInput(3);
            switch (choice)
            {
                case 1:
                    currentUser = authenticationService.LoginUser();
                    break;
                case 2:
                    authenticationService.RegisterUser();
                    break;
                case 3:
                    exitMenu = true;
                    break;
            }
        }

        // encapsulated method to logout
        private void LogOut()
        {
            currentUser = null;
        }

        // helper method to validate choice range
        private int getIntInput(int range)
        {
            int choice = 0;
            while (!int.TryParse(Console.ReadLine(), out choice) || choice > range || choice < 1)
            {
                Console.WriteLine("Please enter a valid choice : ");
            }
            return choice;
        }



        static void Main(string[] args)
        {

            try
            {
                // Greetings at start of game
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Welcome to word guessing Game !");
                Console.ResetColor();
                // call App Run
                new Program().Run();
                // Exiting game app
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Exiting the game .....");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.WriteLine("For dev debugging the source of exception is " + e.Source);
                Console.WriteLine(e.Message);
            }

        }
    }
}