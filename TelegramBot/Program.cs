using System;
using System.Data.SQLite;
using Telegram.Bot;

namespace TelegramBot
{
    internal partial class Program
    {
        const string token = "5874882825:AAFi9wC0xJfy0hKdzCiZGyv6Rz1BMs6C1Fo";

        static void Main(string[] args)
        {
            // Starting receiving updates from Telegram bot
            var botClient = new TelegramBotClient(token);
            botClient.StartReceiving(HandleUpdatesAndErrors.HandleUpdates, HandleUpdatesAndErrors.HandleErrors);

            Console.WriteLine($"[{DateTime.Now}] Bot {botClient.GetMeAsync().Result.Username} was successfully launched.");

            // Checking a connection with SQLite database
            var connection = new SQLiteConnection($"Data Source={SQLStuff.dbName};Version=3;New=True;Compress=True;");

            try
            {
                connection.Open();
                Console.WriteLine($"[{DateTime.Now}] Successfully connected to SQLite database!");
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] {ex.Message}");
            }

            Console.ReadKey();
        }
    }
}
