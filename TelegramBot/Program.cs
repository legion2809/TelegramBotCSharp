using System;
using System.Data.SQLite;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class Program
    {
        const string token = "5874882825:AAFi9wC0xJfy0hKdzCiZGyv6Rz1BMs6C1Fo";
        const string dbName = "users.db";

        // for /calc command
        static double first_num = 0, second_num = 0;
        static string math_oper = "";

        #region "Main" method

        static void Main(string[] args)
        {
            // Starting receiving updates from Telegram bot
            var botClient = new TelegramBotClient(token);
            botClient.StartReceiving(HandleUpdates, HandleErrors);

            Console.WriteLine($"[{DateTime.Now}] Bot {botClient.GetMeAsync().Result.Username} was successfully launched.");
            
            // Checking a connection with SQLite database
            var connection = new SQLiteConnection($"Data Source={dbName};Version=3;New=True;Compress=True;");

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
        #endregion

        #region SQL-ish shit

        // Checking if a user exists in database
        static bool doesUserExist(string chatID)
        {
            var DB = new SQLiteConnection($"Data Source={dbName};");
            DB.Open();
            SQLiteCommand command = DB.CreateCommand();
            command.CommandText = "SELECT count(*) FROM users_list WHERE chatID=@chatID";
            command.Parameters.AddWithValue("@chatID", chatID);

            int count = Convert.ToInt32(command.ExecuteScalar());
            DB.Close();

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
        static string ReadDBRecords(string query)
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

            return null;
        }

        // For executing UPDATE queries
        static void UpdateDB(string query)
        {
            try
            {
                var DB = new SQLiteConnection($"Data Source={dbName};");
                DB.Open();
                SQLiteCommand command = DB.CreateCommand();
                command.CommandText = query;
                command.ExecuteNonQuery();
                DB.Close();
            } catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Pizdec, an error occured: {ex.Message}");
            }
        }

        // Add a record to "users_list" table with a user's data (chat ID, username and state)
        static Task AddUser(string chatID, string username)
        {
            try
            {
                // Adding a user to a "users" table
                var DB = new SQLiteConnection($"Data Source={dbName};");
                DB.Open();
                SQLiteCommand command = DB.CreateCommand();
                command.CommandText = "INSERT INTO users_list VALUES (@chatID, @username, @state)";
                command.Parameters.AddWithValue("@chatID", chatID);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@state", "usual");
                command.ExecuteNonQuery();
                DB.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Pizdec, an error occured: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        #endregion

        #region Reply and inline keyboards

        static ReplyKeyboardMarkup MathOperationButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[]
                {
                    "Addition", "Subtraction", "Multiplication"
                },
                new KeyboardButton[]
                {
                    "Division", "Modulo"
                },
                new KeyboardButton[]
                {
                    "Cancel"
                }
            });
            return replyKeyboard;
        }

        static ReplyKeyboardMarkup CancelKeyboardButton()
        {
            ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[]
                {
                    "Cancel"
                }
            });
            return replyKeyboard;
        }

        #endregion

        #region Handling updates and Telegram API errors
        static Task HandleErrors(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error (Code {apiRequestException.ErrorCode}): '{apiRequestException.Message}'",
                _ => exception.ToString()
            };
            Console.WriteLine($"[{DateTime.Now}] {ErrorMessage}");
            return Task.CompletedTask;
        }

        static async Task CancelAction(ITelegramBotClient botClient, Update update, string state)
        {
            UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "The action was cancelled.",
                replyMarkup: new ReplyKeyboardRemove());
        }

        static async Task HandleUpdates(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;

            if (message != null)
            {
                string state = ReadDBRecords($"SELECT state FROM users_list WHERE chatID='{message.Chat.Id}'");

                switch (state)
                {
                    case "choosing_option":
                        switch (message.Text.ToLower())
                        {
                            case "addition":
                            case "subtraction":
                            case "multiplication":
                            case "division":
                            case "modulo":
                                state = "typefirstnum";
                                math_oper = message.Text.ToLower();
                                UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                break;
                            default:
                                break;
                        }
                        break;
                    case "typefirstnum":
                        try
                        {
                            state = "typesecondnum";
                            double result = Convert.ToDouble(update.Message.Text);
                            first_num = result;
                            UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                            await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: "Good, now type the second one.");
                        }
                        catch (FormatException)
                        {
                            if (message.Text.ToLower() == "cancel")
                            {
                                state = "usual";
                                await CancelAction(botClient, update, state);
                                return;
                            }
                        }
                        break;
                    case "typesecondnum":
                        try
                        {
                            double result = Convert.ToDouble(update.Message.Text);
                            second_num = result;
                            
                            switch (math_oper)
                            {
                                case "addition":
                                    state = "usual";
                                    UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                    await botClient.SendTextMessageAsync(
                                        chatId: update.Message.Chat.Id,
                                        text: $"Great, result is: {first_num} + {second_num} = {first_num + second_num}",
                                        replyMarkup: new ReplyKeyboardRemove());
                                    break;
                                case "subtraction":
                                    state = "usual";
                                    UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                    await botClient.SendTextMessageAsync(
                                        chatId: update.Message.Chat.Id,
                                        text: $"Great, result is: {first_num} - {second_num} = {first_num - second_num}",
                                        replyMarkup: new ReplyKeyboardRemove());
                                    break;
                                case "multiplication":
                                    state = "usual";
                                    UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                    await botClient.SendTextMessageAsync(
                                        chatId: update.Message.Chat.Id,
                                        text: $"Great, result is: {first_num} * {second_num} = {first_num * second_num}",
                                        replyMarkup: new ReplyKeyboardRemove());
                                    break;
                                case "division":
                                    if (second_num == 0)
                                    {
                                        await botClient.SendTextMessageAsync(
                                            chatId: update.Message.Chat.Id,
                                            text: "Seems like a 'division by zero' attempt. Type the a number that is different from 0.");
                                    } else
                                    {
                                        state = "usual";
                                        UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                        await botClient.SendTextMessageAsync(
                                            chatId: update.Message.Chat.Id,
                                            text: $"Great, result is: {first_num} / {second_num} = {first_num / second_num}",
                                            replyMarkup: new ReplyKeyboardRemove());
                                    }
                                    break;
                                case "modulo":
                                    if (second_num == 0)
                                    {
                                        await botClient.SendTextMessageAsync(
                                            chatId: update.Message.Chat.Id,
                                            text: "Seems like a 'division by zero' attempt. Type the a number that is different from 0.");
                                    }
                                    else
                                    {
                                        state = "usual";
                                        UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                        await botClient.SendTextMessageAsync(
                                            chatId: update.Message.Chat.Id,
                                            text: $"Great, result is: {first_num} % {second_num} = {first_num % second_num}",
                                            replyMarkup: new ReplyKeyboardRemove());
                                    }
                                    break;
                            }
                        }
                        catch (FormatException)
                        {
                            if (message.Text.ToLower() == "cancel")
                            {
                                state = "usual";
                                await CancelAction(botClient, update, state);
                                return;
                            }
                        }
                        break;
                    default:
                        break;
                }

                switch (update.Type)
                {
                    case UpdateType.Message:
                        switch (message.Type)
                        {
                            case MessageType.Text:
                                Console.WriteLine($"[{DateTime.Now}] User '{message.Chat.FirstName}' (ID: {message.Chat.Id}) has sent the following message: '{message.Text}'");
                                switch (message.Text.ToLower())
                                {
                                    case "/start":
                                        if (!doesUserExist(message.Chat.Id.ToString()))
                                        {
                                            string[] values = { message.Chat.FirstName, message.Chat.LastName, $"({message.Chat.Username})" };
                                            string username = string.Join(" ", values);
                                            await AddUser(message.Chat.Id.ToString(), username);
                                        }
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            text: $"Hello there, my friend {message.Chat.FirstName} \U0001F44B. I'm a C# Telegram Bot. What can I do for ya, bruh?");
                                        break;
                                    case "/calc":
                                        state = "choosing_option";
                                        UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            text: "Choose one of the options below:",
                                            replyMarkup: MathOperationButtons());
                                        break;
                                    case "/dice":
                                        await botClient.SendDiceAsync(message.Chat.Id);
                                        break;
                                    case "/link":
                                        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]{
                                            new [] {
                                                InlineKeyboardButton.WithUrl(text: "GitHub", url: "https://github.com/legion2809"),
                                                InlineKeyboardButton.WithUrl(text: "Instagram", url: "https://instagram.com/sh_yerkanat")
                                            }
                                        });
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            text: "Here are your links \U00002B07",
                                            replyMarkup: inlineKeyboard);
                                        break;
                                    case "/pic":
                                        await botClient.SendPhotoAsync(
                                            chatId: message.Chat.Id,
                                            photo: "https://raw.githubusercontent.com/TelegramBots/book/master/src/docs/photo-ara.jpg",
                                            caption: "<b>Ara bird</b>. <i>Source</i>: <a href=\"https://pixabay.com\">Pixabay</a>",
                                            parseMode: ParseMode.Html);
                                        break;
                                    case "/today":
                                        CultureInfo ci = new CultureInfo("en-US");
                                        var date = DateTime.Now.ToString("dddd, dd MMMM yyyy", ci);
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            text: $"Today's date is: {date}");
                                        break;
                                    case "addition":
                                    case "multiplication":
                                    case "division":
                                    case "subtraction":
                                    case "modulo":
                                        await botClient.SendTextMessageAsync(
                                           chatId: message.Chat.Id,
                                           text: "Type the first number, please.",
                                           replyMarkup: CancelKeyboardButton());
                                        break;
                                    case "cancel":
                                        state = "usual";
                                        await CancelAction(botClient, update, state);
                                        break;
                                    case "witam":
                                        await botClient.SendTextMessageAsync(
                                           chatId: message.Chat.Id,
                                           text: "Pritam, dude \U0001F44B.",
                                           replyToMessageId: message.MessageId);
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case MessageType.Sticker:
                                Console.WriteLine($"[{DateTime.Now}] User '{message.Chat.FirstName}' (ID: {message.Chat.Id}) has sent the following sticker with ID: '{message.Sticker.FileUniqueId}'");
                                await botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: "Oh, nice sticker, mate \U00001F44C");
                                break;
                            default:
                                break;
                        }
                        break;
                    case UpdateType.CallbackQuery:
                        var pressedButtonData = update.CallbackQuery.Data;
                        Console.WriteLine($"[{DateTime.Now}] User '{update.CallbackQuery.Message.Chat.FirstName}' (ID: {update.CallbackQuery.Message.Chat.Id}) has pressed the button with ID: '{pressedButtonData}'");
                        switch (pressedButtonData)
                        {
                            default:
                                break;
                        }
                        break;
                }
            } 
            #endregion

        }
    }
}
