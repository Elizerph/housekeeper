using Newtonsoft.Json.Linq;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace HouseKeeper.Core;
public class BotDialog
{
    private readonly ITelegramBotClient _botClient;
    private readonly long _chatId;

    public long UserId { get; }

    public BotDialog(ITelegramBotClient botClient, long chatId, long userId)
    {
        _botClient = botClient;
        _chatId = chatId;
        UserId = userId;
    }

    public async Task<int> Send(string text, IEnumerable<MessageButton> buttons = null)
    {
        var keyboardButtons = buttons?.Select(x => new[]
        {
            new InlineKeyboardButton(x.Label)
            {
                CallbackData = x.Data
            }
        });
        var replyMarkup = keyboardButtons == null
            ? null
            : new InlineKeyboardMarkup(keyboardButtons);
        var message = await _botClient.SendMessage(_chatId, text, replyMarkup: replyMarkup);
        return message.MessageId;
    }

    public async Task SendCsvFile(string caption, string fileName, IAsyncEnumerable<IEnumerable<string>> content)
    {
        await SendTextFile(caption, fileName, content.Select(x => string.Join(",", x.Select(y => $"\"{y.Replace("\"", "\"\"")}\""))));
    }

    public async Task SendTextFile(string caption, string fileName, IAsyncEnumerable<string> content)
    {
        await using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream);
        await foreach (var values in content)
        {
            await streamWriter.WriteLineAsync(values);
        }
        await streamWriter.FlushAsync();
        memoryStream.Position = 0;
        var inputFile = InputFile.FromStream(memoryStream, fileName);
        await _botClient.SendDocument(_chatId, inputFile, caption);
    }

    public async Task ClearKeyboard(int messageId)
    {
        await _botClient.EditMessageReplyMarkup(_chatId, messageId);
    }

    public async Task EditText(int messageId, string newText)
    {
        await _botClient.EditMessageText(_chatId, messageId, newText);
    }

    public async Task DowloadFile(string fileId, Stream destination)
    {
        await _botClient.GetInfoAndDownloadFile(fileId, destination);
    }
}
