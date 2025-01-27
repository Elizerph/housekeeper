using HouseKeeper.Core;

using log4net;

using System.Reflection;

using Telegram.Bot;
using Telegram.Bot.Types;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]
namespace HouseKeeper;

internal class Program
{
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private const string BotTokenVariableName = "bottoken";

    private static async Task Main(string[] args)
    {
        var botToken = Environment.GetEnvironmentVariable(BotTokenVariableName);
        if (string.IsNullOrEmpty(botToken))
        {
            logger.Error("Cannot read token");
            return;
        }

        using var cts = new CancellationTokenSource();
        var bot = new TelegramBotClient(botToken, cancellationToken: cts.Token);
        var commands = new[]
        { 
            new BotCommand
            { 
                Command = "manage",
                Description = "Manage your datasets"
            },
            new BotCommand
            { 
                Command = "observation",
                Description = "Make an observation into specified dataset"
            },
            new BotCommand
            { 
                Command = "report",
                Description = "Observations from specified dataset"
            }
        };
        await bot.SetMyCommands(commands);
        var updateHandler = new UpdateHandler();
        await bot.ReceiveAsync(updateHandler);
    }
}
