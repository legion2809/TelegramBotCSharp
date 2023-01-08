namespace TelegramBot;

internal partial class HandleUpdatesAndErrors
{
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

    // Handling Telegram updates
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

                    await HelperMethodsAndFuncs.ProcessingStates(state, botClient, update);
                    await HelperMethodsAndFuncs.ProcessingMessages(botClient, update, state);
                }
                break;
            case UpdateType.CallbackQuery:
                string userState = SQLStuff.ReadDBRecords($"SELECT state FROM users_list WHERE chatID='{update.CallbackQuery.Message.Chat.Id}'");
                await HelperMethodsAndFuncs.ProcessingStates(userState, botClient, update);
                await HelperMethodsAndFuncs.ProcessingMessages(botClient, update, userState);
                break;
            default:
                break;
        }
    }
}

