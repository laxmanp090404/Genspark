namespace GameApp.Models
{
    // model to store Final result of a game
    internal class GameResult
    {
        public bool IsWon { get; set; }

        public int AttemptsUsed { get; set; }

        public string HiddenWord { get; set; }

        public GameResult(bool isWon, int attemptsUsed, string hiddenWord)
        {
            IsWon = isWon;
            AttemptsUsed = attemptsUsed;
            HiddenWord = hiddenWord;
        }

        public override string ToString()
        {
            return IsWon ? $"You won in {AttemptsUsed} attempts!" : $"You lost! The word was {HiddenWord}";
        }
    }
}