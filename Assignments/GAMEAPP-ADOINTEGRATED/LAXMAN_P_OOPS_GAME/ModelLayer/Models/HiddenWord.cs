namespace GameApp.ModelLayer.Models
{
    // model to manage hiddenwords or words for user to guess
    public class HiddenWord
    {
        // uid for hiddenword
        public int WordId { get; set; }

        // hiddenWord
        public string Word { get; set; } = string.Empty;

        public HiddenWord(int wordId, string word)
        {
            WordId = wordId;
            Word = word;
        }

        public override string ToString()
        {
            return $"WordId: {WordId} Word: {Word}";
        }
    }
}