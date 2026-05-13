using GameApp.ModelLayer.Models;

namespace GameApp.DataAccessLayer.Interfaces
{
    public interface IHiddenWordRepository
    {
        // list all hiddenwords
        List<HiddenWord> GetAllWords();
    }
}