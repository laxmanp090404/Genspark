using GameApp.Interfaces;

namespace GameApp.Services
{
    internal class WordProviderService : IWordProvider
    {
        // store predefined list of words
        private List<string> hiddenwordlist ;
        
        private Random random ;
        public WordProviderService()
        {
            hiddenwordlist = new List<string>{"APPLE","MANGO","GRAPE","TRAIN","PLANT","BRAIN"};
            random = new Random();
        }
        public string GetRandomHidden()
        {
            // randomised word selection
            int index = random.Next(hiddenwordlist.Count);

            return hiddenwordlist[index];
        }
    }
}