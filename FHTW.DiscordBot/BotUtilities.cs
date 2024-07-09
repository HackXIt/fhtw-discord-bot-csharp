using System;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using CommandRunMode = Discord.Commands.RunMode;
using InteractionRunMode = Discord.Interactions.RunMode;

namespace BIC_FHTW.DiscordBot;

public static class BotUtilities
{
    public static DiscordSocketClient CreateDicordWebsocketClient(BotSettings botSettings)
    {
        return new DiscordSocketClient(new DiscordSocketConfig
        {
            AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
            DefaultRetryMode = RetryMode.AlwaysRetry,
            MessageCacheSize = botSettings.MessageCacheSize >= 0 ?
                botSettings.MessageCacheSize :
                throw new Exception($"{nameof(botSettings.MessageCacheSize)} must be set to a non negative integer."),
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
#if DEBUG
            LogLevel = LogSeverity.Debug
#else
                LogLevel = LogSeverity.Warning
#endif
        });
    }

    public static CommandService CreateCommandService(BotSettings botSettings)
    {
        return new CommandService(new CommandServiceConfig
        {
            DefaultRunMode = CommandRunMode.Async,
            CaseSensitiveCommands = botSettings.CaseSensitiveComands,
            SeparatorChar = ' ',

#if DEBUG
            LogLevel = LogSeverity.Debug
#else
            LogLevel = LogSeverity.Warning
#endif
        });
    }

    public static InteractionService CreateInteractionService(DiscordSocketClient client, BotSettings botSettings)
    {
        return new InteractionService(client, new InteractionServiceConfig()
        {
            DefaultRunMode = InteractionRunMode.Async,
            EnableAutocompleteHandlers = botSettings.AutocompleteHandlers,
#if DEBUG
            LogLevel = LogSeverity.Debug
#else
            LogLevel = LogSeverity.Warning
#endif
        });
    }
}