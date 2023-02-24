using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Fluent;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace bic_fhtw_discord_bot;

public class FhtwBot
{
    private NLogLoggerFactory _loggerFactory;
    private DiscordClient _discordClient;
    private DiscordConfiguration _discordConfiguration;
    public ILogger<FhtwBot> Logger;
    public List<DiscordEmoji> PossibleBotReactions;

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
        _discordClient = new DiscordClient(_discordConfiguration);
        PossibleBotReactions = new List<DiscordEmoji>
        {
            DiscordEmoji.FromName(_discordClient, BotEmojis.Pin),
            DiscordEmoji.FromName(_discordClient, BotEmojis.RemovePin)
        };
    }

    public async Task MainAsync()
    {
        _discordClient.MessageReactionAdded += ReactionAddedHandler;

        await _discordClient.ConnectAsync();
        await Task.Delay(-1);
    }
    
    private async Task ReactionAddedHandler(DiscordClient client, MessageReactionAddEventArgs eventArgs)
    {
        if (!PossibleBotReactions.Contains(eventArgs.Emoji))  {return;}
        if (eventArgs.Emoji.Equals(DiscordEmoji.FromName(client, BotEmojis.Pin)))
        {
            // await client.SendMessageAsync(eventArgs.Channel, $"Pin!");
            if (!eventArgs.Message.Pinned)
            {
                Logger.Log(LogLevel.Debug, "Pin {messageId} in {channelId}.", eventArgs.Message.Id, eventArgs.Channel.Id);
                await eventArgs.Message.PinAsync();
                await eventArgs.Message.CreateReactionAsync(DiscordEmoji.FromName(client, BotEmojis.Pin));
            }
        } else if (eventArgs.Emoji.Equals(DiscordEmoji.FromName(client, BotEmojis.RemovePin)))
        {
            var pinReactionCount = eventArgs.Message.Reactions.Count(reaction =>
                reaction.Emoji.Equals(DiscordEmoji.FromName(client, BotEmojis.Pin))) - 1; // Minus 1 because of self-reaction
            if (eventArgs.Message.Pinned && pinReactionCount <= 1)
            {
                Logger.Log(LogLevel.Debug, "Unpin {messageId} in {channelId}.", eventArgs.Message.Id, eventArgs.Channel.Id);
                await eventArgs.Message.UnpinAsync();
                await eventArgs.Message.DeleteReactionsEmojiAsync(DiscordEmoji.FromName(client, BotEmojis.Pin));
                await eventArgs.Message.DeleteReactionsEmojiAsync(DiscordEmoji.FromName(client, BotEmojis.RemovePin));

            }
            else
            {
                // FIXME This is a really bad magic number implementation
                Logger.Log(LogLevel.Information, "Cannot unpin {messageId} in {channelId}.", eventArgs.Message.Id, eventArgs.Channel.Id);
                if (pinReactionCount > 2) // Greater than 2 because of user-reaction + self-reaction
                {
                    // Minus 1 because of user-reaction + self-reaction
                    await eventArgs.Channel.SendMessageAsync(
                        $"Cannot unpin message. There are still {pinReactionCount-1} others who want to have it pinned.");
                }
                await eventArgs.Message.DeleteReactionsEmojiAsync(DiscordEmoji.FromName(client, BotEmojis.RemovePin));
            }
        }
    }
}