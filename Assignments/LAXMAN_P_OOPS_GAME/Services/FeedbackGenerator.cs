using System.Text;
using GameApp.Interfaces;

namespace GameApp.Services
{
    internal class FeedbackGenerator : IFeedbackGenerator
    {
        /*
          i have changed the first method as it fails in the testcase

          hidden word = PLANT
          guess = PPPPP
          output : User wins which is not right

          just commented the method instead of deleting for reference

        */

        // public string GenerateFeedback(string hiddenWord, string guess)
        // {
        //     StringBuilder feedback = new StringBuilder();

        //     for (int i = 0; i < hiddenWord.Length; i++)
        //     {
        //         if (guess[i] == hiddenWord[i])
        //         {
        //             feedback.Append('G');
        //         }
        //         else if (hiddenWord.Contains(guess[i]))
        //         {
        //             feedback.Append('Y');
        //         }
        //         else
        //         {
        //             feedback.Append('X');
        //         }
        //     }

        //     return feedback.ToString();
        // }

        // the below method handles the above mentioned testcase
        public string GenerateFeedback(string hiddenWord,string guess)
        {
            char[] feedback = ['X','X','X','X','X'];
            bool[] usedhash = new bool[5];

            // first mark direct matches
            for(int i = 0; i < 5; i++)
            {
                if(hiddenWord[i] ==guess[i])
                {
                    feedback[i] = 'G';
                    usedhash[i] = true;
                }
            }

            // second pass consuming letters
            for(int i = 0; i < 5; i++)
            {
                if(feedback[i] == 'G') continue;
                for(int j = 0; j < 5; j++)
                {
                    if(!usedhash[j] && guess[i] == hiddenWord[j])
                    {
                        feedback[i] = 'Y';
                        usedhash[j] = true;
                        break;
                    }
                }
            }

            return new string(feedback);
        }
    }
}