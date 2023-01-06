namespace TelegramBot;

internal partial class Logger
{
    public static void WriteLog(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                var message = update.Message;
                switch (message.Type)
                {
                    case MessageType.Text:
                        Console.WriteLine($"[{DateTime.Now}] User '{message.Chat.FirstName}' (ID: {message.Chat.Id}) has sent the following message: '{message.Text}'");
                        break;
                    case MessageType.Photo:
                        Console.WriteLine($"[{DateTime.Now}] User '{message.Chat.FirstName}' (ID: {message.Chat.Id}) has sent the picture with ID: '{message.Photo.Last().FileUniqueId}'");
                        break;
                    case MessageType.Document:
                        Console.WriteLine($"[{DateTime.Now}] User '{message.Chat.FirstName}' (ID: {message.Chat.Id}) has sent the document with name: '{message.Document.FileName}'");
                        break;
                    case MessageType.Video:
                        Console.WriteLine($"[{DateTime.Now}] User '{message.Chat.FirstName}' (ID: {message.Chat.Id}) has sent the video with name: '{message.Video.FileName}'");
                        break;
                    case MessageType.Voice:
                        Console.WriteLine($"[{DateTime.Now}] User '{message.Chat.FirstName}' (ID: {message.Chat.Id}) has sent the voice message with name: '{message.Voice.FileId}'");
                        break;
                    case MessageType.Audio:
                        Console.WriteLine($"[{DateTime.Now}] User '{message.Chat.FirstName}' (ID: {message.Chat.Id}) has sent the audio with name: '{message.Audio.FileName}'");
                        break;
                    case MessageType.Sticker:
                        Console.WriteLine($"[{DateTime.Now}] User '{message.Chat.FirstName}' (ID: {message.Chat.Id}) has sent the sticker with ID: '{message.Sticker.FileId}'");
                        break;
                    case MessageType.VideoNote:
                        Console.WriteLine($"[{DateTime.Now}] User '{message.Chat.FirstName}' (ID: {message.Chat.Id}) has sent a video note with ID: '{message.VideoNote.FileId}'");
                        break;
                    default:
                        Console.WriteLine($"[{DateTime.Now}] User '{message.Chat.FirstName}' (ID: {message.Chat.Id}) has sent the message with type: '{message.Type}'");
                        break;
                }
                break;
            case UpdateType.CallbackQuery:
                var callbackData = update.CallbackQuery.Data;
                Console.WriteLine($"[{DateTime.Now}] User '{update.CallbackQuery.Message.Chat.FirstName}' (ID: {update.CallbackQuery.Message.Chat.Id}) has pressed the button with ID: '{callbackData}'");
                break;
            default:
                Console.WriteLine($"[{DateTime.Now}] An update came with following type -> '{update.Type}' from this user: '{update.Message.Chat.FirstName}' (ID: {update.Message.Chat.Id})");
                break;
        }
    }
}

