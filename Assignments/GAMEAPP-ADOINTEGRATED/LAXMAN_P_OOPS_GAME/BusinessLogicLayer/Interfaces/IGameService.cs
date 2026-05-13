using GameApp.ModelLayer.Models;

namespace GameApp.BusinessLogicLayer.Interfaces
{
    internal interface IGameService
    {
        //start game
        void StartGame(User currentUser);

        // view user's games
        void ViewMyGames(int userId);
        // view leaderboard based on scores
        void ViewLeaderboard();
    }
}