using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelegramBot
{
    internal partial class SQLStuff
    {
        public static string dbName = "users.db";

        static void SQLQueriesLog(string query_text)
        {
            Console.WriteLine($"[{DateTime.Now}] A following SQL query was executed: {query_text}", Console.ForegroundColor = ConsoleColor.Yellow);
            Console.ResetColor();
        }

        public static bool doesUserExist(string chatID)
        {
            var DB = new SQLiteConnection($"Data Source={dbName};");
            DB.Open();
            SQLiteCommand command = DB.CreateCommand();
            command.CommandText = "SELECT count(*) FROM users_list WHERE chatID=@chatID";
            string query = command.CommandText;
            command.Parameters.AddWithValue("@chatID", chatID);

            int count = Convert.ToInt32(command.ExecuteScalar());
            DB.Close();

            SQLQueriesLog(query);

            if (count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // For executing SELECT queries
        public static string ReadDBRecords(string query)
        {
            var DB = new SQLiteConnection($"Data Source={dbName};");
            DB.Open();
            SQLiteCommand command = DB.CreateCommand();
            command.CommandText = query;

            if (command.ExecuteScalar() != null)
            {
                string resultset = command.ExecuteScalar().ToString();
                command.ExecuteNonQuery();
                DB.Close();

                return resultset;
            }

            SQLQueriesLog(query);
            return null;
        }

        // Reading users list
        public static Dictionary<string, string> TakeUsersList(string query)
        {
            Dictionary<string, string> users = new Dictionary<string, string>();

            var DB = new SQLiteConnection($"Data Source={dbName};");
            DB.Open();
            SQLiteCommand command = DB.CreateCommand();
            command.CommandText = query;

            SQLiteDataReader dataReader = command.ExecuteReader();

            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    string id = dataReader.GetValue(0).ToString();
                    string username = dataReader.GetValue(1).ToString();
                    users.Add(id, username);
                }
            }

            dataReader.Close();
            DB.Close();

            SQLQueriesLog(query);
            return users;
        }

        // For executing UPDATE queries
        public static void UpdateDB(string query)
        {
            try
            {
                var DB = new SQLiteConnection($"Data Source={dbName};");
                DB.Open();
                SQLiteCommand command = DB.CreateCommand();
                command.CommandText = query;
                command.ExecuteNonQuery();
                DB.Close();
                SQLQueriesLog(query);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Pizdec, an error occured: {ex.Message}");
            }
        }

        // Add a record to "users_list" table with a user's data (chat ID, username and state)
        public static Task AddUser(string chatID, string username)
        {
            try
            {
                // Adding a user to a "users" table
                var DB = new SQLiteConnection($"Data Source={dbName};");
                DB.Open();
                SQLiteCommand command = DB.CreateCommand();
                command.CommandText = "INSERT INTO users_list VALUES (NULL, @chatID, @username, @state, @send_message_to)";
                string query = command.CommandText;
                command.Parameters.AddWithValue("@chatID", chatID);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@state", "usual");
                command.Parameters.AddWithValue("@send_message_to", null);
                command.ExecuteNonQuery();
                DB.Close();
                SQLQueriesLog(query);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Pizdec, an error occured: {ex.Message}");
            }
            return Task.CompletedTask;
        }
    }
}
