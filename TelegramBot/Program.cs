namespace TelegramBot;

internal partial class Program
{
    const string token = "5874882825:AAFi9wC0xJfy0hKdzCiZGyv6Rz1BMs6C1Fo";

    static void Main(string[] args)
    {
        var botClient = new TelegramBotClient(token);

        Console.WriteLine($"[{DateTime.Now}] Bot {botClient.GetMeAsync().Result.Username} was successfully launched.", Console.ForegroundColor = ConsoleColor.DarkGreen);
        Console.ResetColor();

        var connection = new SQLiteConnection();

        // Checking a connection with SQLite database
        try
        {
            if (!System.IO.File.Exists(SQLStuff.dbName))
            {
                connection = new SQLiteConnection($"Data Source={SQLStuff.dbName};Version=3;New=True;Compress=True;");
                SQLiteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "CREATE TABLE users_list (\r\n" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT,\r\n" +
                    "chatID   VARCHAR (100) UNIQUE ON CONFLICT IGNORE,\r\n" +
                    "username VARCHAR (200),\r\n" +
                    "state    VARCHAR (70),\r\n" +
                    "send_message_to INTEGER\r\n);";
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            else
            {
                connection = new SQLiteConnection($"Data Source={SQLStuff.dbName};");
            }

            connection.Open();

            Console.WriteLine($"[{DateTime.Now}] Successfully connected to SQLite database!", Console.ForegroundColor = ConsoleColor.Green);
            Console.ResetColor();

            connection.Close();

            // Starting receiving updates from Telegram bot
            botClient.StartReceiving(HandleUpdatesAndErrors.HandleUpdates, HandleUpdatesAndErrors.HandleErrors);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now}] {ex.Message}");
        }

        Console.ReadKey();
    }
}
