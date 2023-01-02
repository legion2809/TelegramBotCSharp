using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
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
    internal partial class HandleUpdatesAndErrors
    {
        // for /calc command
        static double first_num = 0, second_num = 0;
        static string math_oper = "";
     
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

        // "Cancel" keyboard button logic
        static async Task CancelAction(ITelegramBotClient botClient, Update update, string state)
        {
            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat,
                text: "The action was cancelled.",
                replyMarkup: new ReplyKeyboardRemove());
        }

        #endregion

        #region Handling updates and Telegram API errors

        // Handling Telegram API errors
        public static Task HandleErrors(ITelegramBotClient botClient, Exception exception, CancellationToken token)
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

        public static async Task HandleUpdates(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;

            if (message != null)
            {
                Logger.WriteLog(update);

                if (!SQLStuff.doesUserExist(message.Chat.Id.ToString()))
                {
                    string[] values = { message.Chat.FirstName, message.Chat.LastName, $"({message.Chat.Username})" };
                    string username = string.Join(" ", values);
                    await SQLStuff.AddUser(message.Chat.Id.ToString(), username);
                }

                string state = SQLStuff.ReadDBRecords($"SELECT state FROM users_list WHERE chatID='{message.Chat.Id}'");

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
                                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
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
                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
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
                                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                                    await botClient.SendTextMessageAsync(
                                        chatId: message.Chat,
                                        text: $"Great, result is: {first_num} + {second_num} = {first_num + second_num}",
                                        replyMarkup: new ReplyKeyboardRemove());
                                    break;
                                case "subtraction":
                                    state = "usual";
                                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                                    await botClient.SendTextMessageAsync(
                                        chatId: message.Chat,
                                        text: $"Great, result is: {first_num} - {second_num} = {first_num - second_num}",
                                        replyMarkup: new ReplyKeyboardRemove());
                                    break;
                                case "multiplication":
                                    state = "usual";
                                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
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
                                    }
                                    else
                                    {
                                        state = "usual";
                                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
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
                                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
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
                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                            SQLStuff.UpdateDB($"UPDATE users_list SET send_message_to='{id}' WHERE chatID='{message.Chat.Id}'");

                            string username = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE id='{id}'");

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"OK, you chose this user: '{username}'.\nNow enter the message text you want to send to the user " +
                                $"(you're able to send: photos, stickers, documents, text and voice messages):");
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
                    case "waiting_a_message":

                        state = "usual";

                        string where_send_to = SQLStuff.ReadDBRecords($"SELECT send_message_to FROM users_list WHERE chatID='{message.Chat.Id}'");
                        string chat_id = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{where_send_to}'");

                        SQLStuff.UpdateDB($"UPDATE users_list SET send_message_to=NULL WHERE chatID='{message.Chat.Id}'");
                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");

                        if (message.Type == MessageType.Text)
                        {
                            if (message.Text.ToLower() == "cancel")
                            {
                                break;
                            }

                            await botClient.SendTextMessageAsync(
                                chatId: chat_id,
                                text: $"A message for you from anonymous user: {message.Text}",
                                replyMarkup: new ReplyKeyboardRemove());
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"Message was successfully delivered!",
                                replyMarkup: new ReplyKeyboardRemove());
                        }

                        if (message.Type == MessageType.Document)
                        {
                            var fileId = message.Document.FileId;
                            var fileInfo = await botClient.GetFileAsync(fileId);
                            var filePath = fileInfo.FilePath;

                            DirectoryInfo dir = Directory.CreateDirectory($"..//netcoreapp3.1//VariousTrash//{message.Chat.Id}");

                            string destinationFilePath = $"{dir}//{message.Document.FileName}";

                            await using Stream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                            await botClient.DownloadFileAsync(
                                filePath: filePath,
                                destination: fileStream);
                            fileStream.Close();

                            await botClient.SendDocumentAsync(
                                chatId: chat_id,
                                document: fileId,
                                caption: message.Caption == null ? "A document for you from anonymous user.\n\nCaption is empty."
                                                                 : $"A document for you from anonymous user.\n\nCaption:{message.Caption}",
                                replyMarkup: new ReplyKeyboardRemove());
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"Message was successfully delivered!",
                                replyMarkup: new ReplyKeyboardRemove());

                            System.IO.File.Delete(destinationFilePath);
                        }

                        if (message.Type == MessageType.Photo)
                        {
                            var fileId = message.Photo.Last().FileId;
                            var fileInfo = await botClient.GetFileAsync(fileId);
                            var filePath = fileInfo.FilePath;

                            DirectoryInfo dir = Directory.CreateDirectory($"..//netcoreapp3.1//VariousTrash//{message.Chat.Id}");

                            string destinationFilePath = $"{dir}//{fileId}";

                            await using Stream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                            await botClient.DownloadFileAsync(
                                filePath: filePath,
                                destination: fileStream);
                            fileStream.Close();

                            await botClient.SendPhotoAsync(
                                chatId: chat_id,
                                photo: fileId,
                                caption: message.Caption == null ? "An image (or photo) for you from anonymous user.\n\nCaption is empty."
                                                                 : $"An image (or photo) for you from anonymous user.\n\nCaption:{message.Caption}",
                                replyMarkup: new ReplyKeyboardRemove());
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"Message was successfully delivered!",
                                replyMarkup: new ReplyKeyboardRemove());

                            System.IO.File.Delete(destinationFilePath);
                        }

                        if (message.Type == MessageType.Audio)
                        {
                            var fileId = message.Audio.FileId;
                            var fileInfo = await botClient.GetFileAsync(fileId);
                            var filePath = fileInfo.FilePath;

                            DirectoryInfo dir = Directory.CreateDirectory($"..//netcoreapp3.1//VariousTrash//{message.Chat.Id}");

                            string destinationFilePath = $"{dir}//{message.Audio.FileName}";

                            await using Stream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                            await botClient.DownloadFileAsync(
                                filePath: filePath,
                                destination: fileStream);
                            fileStream.Close();

                            await botClient.SendAudioAsync(
                                chatId: chat_id,
                                audio: fileId,
                                caption: message.Caption == null ? "An audio for you from anonymous user.\n\nCaption is empty."
                                                                 : $"An audio for you from anonymous user.\n\nCaption:{message.Caption}",
                                replyMarkup: new ReplyKeyboardRemove());
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"Message was successfully delivered!",
                                replyMarkup: new ReplyKeyboardRemove());

                            System.IO.File.Delete(destinationFilePath);
                        }

                        if (message.Type == MessageType.Voice)
                        {
                            var fileId = message.Voice.FileId;
                            var fileInfo = await botClient.GetFileAsync(fileId);
                            var filePath = fileInfo.FilePath;

                            DirectoryInfo dir = Directory.CreateDirectory($"..//netcoreapp3.1//VariousTrash//{message.Chat.Id}");

                            string destinationFilePath = $"{dir}//{fileId}";

                            await using Stream writeStream = System.IO.File.OpenWrite(destinationFilePath);
                            await botClient.DownloadFileAsync(
                                filePath: filePath,
                                destination: writeStream);
                            writeStream.Close();

                            await using Stream readStream = System.IO.File.OpenRead(destinationFilePath);
                            await botClient.SendVoiceAsync(
                                chatId: chat_id,
                                voice: readStream,
                                caption: message.Caption == null ? "A voice message for you from anonymous user.\n\nCaption is empty."
                                                                 : $"A voice message for you from anonymous user.\n\nCaption:{message.Caption}",
                                replyMarkup: new ReplyKeyboardRemove());
                            readStream.Close();

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"Message was successfully delivered!",
                                replyMarkup: new ReplyKeyboardRemove());

                            System.IO.File.Delete(destinationFilePath);
                        }

                        if (message.Type == MessageType.Sticker)
                        {
                            var fileId = message.Sticker.FileId;
                            var fileInfo = await botClient.GetFileAsync(fileId);
                            var filePath = fileInfo.FilePath;

                            DirectoryInfo dir = Directory.CreateDirectory($"..//netcoreapp3.1//VariousTrash//{message.Chat.Id}");

                            string destinationFilePath = $"{dir}//{fileId}";

                            await using Stream writeStream = System.IO.File.OpenWrite(destinationFilePath);
                            await botClient.DownloadFileAsync(
                                filePath: filePath,
                                destination: writeStream);
                            writeStream.Close();

                            await using Stream readStream = System.IO.File.OpenRead(destinationFilePath);
                            await botClient.SendStickerAsync(
                                chatId: chat_id,
                                sticker: fileId,
                                replyMarkup: new ReplyKeyboardRemove());
                            readStream.Close();

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"Message was successfully delivered!",
                                replyMarkup: new ReplyKeyboardRemove());

                            System.IO.File.Delete(destinationFilePath);
                        }

                        if (message.Type == MessageType.Video)
                        {
                            var fileId = message.Video.FileId;
                            var fileInfo = await botClient.GetFileAsync(fileId);
                            var filePath = fileInfo.FilePath;

                            DirectoryInfo dir = Directory.CreateDirectory($"..//netcoreapp3.1//VariousTrash//{message.Chat.Id}");

                            string destinationFilePath = $"{dir}//{message.Video.FileName}";

                            await using Stream writeStream = System.IO.File.OpenWrite(destinationFilePath);
                            await botClient.DownloadFileAsync(
                                filePath: filePath,
                                destination: writeStream);
                            writeStream.Close();

                            await using Stream readStream = System.IO.File.OpenRead(destinationFilePath);
                            await botClient.SendVideoAsync(
                                chatId: chat_id,
                                video: fileId,
                                caption: message.Caption == null ? "A video for you from anonymous user.\n\nCaption is empty."
                                                                 : $"A video for you from anonymous user.\n\nCaption:{message.Caption}",
                                replyMarkup: new ReplyKeyboardRemove());
                            readStream.Close();

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"Message was successfully delivered!",
                                replyMarkup: new ReplyKeyboardRemove());

                            System.IO.File.Delete(destinationFilePath);
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
                                switch (message.Text.ToLower())
                                {
                                    case "/start":
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat,
                                            text: $"Hello there, my friend {message.Chat.FirstName} \U0001F44B. I'm a C# Telegram Bot. What can I do for ya, bruh?");
                                        break;
                                    case "/anonmessage":
                                        Dictionary<string, string> users = SQLStuff.TakeUsersList($"SELECT id, username FROM users_list WHERE chatID != '{message.Chat.Id}'");

                                        if (users.Count != 0)
                                        {
                                            string resultset;
                                            string message_text = "List of users to whom you can send a message: \n\n";

                                            foreach (var item in users)
                                            {
                                                resultset = $"(ID: {item.Key}) - '{item.Value}'\n";
                                                message_text = message_text + resultset;
                                            }
                                            message_text = $"{message_text}\nEnter the ID of the user you want to send the message to:";

                                            state = "waiting_for_userID";
                                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");

                                            await botClient.SendTextMessageAsync(
                                                chatId: message.Chat,
                                                text: message_text,
                                                replyMarkup: CancelKeyboardButton());
                                        }
                                        else
                                        {
                                            state = "usual";
                                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");

                                            await botClient.SendTextMessageAsync(
                                                chatId: message.Chat,
                                                text: "Unfortunately, there are no users to whom you can send a message :(",
                                                replyMarkup: new ReplyKeyboardRemove());
                                        }
                                        break;
                                    case "/calc":
                                        state = "choosing_option";
                                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
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
        }
        #endregion
    }
}
