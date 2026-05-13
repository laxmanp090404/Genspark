namespace GameApp.BusinessLogicLayer.Interfaces
{
    internal interface IFeedbackGenerator
    {
        // method to generate feedback for the guess
        string GenerateFeedback(string hiddenWord, string guess);
    }
}