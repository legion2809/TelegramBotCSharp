namespace TelegramBot;

internal partial class HandleUpdatesAndErrors
{
    // for /calc command
    static double first_num = 0, second_num = 0;
    static string math_oper = "";

    #region Reply and inline keyboards
    static InlineKeyboardMarkup LinksInlineKeyboard()
    {
        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new []
            {
                InlineKeyboardButton.WithUrl(text: "GitHub", url: "https://github.com/legion2809"),
                InlineKeyboardButton.WithUrl(text: "Instagram", url: "https://instagram.com/sh_yerkanat"),
                InlineKeyboardButton.WithUrl(text: "Telegram", url: "https://t.me/yerkanat_s")
            }
        });
        return inlineKeyboard;
    }

    static InlineKeyboardMarkup OptionsWithFilesInlineKeyboard()
    {
        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "See my files list", callbackData: "SeeFilesList"),
                InlineKeyboardButton.WithCallbackData(text: "Delete a file", callbackData: "DeleteFile")
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Upload a file", callbackData: "UploadFile"),
                InlineKeyboardButton.WithCallbackData(text: "Download a file", callbackData: "DownloadFile")
            }
        });
        return inlineKeyboard;
    }

    static InlineKeyboardMarkup BackToSettingsInlineKeyboard()
    {
        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "<< Back to Settings", callbackData: "BackToFilesSettings")
            }
        });
        return inlineKeyboard;
    }

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
                    text: "This type of message isn't supported. Try the supported one next time, please.",
                    replyMarkup: state == "InConversation" ? null : new ReplyKeyboardRemove());
                break;
        }
    }
    #endregion

    #region Processing state machine and messages from the user
    static async Task ProcessingStates(string state, ITelegramBotClient botClient, Update update)
    {
        switch (state)
        {
            case "choosing_option":
                if (update.Type != UpdateType.Message)
                {
                    return;
                }
                switch (update.Message.Text.ToLower())
                {
                    case "addition":
                    case "subtraction":
                    case "multiplication":
                    case "division":
                    case "modulo":
                        state = "typefirstnum";
                        math_oper = update.Message.Text.ToLower();
                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat,
                            text: "Type the first number, please.",
                            replyMarkup: CancelKeyboardButton());
                        break;
                    default:
                        if (update.Message.Text.ToLower() == "cancel")
                        {
                            state = "usual";
                            await CancelAction(botClient, update, state);
                            break;
                        }
                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat,
                            text: "Wrong option!");
                        break;
                }
                break;
            case "typefirstnum":
                try
                {
                    if (update.Type != UpdateType.Message)
                    {
                        return;
                    }
                    state = "typesecondnum";
                    double result = Convert.ToDouble(update.Message.Text);
                    first_num = result;
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "Good, now type the second one.",
                        replyMarkup: CancelKeyboardButton());
                }
                catch (FormatException)
                {
                    if (update.Message.Text.ToLower() == "cancel")
                    {
                        state = "usual";
                        await CancelAction(botClient, update, state);
                        break;
                    }
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "You didn't type a number!");
                    return;
                }
                break;
            case "typesecondnum":
                try
                {
                    if (update.Type != UpdateType.Message)
                    {
                        return;
                    }

                    double result = Convert.ToDouble(update.Message.Text);
                    second_num = result;

                    switch (math_oper)
                    {
                        case "addition":
                            state = "usual";
                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                            await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat,
                                text: $"Great, result is: <em>{first_num} + {second_num} = {first_num + second_num}</em>",
                                parseMode: ParseMode.Html,
                                replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "subtraction":
                            state = "usual";
                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                            await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat,
                                text: $"Great, result is: <em>{first_num} - {second_num} = {first_num - second_num}</em>",
                                parseMode: ParseMode.Html,
                                replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "multiplication":
                            state = "usual";
                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                            await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat,
                                text: $"Great, result is: <em>{first_num} * {second_num} = {first_num * second_num}</em>",
                                parseMode: ParseMode.Html,
                                replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "division":
                            if (second_num == 0)
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: update.Message.Chat,
                                    text: "Seems like a 'division by zero' attempt. Type the a number that is different from 0.");
                            }
                            else
                            {
                                state = "usual";
                                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                await botClient.SendTextMessageAsync(
                                    chatId: update.Message.Chat,
                                    text: $"Great, result is: <em>{first_num} / {second_num} = {first_num / second_num}</em>",
                                    parseMode: ParseMode.Html,
                                    replyMarkup: new ReplyKeyboardRemove());
                            }
                            break;
                        case "modulo":
                            if (second_num == 0)
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: update.Message.Chat,
                                    text: "Seems like a 'division by zero' attempt. Type the a number that is different from 0.");
                            }
                            else
                            {
                                state = "usual";
                                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                await botClient.SendTextMessageAsync(
                                    chatId: update.Message.Chat,
                                    text: $"Great, result is: <em>{first_num} % {second_num} = {first_num % second_num}</em>",
                                    parseMode: ParseMode.Html,
                                    replyMarkup: new ReplyKeyboardRemove());
                            }
                            break;
                    }
                }
                catch (FormatException)
                {
                    if (update.Message.Text.ToLower() == "cancel")
                    {
                        state = "usual";
                        await CancelAction(botClient, update, state);
                        break;
                    }
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "You didn't type a number!");
                    return;
                }
                break;
            case "sending_file_for_upload":
                state = "usual";

                if (update.Type != UpdateType.Message)
                {
                    return;
                }

                if (update.Message.Text != null)
                {
                    if (update.Message.Text.ToLower() == "cancel")
                    {
                        await CancelAction(botClient, update, state);
                        return;
                    }

                    if (update.Message.Type == MessageType.Text)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat,
                            text: "Send a file, not a text message!");
                        return;
                    }
                }

                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                await DownloadFile(botClient, update);
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: "Your file was successfully uploaded!",
                    replyMarkup: new ReplyKeyboardRemove());
                break;
            case "choosing_file_for_download":
                state = "usual";

                if (update.Type != UpdateType.Message)
                {
                    return;
                }

                try
                {
                    if (update.Message.Text != null)
                    {
                        if (update.Message.Text.ToLower() == "cancel")
                        {
                            await CancelAction(botClient, update, state);
                            return;
                        }

                        string path = $"..//net6.0//VariousTrash//{update.Message.Chat.Id}";
                        Dictionary<int, string> filesList = new Dictionary<int, string>();
                        string[] files = Directory.GetFiles(path);

                        for (int i = 0; i < files.Length; i++)
                        {
                            filesList.Add(i + 1, files[i]);
                        }

                        int seqID = Convert.ToInt32(update.Message.Text);

                        if (seqID < 1 || seqID > filesList.Count) 
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat,
                                text: "You typed an invalid sequence number!");
                        }

                        string value = "", fileName = filesList.TryGetValue(seqID, out value) ? fileName = value : "";

                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                        await using (Stream readStream = System.IO.File.OpenRead(fileName))
                        {
                            await botClient.SendDocumentAsync(
                                chatId: update.Message.Chat,
                                document: new InputOnlineFile(readStream, fileName.Substring(fileName.IndexOf("\\") + 1)),
                                caption: "Here's your file, grab it!",
                                replyMarkup: new ReplyKeyboardRemove());
                        }
                    }
                } catch
                {
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "You didn't type a sequence number!");
                    return;
                }
                break;
            case "choosing_file_for_delete":
                try
                {
                    state = "usual";
                    if (update.Type != UpdateType.Message)
                    {
                        return;
                    }

                    if (update.Message.Text != null)
                    {
                        if (update.Message.Text.ToLower() == "cancel")
                        {
                            await CancelAction(botClient, update, state);
                            return;
                        }

                        string path = $"..//net6.0//VariousTrash//{update.Message.Chat.Id}";
                        Dictionary<int, string> filesList = new Dictionary<int, string>();
                        string[] files = Directory.GetFiles(path);

                        for (int i = 0; i < files.Length; i++)
                        {
                            filesList.Add(i + 1, files[i]);
                        }

                        int seqID = Convert.ToInt32(update.Message.Text);

                        if (seqID < 1 || seqID > filesList.Count)
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat,
                                text: "You typed an invalid sequence number!");
                        }

                        string value = "", fileName = filesList.TryGetValue(seqID, out value) ? fileName = value : "";

                        System.IO.File.Delete(fileName);

                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: "File was successfully deleted!",
                            replyMarkup: new ReplyKeyboardRemove());
                    }
                }
                catch
                {
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "You didn't type a sequence number!");
                    return;
                }
                break;
            case "waiting_for_userID":
                state = "waiting_a_message";

                try
                {
                    if (update.Type != UpdateType.Message)
                    {
                        return;
                    }

                    int id = Convert.ToInt32(update.Message.Text);
                    string companion_chat_id = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{id}'");

                    if (companion_chat_id == update.Message.Chat.Id.ToString() || companion_chat_id == null)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat,
                            text: "You typed a wrong ID! Type the right one, please.");
                        break;
                    }

                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                    SQLStuff.UpdateDB($"UPDATE users_list SET send_message_to='{id}' WHERE chatID='{update.Message.Chat.Id}'");

                    string username = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE id='{id}'");

                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: $"OK, you chose this user: <em>{username}</em>.\nNow send me the message that you want to send to the user " +
                        $"(you're able to send: audios, videos, photos, stickers, documents, text, voice notes and voice messages):",
                        parseMode: ParseMode.Html);
                }
                catch (FormatException)
                {
                    if (update.Message.Text.ToLower() == "cancel")
                    {
                        state = "usual";
                        await CancelAction(botClient, update, state);
                        break;
                    }
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "You didn't type a number!");
                    return;
                }
                break;
            case "waiting_a_message":

                state = "usual";

                if (update.Type != UpdateType.Message)
                {
                    return;
                }

                if (update.Message.Type == MessageType.Text && update.Message.Text.ToLower() == "cancel")
                {
                    await CancelAction(botClient, update, state);
                    break;
                }

                string where_send_to = SQLStuff.ReadDBRecords($"SELECT send_message_to FROM users_list WHERE chatID='{update.Message.Chat.Id}'");
                string chat_id = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{where_send_to}'");

                SQLStuff.UpdateDB($"UPDATE users_list SET send_message_to=NULL WHERE chatID='{update.Message.Chat.Id}'");
                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                await SendSpecificMessage(botClient, update, chat_id, state);

                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: $"Message was successfully delivered!",
                    replyMarkup: new ReplyKeyboardRemove());
                break;
            case "choosing_companion":
                state = "waiting_for_response";
                try
                {
                    int companion = Convert.ToInt32(update.Message.Text);
                    string id = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{update.Message.Chat.Id}'");
                    string companion_chat_id = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{companion}'");
                    string stateChecking = SQLStuff.ReadDBRecords($"SELECT state FROM users_list WHERE id='{companion}'");

                    if (companion_chat_id == update.Message.Chat.Id.ToString() || companion_chat_id == null)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat,
                            text: "You typed a wrong ID! Type the right one, please.");
                        break;
                    }

                    if (stateChecking == "In_Conversation" || stateChecking == "choosing_yes_or_no" || stateChecking == "waiting_for_response")
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat,
                            text: "This user currently isn't available for conversation.");
                        break;
                    }

                    SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with='{companion}' WHERE chatID='{update.Message.Chat.Id}'");
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                    state = "choosing_yes_or_no";
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{companion}'");

                    string username = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE id='{companion}'");
                    string offererUName = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE chatID='{update.Message.Chat.Id}'");

                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
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
                    if (update.Message.Text.ToLower() == "cancel")
                    {
                        state = "usual";
                        await CancelAction(botClient, update, state);
                        break;
                    }
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "You didn't type a number!");
                    return;
                }
                break;
            case "choosing_yes_or_no":
                if (update.Type != UpdateType.Message)
                {
                    return;
                }

                switch (update.Message.Text.ToLower())
                {
                    case "yes":
                        state = "InConversation";

                        string oID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{update.Message.Chat.Id}'");
                        string cID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE messaging_with='{oID}'");

                        string cChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{cID}'");
                        string uName = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE chatID='{update.Message.Chat.Id}'");

                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                        SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with='{cID}' WHERE chatID='{update.Message.Chat.Id}'");
                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{cID}'");

                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat,
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

                        string offererID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{update.Message.Chat.Id}'");
                        string companionID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE messaging_with='{offererID}'");

                        string companionChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{companionID}'");
                        string user_name = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE chatID='{update.Message.Chat.Id}'");

                        SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE chatID='{companionID}'");
                        SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE chatID='{update.Message.Chat.Id}'");
                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{companionID}'");

                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat,
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
                            chatId: update.Message.Chat,
                            text: "Type only <b>'Yes'</b> or <b>'No'</b>!",
                            parseMode: ParseMode.Html);
                        break;
                }
                break;
            case "waiting_for_response":
                if (update.Type != UpdateType.Message)
                {
                    return;
                }

                if (update.Message.Text.ToLower() == "cancel")
                {
                    state = "usual";
                    string of_ID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{update.Message.Chat.Id}'");
                    string co_ID = SQLStuff.ReadDBRecords($"SELECT messaging_with FROM users_list WHERE id='{of_ID}'");
                    string co_ChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{co_ID}'");

                    SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE id='{co_ID}'");
                    SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE chatID='{update.Message.Chat.Id}'");
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{co_ID}'");

                    await botClient.SendTextMessageAsync(
                        chatId: co_ChatID,
                        text: "An offer to start a conversation was cancelled",
                        replyMarkup: new ReplyKeyboardRemove());
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "You cancelled your offer to start a conversation!",
                        replyMarkup: new ReplyKeyboardRemove());
                }
                break;
            case "InConversation":
                string offerer_ID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{update.Message.Chat.Id}'");
                string companion_ID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE messaging_with='{offerer_ID}'");
                string companion_ChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{companion_ID}'");
                string user_Name = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE id='{offerer_ID}'");

                switch (update.Type)
                {
                    case UpdateType.Message:
                        switch (update.Message.Type)
                        {
                            case MessageType.Text:
                                if (update.Message != null)
                                {
                                    if (update.Message.Text.ToLower() == "stop")
                                    {
                                        state = "usual";

                                        SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE id='{companion_ID}'");
                                        SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE chatID='{update.Message.Chat.Id}'");
                                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{companion_ID}'");

                                        await botClient.SendTextMessageAsync(
                                            chatId: companion_ChatID,
                                            text: $"<b>{user_Name}</b> has left from the conversation. Have a pleasant day!",
                                            parseMode: ParseMode.Html,
                                            replyMarkup: new ReplyKeyboardRemove());
                                        await botClient.SendTextMessageAsync(
                                            chatId: update.Message.Chat,
                                            text: "You left from the conversation!",
                                            replyMarkup: new ReplyKeyboardRemove());
                                        break;
                                    }
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
                break;
            default:
                break;
        }
    }

    static async Task ProcessingMessages(ITelegramBotClient botClient, Update update, string state)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                switch (update.Message.Type)
                {
                    case MessageType.Text:
                        switch (update.Message.Text.ToLower())
                        {
                            case "/start":
                                await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat,
                                    text: $"Hello there, my friend {update.Message.Chat.FirstName} \U0001F44B. I'm a C# Telegram Bot. What can I do for ya, bruh?");
                                break;
                            case "/anonmessage":
                                Dictionary<string, string> users = SQLStuff.TakeUsersList($"SELECT id, username FROM users_list WHERE chatID != '{update.Message.Chat.Id}'");

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
                                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                                    await botClient.SendTextMessageAsync(
                                        chatId: update.Message.Chat,
                                        text: message_text,
                                        parseMode: ParseMode.Html,
                                        replyMarkup: CancelKeyboardButton());
                                }
                                else
                                {
                                    state = "usual";
                                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                                    await botClient.SendTextMessageAsync(
                                        chatId: update.Message.Chat,
                                        text: "Unfortunately, there are no users to whom you can send a message \U0001F626",
                                        replyMarkup: new ReplyKeyboardRemove());
                                }
                                break;
                            case "/calc":
                                state = "choosing_option";
                                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                                await botClient.SendTextMessageAsync(
                                    chatId: update.Message.Chat,
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
                                            update.Message.Chat,
                                            sticker: "CAACAgIAAxkBAAIEz2OzJXjc3RnGKrFOE_5BPu0gz4-8AALcxgEAAWOLRgyxtRIUSi4a_y0E");
                                        break;
                                    case 2:
                                        await botClient.SendStickerAsync(
                                            update.Message.Chat,
                                            sticker: "CAACAgIAAxkBAAIE0GOzJYBAnry97DeRm1jbw3i8HEOeAALdxgEAAWOLRgzrTyk77CMCUS0E");
                                        break;
                                    case 3:
                                        await botClient.SendStickerAsync(
                                            update.Message.Chat,
                                            sticker: "CAACAgIAAxkBAAIE0WOzJZQ4uxEbX6WyeZM0ih2PFv48AALexgEAAWOLRgxUcf2Fq_sguS0E");
                                        break;
                                    case 4:
                                        await botClient.SendStickerAsync(
                                            update.Message.Chat,
                                            sticker: "CAACAgIAAxkBAAIE0mOzJa7-UnwOy3ZITOqpJYaOP33cAALfxgEAAWOLRgwcRRMg1btjFy0E");
                                        break;
                                    case 5:
                                        await botClient.SendStickerAsync(
                                            update.Message.Chat,
                                            sticker: "CAACAgIAAxkBAAIE02OzJb8Fl8TdvPYb7S1sRB46LLbCAALgxgEAAWOLRgxIsfP6yP8mqS0E");
                                        break;
                                    case 6:
                                        await botClient.SendStickerAsync(
                                            update.Message.Chat,
                                            sticker: "CAACAgIAAxkBAAIE1GOzJeZ3wfH-rxYyF2ZKl6JdlWiuAALhxgEAAWOLRgzvmnzNp7-0ei0E");
                                        break;
                                }

                                await botClient.SendTextMessageAsync(
                                    chatId: update.Message.Chat,
                                    text: $"Number rolled: {randNum}");
                                break;
                            case "/files":
                                await botClient.SendTextMessageAsync(
                                    chatId: update.Message.Chat,
                                    text: "Choose one of the options below that you want to use with your files \U00002B07",
                                    replyMarkup: OptionsWithFilesInlineKeyboard());
                                break;
                            case "/links":
                                await botClient.SendTextMessageAsync(
                                    chatId: update.Message.Chat,
                                    text: "Here are my developer's links \U00002B07",
                                    replyMarkup: LinksInlineKeyboard());
                                break;
                            case "/pic":
                                await botClient.SendPhotoAsync(
                                    chatId: update.Message.Chat,
                                    photo: "https://media.discordapp.net/attachments/748112995606986803/1059547380132876328/durka.png",
                                    caption: "<b>Durka, ebat'</b>",
                                    parseMode: ParseMode.Html);
                                break;
                            case "/startconv":
                                users = SQLStuff.TakeUsersList($"SELECT id, username FROM users_list WHERE chatID != '{update.Message.Chat.Id}'");

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
                                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                                    await botClient.SendTextMessageAsync(
                                        chatId: update.Message.Chat,
                                        text: message_text,
                                        parseMode: ParseMode.Html,
                                        replyMarkup: CancelKeyboardButton());
                                }
                                else
                                {
                                    state = "usual";
                                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                                    await botClient.SendTextMessageAsync(
                                        chatId: update.Message.Chat,
                                        text: "Unfortunately, there are no users with whom you can start a conversation \U0001F626.",
                                        replyMarkup: new ReplyKeyboardRemove());
                                }
                                break;
                            case "witam":
                            case "hello":
                            case "hi":
                                await botClient.SendTextMessageAsync(
                                   chatId: update.Message.Chat,
                                   text: "Pritam, dude \U0001F44B.",
                                   replyToMessageId: update.Message.MessageId);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case UpdateType.CallbackQuery:
                var pressedButtonData = update.CallbackQuery.Data;
                switch (pressedButtonData)
                {
                    case "BackToFilesSettings":
                        await BackToSettings(botClient, update);
                        break;
                    case "SeeFilesList":
                        string result = GetFilesList(update);
                        if (result == "NoFiles")
                        {
                            await botClient.EditMessageTextAsync(
                                chatId: update.CallbackQuery.Message.Chat,
                                messageId: update.CallbackQuery.Message.MessageId,
                                text: "Unfortunately, your list of files is empty \U0001F626.",
                                replyMarkup: BackToSettingsInlineKeyboard(),
                                parseMode: ParseMode.Html);
                            return;
                        }

                        await botClient.EditMessageTextAsync(
                            chatId: update.CallbackQuery.Message.Chat,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: result,
                            replyMarkup: BackToSettingsInlineKeyboard(),
                            parseMode: ParseMode.Html);
                        break;
                    case "UploadFile":
                        state = "sending_file_for_upload";

                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.CallbackQuery.Message.Chat.Id}'");

                        await botClient.SendTextMessageAsync(
                            chatId: update.CallbackQuery.Message.Chat,
                            text: "Send me the file you want to upload to the server, please.",
                            replyMarkup: CancelKeyboardButton());
                        break;
                    case "DownloadFile":
                        result = GetFilesList(update);
                        state = "choosing_file_for_download";
                        if (result == "NoFiles")
                        {
                            await botClient.EditMessageTextAsync(
                                chatId: update.CallbackQuery.Message.Chat,
                                messageId: update.CallbackQuery.Message.MessageId,
                                text: "Unfortunately, your list of files is empty \U0001F626.",
                                replyMarkup: BackToSettingsInlineKeyboard(),
                                parseMode: ParseMode.Html);
                            return;
                        }

                        await botClient.EditMessageTextAsync(
                            chatId: update.CallbackQuery.Message.Chat,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: result,
                            replyMarkup: BackToSettingsInlineKeyboard(),
                            parseMode: ParseMode.Html);

                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.CallbackQuery.Message.Chat.Id}'");

                        await botClient.SendTextMessageAsync(
                            chatId: update.CallbackQuery.Message.Chat,
                            text: "Enter the sequence number of the file you want to download, please.",
                            replyMarkup: CancelKeyboardButton());
                        break;
                    case "DeleteFile":
                        state = "choosing_file_for_delete";
                        result = GetFilesList(update);
                        if (result == "NoFiles")
                        {
                            await botClient.EditMessageTextAsync(
                                chatId: update.CallbackQuery.Message.Chat,
                                messageId: update.CallbackQuery.Message.MessageId,
                                text: "Unfortunately, your list of files is empty \U0001F626.",
                                replyMarkup: BackToSettingsInlineKeyboard(),
                                parseMode: ParseMode.Html);
                            return;
                        }

                        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.CallbackQuery.Message.Chat.Id}'");

                        await botClient.EditMessageTextAsync(
                            chatId: update.CallbackQuery.Message.Chat,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: result,
                            replyMarkup: BackToSettingsInlineKeyboard(),
                            parseMode: ParseMode.Html);
                        await botClient.SendTextMessageAsync(
                            chatId: update.CallbackQuery.Message.Chat,
                            text: "Enter the sequence number of the file you want to delete, please.",
                            replyMarkup: CancelKeyboardButton());
                        break;
                    default:
                        break;
                }
                break;
        }
    }

    static async Task BackToSettings(ITelegramBotClient botClient, Update update)
    {
        await botClient.EditMessageTextAsync(
            chatId: update.CallbackQuery.Message.Chat,
            messageId: update.CallbackQuery.Message.MessageId,
            text: "Choose one of the options below that you want to use with your files \U00002B07",
            replyMarkup: OptionsWithFilesInlineKeyboard());
    }

    static string GetFilesList(Update update)
    {
        Dictionary<int, string> filesList = new Dictionary<int, string>();
        string path = $"..//net6.0//VariousTrash//{update.CallbackQuery.Message.Chat.Id}";
        string[] files = Directory.GetFiles(path);
        string messageText = "Here are a list of your files:\n\n";

        if (files.Length != 0)
        {
            for (int i = 0; i < files.Length; i++)
            {
                filesList.Add(i + 1, files[i]);
                messageText += $"<b>{i + 1}. {files[i].Substring(files[i].IndexOf("\\") + 1)}</b>\n";
            }
            return messageText;
        }

        return "NoFiles";
    }

    static async Task DownloadFile(ITelegramBotClient botClient, Update update)
    {
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
        Logger.WriteLog(update);
        switch (update.Type)
        {
            case UpdateType.Message:
                var message = update.Message;

                if (message != null)
                {
                    if (!SQLStuff.doesUserExist(message.Chat.Id.ToString()))
                    {
                        string[] values = { message.Chat.FirstName, message.Chat.LastName, $"({message.Chat.Username})" };
                        string username = string.Join(" ", values);
                        await SQLStuff.AddUser(message.Chat.Id.ToString(), username);
                    }

                    string state = SQLStuff.ReadDBRecords($"SELECT state FROM users_list WHERE chatID='{message.Chat.Id}'");
                    Directory.CreateDirectory($"..//net6.0//VariousTrash//{message.Chat.Id}");

                    await ProcessingStates(state, botClient, update);
                    await ProcessingMessages(botClient, update, state);
                }
                break;
            case UpdateType.CallbackQuery:
                string userState = SQLStuff.ReadDBRecords($"SELECT state FROM users_list WHERE chatID='{update.CallbackQuery.Message.Chat.Id}'");
                await ProcessingStates(userState, botClient, update);
                await ProcessingMessages(botClient, update, userState);
                break;
            default:
                break;
        }
    }
    #endregion
}

