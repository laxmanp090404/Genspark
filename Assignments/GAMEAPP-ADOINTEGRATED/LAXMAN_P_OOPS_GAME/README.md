# Word Guessing Game - 3 Tier Architecture(folder based only) With ADO integration

## Overview
This project is a console-based word guessing game built using a 3-tier architecture.

- The presentation layer is the console application in `Program.cs`.
- The business logic layer is in `BusinessLogicLayer`, where authentication, validation, feedback generation, and gameplay flow are handled.
- The data access layer is in `DataAccessLayer`, which connects to PostgreSQL and stores users, hidden words, games, and guesses.
- The model layer is in `ModelLayer`, which contains the entities and custom exception used by the application.

The application allows a :
- user to register, log in
- play the word guessing game
- view his past games
- view the leaderboard.

## Some clarifications regarding my implementation wrt the task:
- I have followed the tier-based branching approach only by folder and not as projects as said by mam.
- Leaderboard i have fetched only top 5 users.
- When user view his games it is sent sorted by playedat like latest game first
- game scoring logic was made like 1st attempt->score 60,2nd attempt ->score 50 and formulated like score = (7-attempts)*10
- hidden words are stored in database and fetched since no admin module was asked to implement words can only be inserted via dml insert in db
- Sample output can be found in [SuccessfulExecution.png](SuccessfulExecution.png).

## Project Structure

```text
LAXMAN_P_OOPS_GAME/
├── Program.cs
├── LAXMAN_P_OOPS_GAME.csproj
├── README.md
├── SuccessfulExecution.png
├── WordGameDatabase.sql
├── BusinessLogicLayer/
│   ├── Interfaces/
│   │   ├── IAuthenticationService.cs
│   │   ├── IFeedbackGenerator.cs
│   │   ├── IGameService.cs
│   │   ├── IGuessValidator.cs
│   │   └── IWordProvider.cs
│   └── Services/
│       ├── AuthenticationService.cs
│       ├── FeedbackGenerator.cs
│       ├── GameService.cs
│       ├── GuessValidator.cs
│       └── WordProviderService.cs
├── DataAccessLayer/
│   ├── Context/
│   │   └── DbContext.cs
│   ├── Interfaces/
│   │   ├── IGameResultRepository.cs
│   │   ├── IGuessRepository.cs
│   │   ├── IHiddenWordRepository.cs
│   │   └── IUserRepository.cs
│   └── Repositories/
│       ├── GameResultRepository.cs
│       ├── GuessRepository.cs
│       ├── HiddenWordRepository.cs
│       └── UserRepository.cs
└── ModelLayer/
	├── Exceptions/
	│   └── InvalidGuessException.cs
	└── Models/
		├── GameResult.cs
		├── GuessResult.cs
		├── HiddenWord.cs
		└── User.cs
```

## Each File Function

### Presentation Layer
- [Program.cs](Program.cs): Starts the console application, shows the menu, handles login state, and calls the business layer.

### BusinessLogicLayer
- [BusinessLogicLayer/Interfaces/IAuthenticationService.cs](BusinessLogicLayer/Interfaces/IAuthenticationService.cs): Declares login and registration operations.
- [BusinessLogicLayer/Interfaces/IFeedbackGenerator.cs](BusinessLogicLayer/Interfaces/IFeedbackGenerator.cs): Declares the feedback generation contract for guesses.
- [BusinessLogicLayer/Interfaces/IGameService.cs](BusinessLogicLayer/Interfaces/IGameService.cs): Declares gameplay, personal game history, and leaderboard operations.
- [BusinessLogicLayer/Interfaces/IGuessValidator.cs](BusinessLogicLayer/Interfaces/IGuessValidator.cs): Declares validation for user guesses.
- [BusinessLogicLayer/Interfaces/IWordProvider.cs](BusinessLogicLayer/Interfaces/IWordProvider.cs): Declares the contract for fetching a random hidden word.
- [BusinessLogicLayer/Services/AuthenticationService.cs](BusinessLogicLayer/Services/AuthenticationService.cs): Handles registration and login using the user repository.
- [BusinessLogicLayer/Services/FeedbackGenerator.cs](BusinessLogicLayer/Services/FeedbackGenerator.cs): Generates G/Y/X style feedback for each guess.
- [BusinessLogicLayer/Services/GameService.cs](BusinessLogicLayer/Services/GameService.cs): Runs the game , stores results, prints game history, and shows the leaderboard.
- [BusinessLogicLayer/Services/GuessValidator.cs](BusinessLogicLayer/Services/GuessValidator.cs): Validates that guesses based on requirements provided in assignment
- [BusinessLogicLayer/Services/WordProviderService.cs](BusinessLogicLayer/Services/WordProviderService.cs): Fetches all hidden words and returns one at random.

### DataAccessLayer
- [DataAccessLayer/Context/DbContext.cs](DataAccessLayer/Context/DbContext.cs): Creates PostgreSQL connections using the project connection string.
- [DataAccessLayer/Interfaces/IGameResultRepository.cs](DataAccessLayer/Interfaces/IGameResultRepository.cs): Declares methods for saving games, loading games by user, and building the leaderboard.
- [DataAccessLayer/Interfaces/IGuessRepository.cs](DataAccessLayer/Interfaces/IGuessRepository.cs): Declares methods for saving guesses and loading guesses for a game.
- [DataAccessLayer/Interfaces/IHiddenWordRepository.cs](DataAccessLayer/Interfaces/IHiddenWordRepository.cs): Declares the contract for reading all hidden words.
- [DataAccessLayer/Interfaces/IUserRepository.cs](DataAccessLayer/Interfaces/IUserRepository.cs): Declares methods for creating and finding users.
- [DataAccessLayer/Repositories/GameResultRepository.cs](DataAccessLayer/Repositories/GameResultRepository.cs): Stores completed game results and reads past games and leaderboard data from PostgreSQL.
- [DataAccessLayer/Repositories/GuessRepository.cs](DataAccessLayer/Repositories/GuessRepository.cs): Stores each guess made during a game and reads guesses by game id.
- [DataAccessLayer/Repositories/HiddenWordRepository.cs](DataAccessLayer/Repositories/HiddenWordRepository.cs): Reads the list of hidden words from PostgreSQL.
- [DataAccessLayer/Repositories/UserRepository.cs](DataAccessLayer/Repositories/UserRepository.cs): Creates users and retrieves active users by username.

### ModelLayer
- [ModelLayer/Models/User.cs](ModelLayer/Models/User.cs): Represents an application user.
- [ModelLayer/Models/HiddenWord.cs](ModelLayer/Models/HiddenWord.cs): Represents a hidden word used in the game.
- [ModelLayer/Models/GuessResult.cs](ModelLayer/Models/GuessResult.cs): Represents a single guess along with its feedback and attempt number.
- [ModelLayer/Models/GameResult.cs](ModelLayer/Models/GameResult.cs): Represents the final result of a completed game.
- [ModelLayer/Exceptions/InvalidGuessException.cs](ModelLayer/Exceptions/InvalidGuessException.cs): Custom exception thrown when a guess is invalid.

## Database Script
- [WordGameDatabase.sql](WordGameDatabase.sql): Contains the PostgreSQL schema and seed data(for words) used by the application.

## Sample Output
Sample output from a successful run of the application is available in [SuccessfulExecution.png](SuccessfulExecution.png).
