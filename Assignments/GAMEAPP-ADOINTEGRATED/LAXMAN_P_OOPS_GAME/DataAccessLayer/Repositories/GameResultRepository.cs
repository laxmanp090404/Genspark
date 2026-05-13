using GameApp.DataAccessLayer.Context;
using GameApp.DataAccessLayer.Interfaces;
using GameApp.ModelLayer.Models;
using Npgsql;

namespace GameApp.DataAccessLayer.Repositories
{
    public class GameResultRepository : IGameResultRepository
    {
        DbContext context;

        public GameResultRepository()
        {
            context = new DbContext();
        }

        public GameResult CreateGameResult(GameResult item)
        {
            NpgsqlConnection connection = context.GetConnection();

            // create game query also get gameid and createdat
            // use parameterised to avoid sql injection
            string createGameQuery = @"
            INSERT INTO games
            (
                userid,
                iswon,
                attemptsused,
                wordid,
                score
            )
            VALUES
            (
                @userid,
                @iswon,
                @attemptsused,
                @wordid,
                @score
            )
            RETURNING gameid, createdat;
            ";

            NpgsqlCommand command =
            new NpgsqlCommand(createGameQuery, connection);

            command.Parameters.AddWithValue("@userid", item.UserId);

            command.Parameters.AddWithValue("@iswon", item.IsWon);

            command.Parameters.AddWithValue("@attemptsused", item.AttemptsUsed);

            command.Parameters.AddWithValue("@wordid", item.WordId);

            command.Parameters.AddWithValue("@score", item.Score);

            try
            {
                // connection open
                connection.Open();

                NpgsqlDataReader reader = command.ExecuteReader();

                // getting gameid and createdat
                if (reader.Read())
                {
                    item.GameId = reader.GetInt32(0);
                    item.CreatedAt = reader.GetDateTime(1);

                    return item;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while creating game result " + e.Message);
            }
            finally
            {
                // close connection
                connection?.Close();
            }

            return null!;
        }
        public List<GameResult> GetGamesByUserId(int userId)
        {
            List<GameResult> games = new();

            NpgsqlConnection connection = context.GetConnection();

            string query = @"
                            select
                            g.gameid,
                            g.userid,
                            g.iswon,
                            g.attemptsused,
                            g.wordid,
                            h.word,
                            g.score,
                            g.createdat
                            from games g
                            join hiddenwords h
                            on g.wordid = h.wordid
                            where g.userid = @userid
                            order by g.createdat desc;
                            ";

            NpgsqlCommand command =
            new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@userid", userId);

            try
            {
                connection.Open();

                NpgsqlDataReader reader = command.ExecuteReader();
                // read all games
                while (reader.Read())
                {
                    // populate game
                    GameResult game = new GameResult
                    (
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        reader.GetBoolean(2),
                        reader.GetInt32(3),
                        reader.GetInt32(4),
                        reader.GetString(5)
                    );
                    // set score and createdate
                    game.Score = reader.GetInt32(6);

                    game.CreatedAt = reader.GetDateTime(7);

                    // add individual game
                    games.Add(game);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                connection?.Close();
            }

            return games;
        }

        public List<(string username,int totalscore)> GetLeaderboard()
        {
            List<(string username,int totalscore)>
            leaderboard = new();

            NpgsqlConnection connection =
            context.GetConnection();

            string query = @"
                            select
                            u.username,
                            sum(g.score) as totalscore
                            from users u
                            join games g
                            on u.id = g.userid
                            group by u.id, u.username
                            order by totalscore desc
                            limit 5;
                            ";

            NpgsqlCommand command =
            new NpgsqlCommand(query, connection);

            try
            {
                connection.Open();

                NpgsqlDataReader reader =
                command.ExecuteReader();

                while(reader.Read())
                {
                    leaderboard.Add
                    (
                        (
                            reader.GetString(0),
                            Convert.ToInt32(reader.GetInt64(1))
                        )
                    );
                }
            }
            catch(Exception e)
            {
                Console.WriteLine
                (
                    "Error fetching leaderboard "
                    + e.Message
                );
            }
            finally
            {
                connection.Close();
            }

            return leaderboard;
        }
    }
}