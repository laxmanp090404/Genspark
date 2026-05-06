using GameApp.Exceptions;
using GameApp.Interfaces;

namespace GameApp.Services
{
    internal class GuessValidator : IGuessValidator
    {
        // validation class
        public void ValidateGuess(string guess)
        {
            // empty exception
            if (string.IsNullOrWhiteSpace(guess))
            {
                throw new InvalidGuessException("Input cannot be empty.");
            }

            // input less than 5
            if (guess.Length < 5)
            {
                throw new InvalidGuessException("Your input contain less than 5 letters.\nWord must contain exactly 5 letters.");
            }
            
            // input greater than 5
            if (guess.Length > 5)
            {
                throw new InvalidGuessException("Your input contains more than 5 letters.\nWord must contain exactly 5 letters.");
            }
            // Non-alphabetic inputs
            if (!guess.All(char.IsLetter))
            {
                throw new InvalidGuessException("Only alphabetic letters are allowed.");
            }
        }
    }
}