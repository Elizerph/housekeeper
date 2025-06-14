using HouseKeeper.Contexts;
using HouseKeeper.Core;

using Microsoft.Extensions.Configuration;

using Telegram.Bot;
using Telegram.Bot.Types;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]
namespace HouseKeeper;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        var botToken = configuration.GetValue<string>("bottoken");
        var connectionString = configuration.GetValue<string>("dbconnection");
        var applicationContextFactory = new ApplicationContextFactory(connectionString);
        var updateHandler = new UpdateHandler(applicationContextFactory);

        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
        using var cts = new CancellationTokenSource();
        var bot = new TelegramBotClient(botToken, httpClient, cts.Token);
        var commands = new[]
        {
            new BotCommand
            {
                Command = "observation",
                Description = "Make an observation into specified dataset"
            },
            new BotCommand
            {
                Command = "report",
                Description = "Observations from specified dataset"
            },
            new BotCommand
            {
                Command = "manage",
                Description = "Manage your datasets"
            }
        };
        await bot.SetMyCommands(commands, cancellationToken: cts.Token);
        await bot.ReceiveAsync(updateHandler, cancellationToken: cts.Token);
    }
}
