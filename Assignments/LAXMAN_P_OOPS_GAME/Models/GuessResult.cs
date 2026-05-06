namespace GameApp.Models
{
    // model to store a single Guess along with its feedback and attempt
    internal class GuessResult
    {
       public string Guess { get; set; } = string.Empty;

        public string Feedback { get; set; } = string.Empty;

        public int AttemptNumber { get; set; } = 1;

        public GuessResult(string guess, string feedback, int attemptNumber)
        {
            Guess = guess;
            Feedback = feedback;
            AttemptNumber = attemptNumber;
        }

        public override string ToString()
        {
            return $"Attempt {AttemptNumber}: Your guess : {Guess} and the feedback is : {Feedback}";
        }

    }
}