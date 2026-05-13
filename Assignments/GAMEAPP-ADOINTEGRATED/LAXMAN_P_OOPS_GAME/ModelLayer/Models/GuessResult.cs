namespace GameApp.ModelLayer.Models
{
    // model to store a single Guess along with its feedback and attempt
    public class GuessResult
    {
        // additionally use game id as foriegn key or reference
        public int GameId{get;set;}
        public int UserId{get;set;}
       public string Guess { get; set; } = string.Empty;

        public string Feedback { get; set; } = string.Empty;

        public int AttemptNumber { get; set; } = 1;

        public GuessResult(int gameid,int userid,string guess, string feedback, int attemptNumber)
        {
            
            GameId = gameid;
            UserId = userid;
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