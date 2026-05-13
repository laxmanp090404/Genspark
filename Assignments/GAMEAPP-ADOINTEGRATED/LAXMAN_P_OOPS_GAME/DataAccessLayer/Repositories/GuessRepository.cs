using GameApp.DataAccessLayer.Context;
using GameApp.DataAccessLayer.Interfaces;
using GameApp.ModelLayer.Models;
using Npgsql;

namespace GameApp.DataAccessLayer.Repositories
{
    public class GuessRepository : IGuessRepository
    {
        DbContext context;

        public GuessRepository()
        {
            context = new DbContext();
        }

        public GuessResult CreateGuess(GuessResult item)
        {
            NpgsqlConnection connection =
            context.GetConnection();

            // insert guesses and get guessid
            string query = @"
            insert into guesses
            (
                gameid,
                userid,
                guess,
                feedback,
                attemptnumber
            )
            values
            (
                @gameid,
                @userid,
                @guess,
                @feedback,
                @attemptnumber
            )
            returning guessid;
            ";

            NpgsqlCommand command =
            new NpgsqlCommand(query, connection);

            // parameter injections
            command.Parameters.AddWithValue
            ("@gameid", item.GameId);

            command.Parameters.AddWithValue
            ("@userid", item.UserId);

            command.Parameters.AddWithValue
            ("@guess", item.Guess);

            command.Parameters.AddWithValue
            ("@feedback", item.Feedback);

            command.Parameters.AddWithValue
            ("@attemptnumber", item.AttemptNumber);

            try
            {
                connection.Open();
                // executing command and return guesssresult
                command.ExecuteScalar();

                return item;
            }
            catch(Exception e)
            {
                Console.WriteLine
                (
                    "Error creating guess "
                    + e.Message
                );
            }
            finally
            {
                connection?.Close();
            }

            return null!;
        }

        public List<GuessResult>GetGuessesByGameId(int gameId)
        {
            List<GuessResult> guesses = new();

            NpgsqlConnection connection =
            context.GetConnection();
            // return guesses for a game ordered by attempt desc
            string query = @"
            select
            gameid,
            userid,
            guess,
            feedback,
            attemptnumber
            from guesses
            where gameid = @gameid
            order by attemptnumber;
            ";

            NpgsqlCommand command =
            new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue
            ("@gameid", gameId);

            try
            {
                connection.Open();

                NpgsqlDataReader reader =
                command.ExecuteReader();

                while(reader.Read())
                {
                    // populate guess
                    GuessResult guess =
                    new GuessResult
                    (
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        reader.GetString(2).Trim(),
                        reader.GetString(3).Trim(),
                        reader.GetInt32(4)
                    );
                    // add guess to list
                    guesses.Add(guess);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                connection?.Close();
            }

            return guesses;
        }
    }
}