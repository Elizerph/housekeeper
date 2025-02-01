using HouseKeeper.Contexts;
using HouseKeeper.Core.States;

using log4net;

using System.Reflection;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HouseKeeper.Core;
public class UpdateHandler : IUpdateHandler
{
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly object _syncRoot = new();
    private readonly Dictionary<long, IState> _userStates = [];
    private readonly IApplicationContextFactory _applicationContextFactory;

    public UpdateHandler(IApplicationContextFactory applicationContextFactory)
    {
        _applicationContextFactory = applicationContextFactory;
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.Error("Error", exception);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                var message = update.Message;
                var documentId = message.Document?.FileId;
                if (!string.IsNullOrEmpty(documentId))
                {
                    var user = message.From;
                    var state = GetUserState(user.Id, botClient, message.Chat);
                    var newState = await state.InputDocument(documentId);
                    _userStates[user.Id] = newState;
                }
                else
                {
                    var messageText = message?.Text;
                    if (!string.IsNullOrEmpty(messageText))
                    {
                        var user = message.From;
                        var state = GetUserState(user.Id, botClient, message.Chat);
                        var newState = await state.InputText(messageText);
                        _userStates[user.Id] = newState;
                    }
                }
                break;
            case UpdateType.CallbackQuery:
                var query = update.CallbackQuery;
                var queryMessage = query.Message;
                var queryUser = query.From;
                var queryState = GetUserState(queryUser.Id, botClient, queryMessage.Chat);
                var newQueryState = await queryState.InputButton(queryMessage.Id, query.Data);
                _userStates[queryUser.Id] = newQueryState;
                break;
            default:
                break;
        }
    }

    private IState GetUserState(long userId, ITelegramBotClient bot, Chat chat)
    {
        lock (_syncRoot)
        {
            if (_userStates.TryGetValue(userId, out var state))
                return state;
            var dialog = new BotDialog(bot, chat.Id, userId);
            var newState = new StartState(dialog, _applicationContextFactory);
            _userStates[userId] = newState;
            return newState;
        }
    }
}
