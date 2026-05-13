using GameApp.BusinessLogicLayer.Interfaces;
using GameApp.DataAccessLayer.Interfaces;
using GameApp.ModelLayer.Models;

namespace GameApp.BusinessLogicLayer.Services
{
    internal class WordProviderService : IWordProvider
    {
        private readonly IHiddenWordRepository repository;

        private readonly Random random;

        public WordProviderService (IHiddenWordRepository repo)
        {
            repository = repo;

            random = new Random();
        }

        public HiddenWord GetRandomHidden()
        {
            // fetch all words from db
            List<HiddenWord> words =
            repository.GetAllWords();
            // send random word
            int index =
            random.Next(words.Count);

            return words[index];
        }
    }
}