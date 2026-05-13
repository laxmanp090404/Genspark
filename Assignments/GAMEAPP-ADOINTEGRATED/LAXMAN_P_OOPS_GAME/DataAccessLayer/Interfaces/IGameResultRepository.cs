using GameApp.ModelLayer.Models;

namespace GameApp.DataAccessLayer.Interfaces
{
    public interface IGameResultRepository
    {
         GameResult CreateGameResult(GameResult item);
        // to use in get my games ie get users games by his id
         List<GameResult> GetGamesByUserId(int userId);

         // store and return pairs of username along with score for leaderboard   
         List<(string username,int totalscore)> GetLeaderboard();
    }
}