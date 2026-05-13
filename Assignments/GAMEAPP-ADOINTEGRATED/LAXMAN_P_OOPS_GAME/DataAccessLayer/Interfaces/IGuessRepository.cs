using GameApp.ModelLayer.Models;

namespace GameApp.DataAccessLayer.Interfaces
{
    public interface IGuessRepository
    {
        // create a guess
        GuessResult CreateGuess(GuessResult item);
        // list all guesses by gameid
        List<GuessResult> GetGuessesByGameId(int gameId);
    }
}