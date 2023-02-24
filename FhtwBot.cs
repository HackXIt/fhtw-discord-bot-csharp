using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Fluent;

namespace bic_fhtw_discord_bot;

public class FhtwBot
{
    private NLogLoggerFactory _loggerFactory;
    private DiscordConfiguration _discordConfiguration;
    public ILogger<FhtwBot> Logger;

    public FhtwBot(NLogLoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _discordConfiguration = new DiscordConfiguration()
        {
            Token = File.ReadAllText("token.txt"),
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged,
            LoggerFactory = loggerFactory
        };
        Logger = _loggerFactory.CreateLogger<FhtwBot>();
    }

    public async Task MainAsync()
    {
        var client = new DiscordClient(_discordConfiguration);

        // client.MessageReactionAdded += ReactionAddedHandler;

        await client.ConnectAsync();
        await Task.Delay(-1);
    }
}