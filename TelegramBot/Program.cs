using System;
using System.Collections.Generic;
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

        // Reading users list
        static Dictionary<string, string> TakeUsersList(string query)
        {
            Dictionary<string, string> users = new Dictionary<string, string>();

            var DB = new SQLiteConnection($"Data Source={dbName};");
            DB.Open();
            SQLiteCommand command = DB.CreateCommand();
            command.CommandText = query;

            SQLiteDataReader dataReader = command.ExecuteReader();

            if (dataReader.HasRows)
            {
                while(dataReader.Read())
                {
                    string id = dataReader.GetValue(0).ToString();
                    string username = dataReader.GetValue(1).ToString();
                    users.Add(id, username);
                }
            }

            dataReader.Close();
            DB.Close();

            return users;
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
                command.CommandText = "INSERT INTO users_list VALUES (NULL, @chatID, @username, @state, @send_message_to)";
                command.Parameters.AddWithValue("@chatID", chatID);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@state", "usual");
                command.Parameters.AddWithValue("@send_message_to", null);
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
                chatId: update.Message.Chat,
                text: "The action was cancelled.",
                replyMarkup: new ReplyKeyboardRemove());
        }

        static async Task HandleUpdates(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;

            if (message != null)
            {

                if (!doesUserExist(message.Chat.Id.ToString()))
                {
                    string[] values = { message.Chat.FirstName, message.Chat.LastName, $"({message.Chat.Username})" };
                    string username = string.Join(" ", values);
                    await AddUser(message.Chat.Id.ToString(), username);
                }

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
                                UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
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
                            UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
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
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: "You didn't type a number!");
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
                                    UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                                    await botClient.SendTextMessageAsync(
                                        chatId: message.Chat,
                                        text: $"Great, result is: {first_num} + {second_num} = {first_num + second_num}",
                                        replyMarkup: new ReplyKeyboardRemove());
                                    break;
                                case "subtraction":
                                    state = "usual";
                                    UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                                    await botClient.SendTextMessageAsync(
                                        chatId: message.Chat,
                                        text: $"Great, result is: {first_num} - {second_num} = {first_num - second_num}",
                                        replyMarkup: new ReplyKeyboardRemove());
                                    break;
                                case "multiplication":
                                    state = "usual";
                                    UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                                    await botClient.SendTextMessageAsync(
                                        chatId: message.Chat,
                                        text: $"Great, result is: {first_num} * {second_num} = {first_num * second_num}",
                                        replyMarkup: new ReplyKeyboardRemove());
                                    break;
                                case "division":
                                    if (second_num == 0)
                                    {
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat,
                                            text: "Seems like a 'division by zero' attempt. Type the a number that is different from 0.");
                                    } else
                                    {
                                        state = "usual";
                                        UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat,
                                            text: $"Great, result is: {first_num} / {second_num} = {first_num / second_num}",
                                            replyMarkup: new ReplyKeyboardRemove());
                                    }
                                    break;
                                case "modulo":
                                    if (second_num == 0)
                                    {
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat,
                                            text: "Seems like a 'division by zero' attempt. Type the a number that is different from 0.");
                                    }
                                    else
                                    {
                                        state = "usual";
                                        UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat,
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
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: "You didn't type a number!");
                        }
                        break;
                    case "waiting_for_userID":
                        state = "waiting_a_message";

                        try
                        {
                            int id = Convert.ToInt32(message.Text);
                            UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                            UpdateDB($"UPDATE users_list SET send_message_to='{id}' WHERE chatID='{message.Chat.Id}'");

                            string username = ReadDBRecords($"SELECT username FROM users_list WHERE id='{id}'");

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"OK, you chose this user: {username}.\nNow enter the message text you want to send to the user: ");
                        } catch (FormatException)
                        {
                            if (message.Text.ToLower() == "cancel")
                            {
                                state = "usual";
                                await CancelAction(botClient, update, state);
                                return;
                            }
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: "You didn't type a number!");
                        }


                        break;
                    case "waiting_a_message":
                        state = "usual";

                        string where_send_to = ReadDBRecords($"SELECT send_message_to FROM users_list WHERE chatID='{message.Chat.Id}'");
                        string chat_id = ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{where_send_to}'");

                        UpdateDB($"UPDATE users_list SET send_message_to=NULL WHERE chatID='{message.Chat.Id}'");
                        UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");

                        await botClient.SendTextMessageAsync(
                            chatId: chat_id,
                            text: $"A message for you from anonymous user: {message.Text}",
                            replyMarkup: new ReplyKeyboardRemove());
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: $"Message was successfully delivered!", 
                            replyMarkup: new ReplyKeyboardRemove());
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
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat,
                                            text: $"Hello there, my friend {message.Chat.FirstName} \U0001F44B. I'm a C# Telegram Bot. What can I do for ya, bruh?");
                                        break;
                                    case "/anonmessage":
                                        Dictionary<string, string> users = TakeUsersList($"SELECT id, username FROM users_list WHERE chatID != '{message.Chat.Id}'");

                                        if (users.Count != 0)
                                        {
                                            string resultset;
                                            string message_text = "List of users to whom you can send a message: \n\n";

                                            foreach (var item in users)
                                            {
                                                resultset = $"(ID: {item.Key}) {item.Value}\n";
                                                message_text = message_text + resultset;
                                            }
                                            message_text = $"{message_text}\nEnter the ID of the user you want to send the message to:";

                                            state = "waiting_for_userID";
                                            UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");

                                            await botClient.SendTextMessageAsync(
                                                chatId: message.Chat,
                                                text: message_text,
                                                replyMarkup: CancelKeyboardButton());
                                        } else
                                        {
                                            state = "usual";
                                            UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");

                                            await botClient.SendTextMessageAsync(
                                                chatId: message.Chat,
                                                text: "Unfortunately, there are no users to whom you can send a message :(",
                                                replyMarkup: new ReplyKeyboardRemove());
                                        }
                                        break;
                                    case "/calc":
                                        state = "choosing_option";
                                        UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat,
                                            text: "Choose one of the options below:",
                                            replyMarkup: MathOperationButtons());
                                        break;
                                    case "/dice":
                                        await botClient.SendDiceAsync(message.Chat);
                                        break;
                                    case "/link":
                                        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]{
                                            new [] {
                                                InlineKeyboardButton.WithUrl(text: "GitHub", url: "https://github.com/legion2809"),
                                                InlineKeyboardButton.WithUrl(text: "Instagram", url: "https://instagram.com/sh_yerkanat")
                                            }
                                        });
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat,
                                            text: "Here are your links \U00002B07",
                                            replyMarkup: inlineKeyboard);
                                        break;
                                    case "/pic":
                                        await botClient.SendPhotoAsync(
                                            chatId: message.Chat,
                                            photo: "https://raw.githubusercontent.com/TelegramBots/book/master/src/docs/photo-ara.jpg",
                                            caption: "<b>Ara bird</b>. <i>Source</i>: <a href=\"https://pixabay.com\">Pixabay</a>",
                                            parseMode: ParseMode.Html);
                                        break;
                                    case "/today":
                                        CultureInfo ci = new CultureInfo("en-US");
                                        var date = DateTime.Now.ToString("dddd, dd MMMM yyyy", ci);
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat,
                                            text: $"Today's date is: {date}");
                                        break;
                                    case "addition":
                                    case "multiplication":
                                    case "division":
                                    case "subtraction":
                                    case "modulo":
                                        await botClient.SendTextMessageAsync(
                                           chatId: message.Chat,
                                           text: "Type the first number, please.",
                                           replyMarkup: CancelKeyboardButton());
                                        break;
                                    case "cancel":
                                        state = "usual";
                                        await CancelAction(botClient, update, state);
                                        break;
                                    case "witam":
                                        await botClient.SendTextMessageAsync(
                                           chatId: message.Chat,
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
                                    chatId: message.Chat,
                                    text: "Oh, nice sticker, mate \U0001F44C");
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
