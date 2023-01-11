namespace TelegramBot;

internal partial class HelperMethodsAndFuncs
{
    // for /calc command
    static double first_num = 0, second_num = 0;
    static string math_oper = "";

    // Users' files, temp files' and pictures pathes
    static string tempPath = $"..//net6.0//AnonMessages//";
    static string usersFilesPath = $"..//net6.0//VariousTrash//";
    static string picturesPath = $"..//net6.0//VariousTrash//Pictures//";

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
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Delete all files", callbackData: "DeleteAllFiles")
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
    // Send specific messages to user (for /anonmessage and /startconv commands)
    static async Task SendSpecificMessage(ITelegramBotClient botClient, Update update, string chat_id, string state, string path = "")
    {
        switch (update.Message.Type)
        {
            case MessageType.Text:
                await botClient.SendTextMessageAsync(
                    chatId: chat_id,
                    text: state == "InConversation" ? update.Message.Text : $"A message for you from anonymous user: <b>{update.Message.Text}</b>",
                    parseMode: ParseMode.Html,
                    replyMarkup: state != "InConversation" ? null : StopKeyboard());
                break;
            case MessageType.Document:
                var fileId = update.Message.Document.FileId;

                await DownloadFile(botClient, update, state, path);

                await botClient.SendDocumentAsync(
                    chatId: chat_id,
                    document: fileId,
                    caption: state == "InConversation" ? update.Message.Text : $"A document for you from anonymous user.\n\n{(update.Message.Caption == null ? "A caption is empty" : update.Message.Caption)}",
                    replyMarkup: state != "InConversation" ? null : StopKeyboard());
                break;
            case MessageType.Photo:
                fileId = update.Message.Photo.Last().FileId;

                await DownloadFile(botClient, update, state, path);

                await botClient.SendPhotoAsync(
                    chatId: chat_id,
                    photo: fileId,
                    caption: state == "InConversation" ? update.Message.Text : $"A picture (or photo) for you from anonymous user.\n\n{(update.Message.Caption == null ? "A caption is empty" : update.Message.Caption)}",
                    replyMarkup: state != "InConversation" ? null : StopKeyboard());
                break;
            case MessageType.Audio:
                fileId = update.Message.Audio.FileId;

                await DownloadFile(botClient, update, state, path);

                await botClient.SendAudioAsync(
                    chatId: chat_id,
                    audio: fileId,
                    caption: state == "InConversation" ? update.Message.Text : $"An audio for you from anonymous user.\n\n{(update.Message.Caption == null ? "A caption is empty" : update.Message.Caption)}",
                    replyMarkup: state != "InConversation" ? null : StopKeyboard());
                break;
            case MessageType.Voice:
                fileId = update.Message.Voice.FileId;

                await DownloadFile(botClient, update, state, path);

                await botClient.SendVoiceAsync(
                    chatId: chat_id,
                    voice: fileId,
                    caption: state == "InConversation" ? update.Message.Text : $"A voice message for you from anonymous user.\n\n{(update.Message.Caption == null ? "A caption is empty" : update.Message.Caption)}",
                    replyMarkup: state != "InConversation" ? null : StopKeyboard());

                break;
            case MessageType.Sticker:
                fileId = update.Message.Sticker.FileId;

                await DownloadFile(botClient, update, state, path);

                await botClient.SendStickerAsync(
                    chatId: chat_id,
                    sticker: fileId,
                    replyMarkup: state != "InConversation" ? null : StopKeyboard());
                break;
            case MessageType.Video:
                fileId = update.Message.Video.FileId;

                await DownloadFile(botClient, update, state, path);

                await botClient.SendVideoAsync(
                    chatId: chat_id,
                    video: fileId,
                    caption: state == "InConversation" ? update.Message.Text : $"A video for you from anonymous user.\n\n{(update.Message.Caption == null ? "A caption is empty" : update.Message.Caption)}",
                    replyMarkup: state != "InConversation" ? null : StopKeyboard());
                break;
            case MessageType.VideoNote:
                fileId = update.Message.VideoNote.FileId;

                await DownloadFile(botClient, update, state, path);

                await botClient.SendVideoNoteAsync(
                    chatId: chat_id,
                    videoNote: fileId,
                    duration: update.Message.VideoNote.Duration,
                    replyMarkup: state != "InConversation" ? null : StopKeyboard());
                break;
            case MessageType.Location:
                double[] location = new double[2];
                location[0] = update.Message.Location.Latitude;
                location[1] = update.Message.Location.Longitude;

                await botClient.SendLocationAsync(
                    chatId: chat_id,
                    latitude: location[0],
                    longitude: location[1]);
                break;
            default:
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: "This type of message isn't supported. Try the supported one next time, please.",
                    replyMarkup: state == "InConversation" ? null : new ReplyKeyboardRemove());
                break;
        }
    }
    #endregion

    #region Methods and functions related with files
    // Checking if user's files list is empty or not
    static async Task CheckFilesList(ITelegramBotClient botClient, Update update, string state)
    {
        Dictionary<int, string> filesList = new Dictionary<int, string>();
        string path = Path.Combine(usersFilesPath, update.CallbackQuery.Message.Chat.Id.ToString());
        string[] files = Directory.GetFiles(path);
        string messageText = "Here are a list of your files:\n\n";

        if (files.Length == 0)
        {
            await botClient.EditMessageTextAsync(
                chatId: update.CallbackQuery.Message.Chat,
                messageId: update.CallbackQuery.Message.MessageId,
                text: update.CallbackQuery.Data == "DeleteFile" || update.CallbackQuery.Data == "DeleteAllFiles" ? "There is nothing to delete." : "Unfortunately, your list of files is empty \U0001F641.",
                replyMarkup: BackToSettingsInlineKeyboard(),
                parseMode: ParseMode.Html);
            return;
        }

        for (int i = 0; i < files.Length; i++)
        {
            filesList.Add(i + 1, files[i]);
            messageText += $"<b>{i + 1}. {files[i].Substring(files[i].IndexOf("\\") + 1)}</b>\n";
        }

        await botClient.EditMessageTextAsync(
            chatId: update.CallbackQuery.Message.Chat,
            messageId: update.CallbackQuery.Message.MessageId,
            text: messageText,
            replyMarkup: BackToSettingsInlineKeyboard(),
            parseMode: ParseMode.Html);

        if (update.CallbackQuery.Data == "SeeFilesList")
        {
            return;
        }

        string message = "";

        switch (update.CallbackQuery.Data)
        {
            case "DownloadFile":
                state = "choosing_file_for_download";
                message = "Enter the sequence number of the file you want to download, please.";
                break;
            case "DeleteFile":
                state = "choosing_file_for_delete";
                message = "Enter the sequence number of the file you want to delete, please.";
                break;
            case "DeleteAllFiles":
                state = "deciding_to_delete_or_not";
                message = "Are you sure about your decision to delete all files?";
                break;
        }

        SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.CallbackQuery.Message.Chat.Id}'");
        await botClient.SendTextMessageAsync(
            chatId: update.CallbackQuery.Message.Chat,
            text: message,
            replyMarkup: update.CallbackQuery.Data == "DeleteAllFiles" ? YesNoKeyboard() : CancelKeyboardButton());
    }

    // Fetching list of user's files
    static Dictionary<int, string> GetFilesList(string path)
    {
        Dictionary<int, string> filesList = new Dictionary<int, string>();
        string[] files = Directory.GetFiles(path);

        if (files.Length != 0)
        {
            for (int i = 0; i < files.Length; i++)
            {
                filesList.Add(i + 1, files[i]);
            }
            return filesList;
        }

        return null;
    }

    // In order to user can upload files to his own storage
    static async Task DownloadFile(ITelegramBotClient botClient, Update update, string state = "", string fileId = "", string path = "")
    {
        Telegram.Bot.Types.File fileInfo;
        string filePath;
        string partofPath;

        switch (update.Message.Type)
        {
            case MessageType.Document:
                fileId = update.Message.Document.FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;
                partofPath = update.Message.Document.FileName;
                break;
            case MessageType.Photo:
                fileId = update.Message.Photo.Last().FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;
                partofPath = filePath.Substring(filePath.IndexOf("/") + 1);
                break;
            case MessageType.Video:
                fileId = update.Message.Video.FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;
                partofPath = update.Message.Video.FileName;
                break;
            case MessageType.Audio:
                fileId = update.Message.Audio.FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;
                partofPath = update.Message.Audio.FileName;
                break;
            case MessageType.Voice:
                fileId = update.Message.Voice.FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;
                partofPath = filePath.Substring(filePath.IndexOf("/") + 1);
                break;
            case MessageType.Sticker:
                fileId = update.Message.Sticker.FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;
                partofPath = filePath.Substring(filePath.IndexOf("/") + 1);
                break;
            case MessageType.VideoNote:
                fileId = update.Message.VideoNote.FileId;
                fileInfo = await botClient.GetFileAsync(fileId);
                filePath = fileInfo.FilePath;
                partofPath = filePath.Substring(filePath.IndexOf("/") + 1);
                break;
            default:
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "Unknown type of message.");
                return;
        }

        DirectoryInfo dir = path == "" ? Directory.CreateDirectory(Path.Combine(usersFilesPath, update.Message.Chat.Id.ToString())) : Directory.CreateDirectory(path);

        string destinationFilePath = $"{dir}//{partofPath}";

        await using (Stream fileStream = System.IO.File.OpenWrite(destinationFilePath))
        {
            await botClient.DownloadFileAsync(
                filePath: filePath,
                destination: fileStream);

            if (destinationFilePath.Contains("AnonMessages") || state == "InConversation")
            {
                System.IO.File.Delete(destinationFilePath);
            }
        }
    }

    // Processing uploading and downloading files
    static async Task ForDownloadingAndDeletingFiles(ITelegramBotClient botClient, Update update, string state, string path)
    {
        Dictionary<int, string> filesList = GetFilesList(path);

        string fileName = "";

        if (state != "deciding_to_delete_or_not")
        {
            int seqID = Convert.ToInt32(update.Message.Text);

            if (seqID < 1 || seqID > filesList.Count)
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: "You typed an invalid sequence number!");
                return;
            }

            string value;
            fileName = filesList.TryGetValue(seqID, out value) ? value : "";
        }

        switch (state)
        {
            case "choosing_file_for_download":
                if (IgnoreNonMessageUpdates(update) == true)
                {
                    return;
                }

                state = "usual";
                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                await using (Stream readStream = System.IO.File.OpenRead(fileName))
                {
                    await botClient.SendDocumentAsync(
                        chatId: update.Message.Chat,
                        document: new InputOnlineFile(readStream, fileName.Substring(fileName.IndexOf("\\") + 1)),
                        caption: "Here's your file, grab it!",
                        replyMarkup: new ReplyKeyboardRemove());
                }
                break;
            case "choosing_file_for_delete":
                if (IgnoreNonMessageUpdates(update) == true)
                {
                    return;
                }

                state = "usual";
                System.IO.File.Delete(fileName);

                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: "File was successfully deleted!",
                    replyMarkup: new ReplyKeyboardRemove());
                break;
            case "deciding_to_delete_or_not":
                if (IgnoreNonMessageUpdates(update) == true)
                {
                    return;
                }

                state = "usual";
                if (update.Message.Text.ToLower() != "yes" && update.Message.Text.ToLower() != "no")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "Choose or type only 'yes' or 'no'!");
                }

                if (update.Message.Text.ToLower() == "yes")
                {
                    foreach (var item in filesList)
                    {
                        System.IO.File.Delete(item.Value);
                    }
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "All files were successfully deleted!",
                        replyMarkup: new ReplyKeyboardRemove());
                }

                if (update.Message.Text.ToLower() == "no")
                {
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "Fair enough, I even think that this is a reasonable decision \U0001F44D.",
                        replyMarkup: new ReplyKeyboardRemove());
                }
                break;
            default:
                break;
        }
    }
    #endregion

    #region Processing state machine and messages from the user

    static bool IgnoreNonMessageUpdates(Update update)
    {
        if (update.Type != UpdateType.Message)
        {
            return true;
        }
        return false;
    }

    static async Task BackToSettings(ITelegramBotClient botClient, Update update)
    {
        await botClient.EditMessageTextAsync(
            chatId: update.CallbackQuery.Message.Chat,
            messageId: update.CallbackQuery.Message.MessageId,
            text: "Choose one of the options below that you want to use with your files \U00002B07",
            replyMarkup: OptionsWithFilesInlineKeyboard());
    }

    // Processing users' conversation process
    static async Task ForConversations(ITelegramBotClient botClient, Update update, string state)
    {
        // for "yes", "no", "stop" and partially "cancel" options
        string offererID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{update.Message.Chat.Id}'");
        string companionID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE messaging_with='{offererID}'");
        string companionChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{companionID}'");
        string username = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE chatID='{update.Message.Chat.Id}'");

        // taking companionID and companionChatID for "cancel" option
        string co_ID = SQLStuff.ReadDBRecords($"SELECT messaging_with FROM users_list WHERE id='{offererID}'");
        string co_ChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{co_ID}'");

        // taking username for "stop" option
        string userName = SQLStuff.ReadDBRecords($"SELECT username FROM users_list WHERE id='{offererID}'");

        switch (state)
        {
            case "waiting_for_response":
                if (update.Message.Text.ToLower() == "cancel")
                {
                    state = "usual";

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
            case "choosing_yes_or_no":
                if (update.Message.Text.ToLower() == "yes")
                {
                    state = "InConversation";
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                    SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with='{companionID}' WHERE chatID='{update.Message.Chat.Id}'");
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{companionID}'");

                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "You accepted an offer!\n\nIn order to stop a conversation just type <b>'stop'</b> (in any case).",
                        parseMode: ParseMode.Html,
                        replyMarkup: StopKeyboard());
                    await botClient.SendTextMessageAsync(
                        chatId: companionChatID,
                        text: $"<b>{username}</b> accepted an offer to start a conversation!\n\nIn order to stop a conversation just type <b>'stop'</b> (in any case).",
                        parseMode: ParseMode.Html,
                        replyMarkup: StopKeyboard());
                }

                if (update.Message.Text.ToLower() == "no")
                {
                    state = "usual";

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
                        text: $"<b>{username}</b> has declined your offer to start a conversation. Have a pleasant day!",
                        parseMode: ParseMode.Html,
                        replyMarkup: new ReplyKeyboardRemove());
                }
                break;
            case "InConversation":
                if (update.Message.Text.ToLower() == "stop")
                {
                    state = "usual";

                    SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE id='{companionID}'");
                    SQLStuff.UpdateDB($"UPDATE users_list SET messaging_with=NULL WHERE chatID='{update.Message.Chat.Id}'");
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                    SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE id='{companionID}'");

                    await botClient.SendTextMessageAsync(
                        chatId: companionChatID,
                        text: $"<b>{userName}</b> has left from the conversation. Have a pleasant day!",
                        parseMode: ParseMode.Html,
                        replyMarkup: new ReplyKeyboardRemove());
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "You left from the conversation!",
                        replyMarkup: new ReplyKeyboardRemove());
                }
                break;
            default:
                break;
        }
    }

    // Check users list in order to send a anonymous message to other user or start a conversation with one of them
    static async Task CheckUsersList(ITelegramBotClient botClient, Update update, string state)
    {
        Dictionary<string, string> users = SQLStuff.TakeUsersList($"SELECT id, username FROM users_list WHERE chatID != '{update.Message.Chat.Id}'");

        if (users.Count != 0)
        {
            string resultset;
            string message_text = update.Message.Text == "/anonmessage" ? "List of users to whom you can send a message: \n\n"
                                                                        : "List of users to with whom you can start a conversation: \n\n";
            foreach (var item in users)
            {
                resultset = $"<b>(ID: {item.Key}) - '{item.Value}'</b>\n";
                message_text = message_text + resultset;
            }

            message_text = update.Message.Text == "/anonmessage" ? $"{message_text}\nEnter the ID of the user you want to send the message to:"
                                                                 : $"{message_text}\nEnter the ID of the user with whom you want to start a conversation:";

            state = update.Message.Text == "/anonmessage" ? "waiting_for_userID" : "choosing_companion";
            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat,
                text: message_text,
                parseMode: ParseMode.Html,
                replyMarkup: CancelKeyboardButton());
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat,
                text: update.Message.Text == "/anonmessage" ? "Unfortunately, there are no users to whom you can send a message \U0001F641"
                                                            : "Unfortunately, there are no users with whom you can start a conversation \U0001F641.",
                replyMarkup: new ReplyKeyboardRemove());
            return;
        }
    }

    public static async Task ProcessingStates(string state, ITelegramBotClient botClient, Update update)
    {
        if (IgnoreNonMessageUpdates(update) == true)
        {
            return;
        }

        switch (state)
        {
            case "choosing_option":
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
                    double result = Convert.ToDouble(update.Message.Text);
                    second_num = result;

                    switch (math_oper)
                    {
                        case "addition":
                            state = "usual";
                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                            await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat,
                                text: $"Great, the result is: <em>{first_num} + {second_num} = {first_num + second_num}</em>",
                                parseMode: ParseMode.Html,
                                replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "subtraction":
                            state = "usual";
                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                            await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat,
                                text: $"Great, the result is: <em>{first_num} - {second_num} = {first_num - second_num}</em>",
                                parseMode: ParseMode.Html,
                                replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "multiplication":
                            state = "usual";
                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");
                            await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat,
                                text: $"Great, the result is: <em>{first_num} * {second_num} = {first_num * second_num}</em>",
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
                                    text: $"Great, the result is: <em>{first_num} / {second_num} = {first_num / second_num}</em>",
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

                await DownloadFile(botClient, update, state);
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: "Your file was successfully uploaded!",
                    replyMarkup: new ReplyKeyboardRemove());
                break;
            case "choosing_file_for_download":
                try
                {
                    if (update.Message.Text != null)
                    {
                        if (update.Message.Text.ToLower() == "cancel")
                        {
                            state = "usual";
                            await CancelAction(botClient, update, state);
                            return;
                        }

                        await ForDownloadingAndDeletingFiles(botClient, update, state, Path.Combine(usersFilesPath, update.Message.Chat.Id.ToString()));
                    }
                }
                catch (FormatException)
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
                    if (update.Message.Text != null)
                    {
                        if (update.Message.Text.ToLower() == "cancel")
                        {
                            state = "usual";
                            await CancelAction(botClient, update, state);
                            return;
                        }
                        await ForDownloadingAndDeletingFiles(botClient, update, state, Path.Combine(usersFilesPath, update.Message.Chat.Id.ToString()));
                    }
                }
                catch (FormatException)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "You didn't type a sequence number!");
                    return;
                }
                break;
            case "deciding_to_delete_or_not":
                await ForDownloadingAndDeletingFiles(botClient, update, state, Path.Combine(usersFilesPath, update.Message.Chat.Id.ToString()));
                break;
            case "waiting_for_userID":
                state = "waiting_a_message";

                try
                {
                    int id = Convert.ToInt32(update.Message.Text);
                    string receiverChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{id}'");

                    if (receiverChatID == update.Message.Chat.Id.ToString() || receiverChatID == null)
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
                        $"(you're able to send: your location, audios, videos, photos, stickers, documents, text, voice notes and voice messages):",
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

                if (update.Message.Type == MessageType.Text && update.Message.Text.ToLower() == "cancel")
                {
                    await CancelAction(botClient, update, state);
                    break;
                }

                string where_send_to = SQLStuff.ReadDBRecords($"SELECT send_message_to FROM users_list WHERE chatID='{update.Message.Chat.Id}'");
                string chat_id = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{where_send_to}'");

                SQLStuff.UpdateDB($"UPDATE users_list SET send_message_to=NULL WHERE chatID='{update.Message.Chat.Id}'");
                SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.Message.Chat.Id}'");

                string path = Directory.CreateDirectory(Path.Combine(tempPath, update.Message.Chat.Id.ToString())).ToString();

                await SendSpecificMessage(botClient, update, chat_id, state, path);

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

                    if (stateChecking == "InConversation" || stateChecking == "choosing_yes_or_no" || stateChecking == "waiting_for_response")
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
                switch (update.Message.Text.ToLower())
                {
                    case "yes":
                        await ForConversations(botClient, update, state);
                        break;
                    case "no":
                        await ForConversations(botClient, update, state);
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
                if (update.Message.Text.ToLower() == "cancel")
                {
                    await ForConversations(botClient, update, state);
                }
                break;
            case "InConversation":
                string offerer_ID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE chatID='{update.Message.Chat.Id}'");
                string companion_ID = SQLStuff.ReadDBRecords($"SELECT id FROM users_list WHERE messaging_with='{offerer_ID}'");
                string companion_ChatID = SQLStuff.ReadDBRecords($"SELECT chatID FROM users_list WHERE id='{companion_ID}'");

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
                                        await ForConversations(botClient, update, state);
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

    public static async Task ProcessingMessages(ITelegramBotClient botClient, Update update, string state)
    {
        Random rnd = new Random();
        if (state == "usual")
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
                                        text: $"Hello there, my friend {update.Message.Chat.FirstName} \U0001F44B. Thanks for starting to use me. What can I do for ya?");
                                    break;
                                case "/anonmessage":
                                    await CheckUsersList(botClient, update, state);
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
                                    Directory.CreateDirectory(picturesPath).ToString();
                                    Dictionary<int, string> picturesList = GetFilesList(picturesPath);

                                    if (picturesList == null)
                                    {
                                        await botClient.SendTextMessageAsync(
                                            chatId: update.Message.Chat,
                                            text: "No pictures today, sorry \U0001F641.");
                                        return;
                                    }

                                    int randPic = rnd.Next(1, picturesList.Count + 1);
                                    string value = "", picName = picturesList.TryGetValue(randPic, out value) ? value : "";

                                    await using (Stream stream = System.IO.File.OpenRead(picName))
                                    {
                                        await botClient.SendPhotoAsync(
                                            chatId: update.Message.Chat,
                                            photo: stream,
                                            caption: "<b>Here is your picture, enjoy it :)</b>",
                                            parseMode: ParseMode.Html);
                                    }
                                    break;
                                case "/startconv":
                                    await CheckUsersList(botClient, update, state);
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
                        case "UploadFile":
                            state = "sending_file_for_upload";

                            SQLStuff.UpdateDB($"UPDATE users_list SET state='{state}' WHERE chatID='{update.CallbackQuery.Message.Chat.Id}'");

                            await botClient.SendTextMessageAsync(
                                chatId: update.CallbackQuery.Message.Chat,
                                text: "Send me the file you want to upload to the server, please.",
                                replyMarkup: CancelKeyboardButton());
                            break;
                        case "SeeFilesList":
                        case "DownloadFile":
                        case "DeleteFile":
                        case "DeleteAllFiles":
                            await CheckFilesList(botClient, update, state);
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }
        else
        {
            return;
        }
    }
    #endregion
}
