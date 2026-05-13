using GameApp.DataAccessLayer.Context;
using GameApp.DataAccessLayer.Interfaces;
using GameApp.ModelLayer.Models;
using Npgsql;

namespace GameApp.DataAccessLayer.Repositories
{
    public class HiddenWordRepository : IHiddenWordRepository
    {
        DbContext context;

        public HiddenWordRepository()
        {
            context = new DbContext();
        }

       // fetch and store hidden words in a list
        public List<HiddenWord> GetAllWords()
        {
            List<HiddenWord> words = new();

            NpgsqlConnection connection =
            context.GetConnection();

            string query =
            @"select wordid, word from hiddenwords;";

            NpgsqlCommand command =
            new NpgsqlCommand(query, connection);

            try
            {
                connection.Open();

                NpgsqlDataReader reader =
                command.ExecuteReader();

                while(reader.Read())
                {
                    HiddenWord word =
                    new HiddenWord
                    (
                        reader.GetInt32(0),
                        reader.GetString(1).Trim()
                    );

                    words.Add(word);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine
                (
                    "Error fetching hidden words "
                    + e.Message
                );
            }
            finally
            {
                connection.Close();
            }

            return words;
        }
    }
}