namespace TelegramBot;

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

    #region Send various types of messages to other user
    // Send specific messages to user (for /anonmessage command)
    static async Task SendSpecificMessage(ITelegramBotClient botClient, Update update, string chat_id)
    {
        switch (update.Message.Type)
        {
            case MessageType.Text:
                if (update.Message.Text.ToLower() == "cancel")
                {
                    string state = "usual";
                    await CancelAction(botClient, update, state);
                    break;
                }

                await botClient.SendTextMessageAsync(
                    chatId: chat_id,
                    text: $"A message for you from anonymous user: {update.Message.Text}",
                    replyMarkup: new ReplyKeyboardRemove());
                break;
            case MessageType.Document:
                var fileId = update.Message.Document.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePath = fileInfo.FilePath;

                DirectoryInfo dir = Directory.CreateDirectory($"..//net6.0//VariousTrash//{update.Message.Chat.Id}");

                string destinationFilePath = $"{dir}//{update.Message.Document.FileName}";

                await using (Stream fileStream = System.IO.File.OpenWrite(destinationFilePath))
                {
                    await botClient.DownloadFileAsync(
                        filePath: filePath,
                        destination: fileStream);
                }

                await botClient.SendDocumentAsync(
                    chatId: chat_id,
                    document: fileId,
                    caption: update.Message.Caption == null ? "A document for you from anonymous user.\n\nCaption is empty."
                                                     : $"A document for you from anonymous user.\n\nCaption: {update.Message.Caption}",
                    replyMarkup: new ReplyKeyboardRemove());

                System.IO.File.Delete(destinationFilePath);
                break;
            case MessageType.Photo:
                fileId = update.Message.Photo.Last().FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;

                dir = Directory.CreateDirectory($"..//net6.0//VariousTrash//{update.Message.Chat.Id}");

                destinationFilePath = $"{dir}//{fileId}";

                await using (Stream fileStream = System.IO.File.OpenWrite(destinationFilePath))
                {
                    await botClient.DownloadFileAsync(
                        filePath: filePath,
                        destination: fileStream);
                }

                await botClient.SendPhotoAsync(
                    chatId: chat_id,
                    photo: fileId,
                    caption: update.Message.Caption == null ? "An image (or photo) for you from anonymous user.\n\nCaption is empty."
                                                     : $"An image (or photo) for you from anonymous user.\n\nCaption: {update.Message.Caption}",
                    replyMarkup: new ReplyKeyboardRemove());

                System.IO.File.Delete(destinationFilePath);
                break;
            case MessageType.Audio:
                fileId = update.Message.Audio.FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;

                dir = Directory.CreateDirectory($"..//net6.0//VariousTrash//{update.Message.Chat.Id}");

                destinationFilePath = $"{dir}//{update.Message.Audio.FileName}";

                await using (Stream fileStream = System.IO.File.OpenWrite(destinationFilePath))
                {
                    await botClient.DownloadFileAsync(
                        filePath: filePath,
                        destination: fileStream);
                }

                await botClient.SendAudioAsync(
                    chatId: chat_id,
                    audio: fileId,
                    caption: update.Message.Caption == null ? "An audio for you from anonymous user.\n\nCaption is empty."
                                                     : $"An audio for you from anonymous user.\n\nCaption: {update.Message.Caption}",
                    replyMarkup: new ReplyKeyboardRemove());

                System.IO.File.Delete(destinationFilePath);
                break;
            case MessageType.Voice:
                fileId = update.Message.Voice.FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;

                dir = Directory.CreateDirectory($"..//net6.0//VariousTrash//{update.Message.Chat.Id}");

                destinationFilePath = $"{dir}//{fileId}";

                await using (Stream fileStream = System.IO.File.OpenWrite(destinationFilePath))
                {
                    await botClient.DownloadFileAsync(
                        filePath: filePath,
                        destination: fileStream);
                }

                await using (Stream readStream = System.IO.File.OpenRead(destinationFilePath))
                {
                    await botClient.SendVoiceAsync(
                        chatId: chat_id,
                        voice: readStream,
                        caption: update.Message.Caption == null ? "A voice message for you from anonymous user.\n\nCaption is empty."
                                                         : $"A voice message for you from anonymous user.\n\nCaption: {update.Message.Caption}",
                        replyMarkup: new ReplyKeyboardRemove());
                }

                System.IO.File.Delete(destinationFilePath);
                break;
            case MessageType.Sticker:
                fileId = update.Message.Sticker.FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;

                dir = Directory.CreateDirectory($"..//net6.0//VariousTrash//{update.Message.Chat.Id}");

                destinationFilePath = $"{dir}//{fileInfo.FilePath.Substring(fileInfo.FilePath.IndexOf("/") + 1)}";

                await using (Stream fileStream = System.IO.File.OpenWrite(destinationFilePath))
                {
                    await botClient.DownloadFileAsync(
                        filePath: filePath,
                        destination: fileStream);
                }

                await using (Stream readStream = System.IO.File.OpenRead(destinationFilePath))
                {
                    await botClient.SendStickerAsync(
                        chatId: chat_id,
                        sticker: fileId,
                        replyMarkup: new ReplyKeyboardRemove());
                }

                System.IO.File.Delete(destinationFilePath);
                break;
            case MessageType.Video:
                fileId = update.Message.Video.FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;

                dir = Directory.CreateDirectory($"..//net6.0//VariousTrash//{update.Message.Chat.Id}");

                destinationFilePath = $"{dir}//{update.Message.Video.FileName}";

                await using (Stream fileStream = System.IO.File.OpenWrite(destinationFilePath))
                {
                    await botClient.DownloadFileAsync(
                        filePath: filePath,
                        destination: fileStream);
                }

                await using (Stream readStream = System.IO.File.OpenRead(destinationFilePath))
                {
                    await botClient.SendVideoAsync(
                        chatId: chat_id,
                        video: fileId,
                        caption: update.Message.Caption == null ? "A video for you from anonymous user.\n\nCaption is empty."
                                                         : $"A video for you from anonymous user.\n\nCaption: {update.Message.Caption}",
                        replyMarkup: new ReplyKeyboardRemove());
                }

                System.IO.File.Delete(destinationFilePath);
                break;
            case MessageType.VideoNote:
                fileId = update.Message.VideoNote.FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;

                dir = Directory.CreateDirectory($"..//net6.0//VariousTrash//{update.Message.Chat.Id}");

                destinationFilePath = $"{dir}//{fileInfo.FilePath.Substring(fileInfo.FilePath.IndexOf("/") + 1)}";

                await using (Stream fileStream = System.IO.File.OpenWrite(destinationFilePath))
                {
                    await botClient.DownloadFileAsync(
                        filePath: filePath,
                        destination: fileStream);
                }

                await using (Stream readStream = System.IO.File.OpenRead(destinationFilePath))
                {
                    await botClient.SendVideoNoteAsync(
                        chatId: chat_id,
                        videoNote: fileId,
                        duration: update.Message.VideoNote.Duration,
                        replyMarkup: new ReplyKeyboardRemove());
                }

                System.IO.File.Delete(destinationFilePath);
                break;
            default:
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "This type of message isn't supported. Try the supported one, please.",
                    replyMarkup: new ReplyKeyboardRemove());
                break;
        }
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
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{DateTime.Now}] {ErrorMessage}");
        Console.ResetColor();
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
                            if (message.Text.ToLower() == "cancel")
                            {
                                state = "usual";
                                await CancelAction(botClient, update, state);
                                break;
                            }
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
                            break;
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
                            break;
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
                            break;
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

                    await SendSpecificMessage(botClient, update, chat_id);

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
                                    Random rnd = new Random();
                                    int randNum = rnd.Next(1, 7);

                                    switch (randNum)
                                    {
                                        case 1:
                                            await botClient.SendStickerAsync(
                                                message.Chat,
                                                sticker: "CAACAgIAAxkBAAIEz2OzJXjc3RnGKrFOE_5BPu0gz4-8AALcxgEAAWOLRgyxtRIUSi4a_y0E");
                                            break;
                                        case 2:
                                            await botClient.SendStickerAsync(
                                                message.Chat,
                                                sticker: "CAACAgIAAxkBAAIE0GOzJYBAnry97DeRm1jbw3i8HEOeAALdxgEAAWOLRgzrTyk77CMCUS0E");
                                            break;
                                        case 3:
                                            await botClient.SendStickerAsync(
                                                message.Chat,
                                                sticker: "CAACAgIAAxkBAAIE0WOzJZQ4uxEbX6WyeZM0ih2PFv48AALexgEAAWOLRgxUcf2Fq_sguS0E");
                                            break;
                                        case 4:
                                            await botClient.SendStickerAsync(
                                                message.Chat,
                                                sticker: "CAACAgIAAxkBAAIE0mOzJa7-UnwOy3ZITOqpJYaOP33cAALfxgEAAWOLRgwcRRMg1btjFy0E");
                                            break;
                                        case 5:
                                            await botClient.SendStickerAsync(
                                                message.Chat,
                                                sticker: "CAACAgIAAxkBAAIE02OzJb8Fl8TdvPYb7S1sRB46LLbCAALgxgEAAWOLRgxIsfP6yP8mqS0E");
                                            break;
                                        case 6:
                                            await botClient.SendStickerAsync(
                                                message.Chat,
                                                sticker: "CAACAgIAAxkBAAIE1GOzJeZ3wfH-rxYyF2ZKl6JdlWiuAALhxgEAAWOLRgzvmnzNp7-0ei0E");
                                            break;
                                    }

                                    await botClient.SendTextMessageAsync(
                                        chatId: message.Chat,
                                        text: $"Number rolled: {randNum}");
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
                                        photo: "https://media.discordapp.net/attachments/748112995606986803/1059547380132876328/durka.png",
                                        caption: "<b>Durka, ebat'</b>",
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

