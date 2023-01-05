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
    
    static ReplyKeyboardMarkup YesNoKeyboard()
    {
        ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[]
            {
                "Yes",
                "No"
            }
        });
        return replyKeyboard;
    }

    static ReplyKeyboardMarkup StopKeyboard()
    {
        ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[]
            {
                "Stop"
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
    static async Task SendSpecificMessage(ITelegramBotClient botClient, Update update, string chat_id, string state)
    {
        switch (update.Message.Type)
        {
            case MessageType.Text:
                await botClient.SendTextMessageAsync(
                    chatId: chat_id,
                    text: state == "InConversation" ? update.Message.Text : $"A message for you from anonymous user: <b>{update.Message.Text}</b>",
                    parseMode: ParseMode.Html,
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
                    caption: state == "InConversation" ? update.Message.Text : $"A document for you from anonymous user.\n\n{(update.Message.Caption == null ? "A caption is empty" : update.Message.Caption)}",
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
                    caption: state == "InConversation" ? update.Message.Text : $"A picture (or photo) for you from anonymous user.\n\n{(update.Message.Caption == null ? "A caption is empty" : update.Message.Caption)}",
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
                    caption: state == "InConversation" ? update.Message.Text : $"An audio for you from anonymous user.\n\n{(update.Message.Caption == null ? "A caption is empty" : update.Message.Caption)}",
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
                        caption: state == "InConversation" ? update.Message.Text : $"A voice message for you from anonymous user.\n\n{(update.Message.Caption == null ? "A caption is empty" : update.Message.Caption)}",
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
                        caption: state == "InConversation" ? update.Message.Text : $"A video for you from anonymous user.\n\n{(update.Message.Caption == null ? "A caption is empty" : update.Message.Caption)}",
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
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: "Type the first number, please.");
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
                        return;
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
                                    text: $"Great, result is: <em>{first_num} + {second_num} = {first_num + second_num}</em>",
                                    parseMode: ParseMode.Html,
                                    replyMarkup: new ReplyKeyboardRemove());
                                break;
                            case "subtraction":
                                state = "usual";
                                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                                await botClient.SendTextMessageAsync(
                                    chatId: message.Chat,
                                    text: $"Great, result is: <em>{first_num} - {second_num} = {first_num - second_num}</em>",
                                    parseMode: ParseMode.Html,
                                    replyMarkup: new ReplyKeyboardRemove());
                                break;
                            case "multiplication":
                                state = "usual";
                                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                                await botClient.SendTextMessageAsync(
                                    chatId: message.Chat,
                                    text: $"Great, result is: <em>{first_num} * {second_num} = {first_num * second_num}</em>",
                                    parseMode: ParseMode.Html,
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
                                        text: $"Great, result is: <em>{first_num} / {second_num} = {first_num / second_num}</em>",
                                        parseMode: ParseMode.Html,
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
                                        text: $"Great, result is: <em>{first_num} % {second_num} = {first_num % second_num}</em>",
                                        parseMode: ParseMode.Html,
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
                        return;
                    }
                    break;
                case "waiting_for_userID":
                    state = "waiting_a_message";

                    try
                    {
                        int id = Convert.ToInt32(message.Text);
                        string companion_chat_id = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{id}'");

                        if (companion_chat_id == message.Chat.Id.ToString() || companion_chat_id == null)
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: "You typed a wrong ID! Type the right one, please.");
                            break;
                        }

                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                        SQLStuff.UpdateDB($"UPDATE users_list SET send_message_to='{id}' WHERE chatID='{message.Chat.Id}'");

                        string username = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE id='{id}'");

                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: $"OK, you chose this user: <em>{username}</em>.\nNow enter the message text you want to send to the user " +
                            $"(you're able to send: photos, stickers, documents, text and voice messages):",
                            parseMode: ParseMode.Html);
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
                        return;
                    }
                    break;
                case "waiting_a_message":

                    state = "usual";

                    if (update.Message.Text.ToLower() == "cancel")
                    {
                        state = "usual";
                        await CancelAction(botClient, update, state);
                        break;
                    }

                    string where_send_to = SQLStuff.ReadDBRecords($"SELECT send_message_to FROM users_list WHERE chatID='{message.Chat.Id}'");
                    string chat_id = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{where_send_to}'");

                    SQLStuff.UpdateDB($"UPDATE users_list SET send_message_to=NULL WHERE chatID='{message.Chat.Id}'");
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");

                    await SendSpecificMessage(botClient, update, chat_id, state);

                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: $"Message was successfully delivered!",
                        replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "choosing_companion":
                    state = "waiting_for_response";
                    try
                    {
                        int companion = Convert.ToInt32(message.Text);
                        string id = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{message.Chat.Id}'");
                        string companion_chat_id = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{companion}'");
                        string stateChecking = SQLStuff.ReadDBRecords($"SELECT state FROM users_list WHERE id='{companion}'");

                        if (companion_chat_id == message.Chat.Id.ToString() || companion_chat_id == null)
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: "You typed a wrong ID! Type the right one, please.");
                            break;
                        }

                        if (stateChecking == "In_Conversation" || stateChecking == "choosing_yes_or_no" || stateChecking == "waiting_for_response")
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: "This user currently isn't available for conversation.");
                            break;
                        }

                        SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with='{companion}' WHERE chatID='{message.Chat.Id}'");
                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");

                        state = "choosing_yes_or_no";
                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{companion}'");

                        string username = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE id='{companion}'");
                        string offererUName = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE chatID='{message.Chat.Id}'");

                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: $"OK, you chose this user: <em>'{username}'</em>.\nWait for his/her response.",
                            parseMode: ParseMode.Html);
                        await botClient.SendTextMessageAsync(
                            chatId: companion_chat_id,
                            text: $"User <em>{offererUName}</em> has sent an offer to you to start a conversation. Wanna talk with him/her?",
                            parseMode: ParseMode.Html,
                            replyMarkup: YesNoKeyboard());
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
                        return;
                    }
                    break;
                case "choosing_yes_or_no":
                    switch (message.Text.ToLower()) 
                    {
                        case "yes":
                            state = "InConversation";

                            string oID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{message.Chat.Id}'");
                            string cID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE messaging_with='{oID}'");

                            string cChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{cID}'");
                            string uName = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE chatID='{message.Chat.Id}'");

                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                            SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with='{cID}' WHERE chatID='{message.Chat.Id}'");
                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{cID}'");

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: "You accepted an offer!\n\nIn order to stop a conversation just type <b>'stop'</b> (in any case).",
                                parseMode: ParseMode.Html,
                                replyMarkup: StopKeyboard());
                            await botClient.SendTextMessageAsync(
                                chatId: cChatID,
                                text: $"<b>{uName}</b> accepted an offer to start a conversation!\n\nIn order to stop a conversation just type <b>'stop'</b> (in any case).",
                                parseMode: ParseMode.Html,
                                replyMarkup: StopKeyboard());
                            break;
                        case "no":
                            state = "usual";

                            string offererID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{message.Chat.Id}'");
                            string companionID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE messaging_with='{offererID}'");

                            string companionChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{companionID}'");
                            string user_name = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE chatID='{message.Chat.Id}'");

                            SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE chatID='{companionID}'");
                            SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE chatID='{message.Chat.Id}'");
                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{companionID}'");

                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: "You declined an offer to start a conversation!",
                                parseMode: ParseMode.Html,
                                replyMarkup: new ReplyKeyboardRemove()); 
                            await botClient.SendTextMessageAsync(
                                chatId: companionChatID,
                                text: $"<b>{user_name}</b> has declined your offer to start a conversation. Have a pleasant day!",
                                parseMode: ParseMode.Html,
                                replyMarkup: new ReplyKeyboardRemove());
                            break;
                        default:
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: "Type only <b>'Yes'</b> or <b>'No'</b>!",
                                parseMode: ParseMode.Html);
                            break;
                    }
                    break;
                case "waiting_for_response":
                    if (message.Text.ToLower() == "cancel")
                    {
                        state = "usual";
                        string of_ID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{message.Chat.Id}'");
                        string co_ID = SQLStuff.ReadDBRecords($"SELECT messaging_with FROM users_list WHERE id='{of_ID}'");
                        string co_ChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{co_ID}'");

                        SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE id='{co_ID}'");
                        SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE chatID='{message.Chat.Id}'");
                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{co_ID}'");

                        await botClient.SendTextMessageAsync(
                            chatId: co_ChatID,
                            text: "An offer to start a conversation was cancelled",
                            replyMarkup: new ReplyKeyboardRemove());
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: "You cancelled your offer to start a conversation!",
                            replyMarkup: new ReplyKeyboardRemove());
                    }
                    break;
                case "InConversation":
                    string offerer_ID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{message.Chat.Id}'");
                    string companion_ID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE messaging_with='{offerer_ID}'");
                    string companion_ChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{companion_ID}'");
                    string user_Name = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE id='{offerer_ID}'");

                    switch (message.Type)
                    {
                        case MessageType.Text:
                            if (message.Text.ToLower() == "stop")
                            {
                                state = "usual";

                                SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE id='{companion_ID}'");
                                SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE chatID='{message.Chat.Id}'");
                                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");
                                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{companion_ID}'");

                                await botClient.SendTextMessageAsync(
                                    chatId: companion_ChatID,
                                    text: $"<b>{user_Name}</b> has left from the conversation. Have a pleasant day!",
                                    parseMode: ParseMode.Html,
                                    replyMarkup: new ReplyKeyboardRemove());
                                await botClient.SendTextMessageAsync(
                                    chatId: message.Chat,
                                    text: "You left from the conversation!",
                                    replyMarkup: new ReplyKeyboardRemove());
                                break;
                            }
                            await SendSpecificMessage(botClient, update, companion_ChatID, state);
                            break;
                        default:
                            await SendSpecificMessage(botClient, update, companion_ChatID, state);
                            break;
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
                                            resultset = $"<b>(ID: {item.Key}) - '{item.Value}'</b>\n";
                                            message_text = message_text + resultset;
                                        }
                                        message_text = $"{message_text}\nEnter the ID of the user you want to send the message to:";

                                        state = "waiting_for_userID";
                                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");

                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat,
                                            text: message_text,
                                            parseMode: ParseMode.Html,
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
                                case "/startconv":
                                    users = SQLStuff.TakeUsersList($"SELECT id, username FROM users_list WHERE chatID != '{message.Chat.Id}'");

                                    if (users.Count != 0)
                                    {
                                        string resultset;
                                        string message_text = "List of users with whom you can start a conversation: \n\n";

                                        foreach (var item in users)
                                        {
                                            resultset = $"<b>(ID: {item.Key}) - '{item.Value}'</b>\n";
                                            message_text = message_text + resultset;
                                        }
                                        message_text = $"{message_text}\nEnter the ID of the user with whom you want to start a conversation:";

                                        state = "choosing_companion";
                                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{message.Chat.Id}'");

                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat,
                                            text: message_text,
                                            parseMode: ParseMode.Html,
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
                                case "/today":
                                    CultureInfo ci = new CultureInfo("en-US");
                                    var date = DateTime.Now.ToString("dddd, dd MMMM yyyy", ci);
                                    await botClient.SendTextMessageAsync(
                                        chatId: message.Chat,
                                        text: $"Today's date is: {date}");
                                    break;
                                case "witam":
                                case "hello":
                                case "hi":
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

