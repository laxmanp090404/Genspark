namespace GameApp.ModelLayer.Models
{
    // model to store Final result of a game
    public class GameResult
    {
        public int GameId{get;set;}
        public int UserId{get;set;}
        public bool IsWon { get; set; }

        public int AttemptsUsed { get; set; }

        public int WordId{get;set;}
        public string HiddenWord { get; set; }

        public int Score{get;set;} = 0;
        public DateTime CreatedAt{get;set;}

        public GameResult(int gameid,int userid,bool isWon, int attemptsUsed,int Wordid,string hiddenWord)
        {
            GameId =gameid;
            UserId =userid;
            IsWon = isWon;
            AttemptsUsed = attemptsUsed;
            WordId = Wordid;
            HiddenWord = hiddenWord;
        }

         public override string ToString()
        {
            return
            $"Game Id: {GameId}\n Won: {IsWon}\nAttempts Used: {AttemptsUsed}\nScore: {Score}\nPlayed At: {CreatedAt}";
        }
    }
}