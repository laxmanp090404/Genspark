using GameApp.BusinessLogicLayer.Interfaces;
using GameApp.DataAccessLayer.Interfaces;
using GameApp.ModelLayer.Exceptions;
using GameApp.ModelLayer.Models;

namespace GameApp.BusinessLogicLayer.Services
{
    internal class GameService : IGameService
    {
        private readonly IWordProvider wordProvider;

        private readonly IGuessValidator guessValidator;

        private readonly IFeedbackGenerator feedbackGenerator;

        private readonly IGameResultRepository gameRepository;

        private readonly IGuessRepository guessRepository;

        private readonly int MaxAttempts;

        public GameService
        (
            IWordProvider provider,
            IGuessValidator validator,
            IFeedbackGenerator generator,
            IGameResultRepository gameRepo,
            IGuessRepository guessRepo
        )
        {
            wordProvider = provider;

            guessValidator = validator;

            feedbackGenerator = generator;

            gameRepository = gameRepo;

            guessRepository = guessRepo;

            MaxAttempts = 6;
        }

        public void StartGame(User currentUser)
        {
            HiddenWord hiddenWord = wordProvider.GetRandomHidden();

            bool isWon = false;

            List<GuessResult> guessHistory = new();

            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                Console.WriteLine($"\nAttempt {attempt}/{MaxAttempts}");

                Console.Write("Please enter a 5 letter word: ");

                string guess = (Console.ReadLine() ?? "").ToUpper();

                if (guessHistory.Any(g => g.Guess == guess))
                {
                    Console.WriteLine("Please avoid guessing the same guess again");

                    attempt--;

                    continue;
                }

                try
                {
                    // validate guess for exceptions
                    guessValidator.ValidateGuess(guess);

                    string feedback = feedbackGenerator.GenerateFeedback(hiddenWord.Word, guess);

                    GuessResult guessResult = new GuessResult(0, currentUser.Id, guess, feedback, attempt);

                    guessHistory.Add(guessResult);

                    // print the guess
                    PrintGuess(guess);
                    // print feedback with colorcoding
                    PrintColoredFeedBack(feedback);

                    if (feedback == "GGGGG")
                    {
                        isWon = true;

                        PrintAttemptComment(attempt);

                        Console.WriteLine("\nYou guessed correctly!");

                        break;
                    }
                }
                catch (InvalidGuessException ex)
                {
                    Console.WriteLine($"Invalid Guess: {ex.Message}");

                    attempt--;

                    continue;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected error " + e.Message);

                    attempt--;

                    continue;
                }
            }

            GameResult gameResult = new GameResult(0, currentUser.Id, isWon, guessHistory.Count, hiddenWord.WordId, hiddenWord.Word);
            // calculate score based on number of attempts
            gameResult.Score = CalculateScore(isWon, guessHistory.Count);

            GameResult createdGame = gameRepository.CreateGameResult(gameResult);

            foreach (var guess in guessHistory)
            {
                guess.GameId = createdGame.GameId;

                guessRepository.CreateGuess(guess);
            }

            Console.WriteLine();

            Console.WriteLine(createdGame);

            if (!isWon)
            {
                Console.WriteLine
                (
                    $"Correct Word: {hiddenWord.Word}"
                );
            }
        }

        public void ViewMyGames(int userId)
        {
            List<GameResult> games = gameRepository.GetGamesByUserId(userId);

            if (games.Count == 0)
            {
                Console.WriteLine("No games played yet");
                return;
            }

            foreach (var game in games)
            {
                Console.WriteLine();

                Console.WriteLine(game);

                Console.WriteLine("----------------------");
            }
        }

        public void ViewLeaderboard()
        {
            var leaderboard = gameRepository.GetLeaderboard();

            Console.WriteLine();

            Console.WriteLine("===== LEADERBOARD =====");

            int rank = 1;

            // rank calculation
            foreach (var user in leaderboard)
            {
                Console.WriteLine($"{rank}. {user.username} " + $"- {user.totalscore}");

                rank++;
            }
        }

        // calculate score based on attempts 6->10 score 1->60score
        private int CalculateScore(bool isWon, int attemptsUsed)
        {
            if (!isWon)
            {
                return 0;
            }

            return (7 - attemptsUsed) * 10;
        }

        private void PrintColoredFeedBack(string feedback)
        {
            Console.WriteLine();

            foreach (var f in feedback)
            {
                switch (f)
                {
                    case 'G':
                        Console.ForegroundColor =
                        ConsoleColor.Green;
                        break;

                    case 'Y':
                        Console.ForegroundColor =
                        ConsoleColor.Yellow;
                        break;

                    case 'X':
                        Console.ForegroundColor =
                        ConsoleColor.Red;
                        break;
                }

                Console.Write(f + "   ");
            }

            Console.ResetColor();

            Console.WriteLine();
        }

        // mapping attempt to correspondin comment
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

        private void PrintGuess(string guess)
        {
            Console.WriteLine();

            foreach (var g in guess)
            {
                Console.Write(g + "   ");
            }

            Console.WriteLine();
        }
    }
}