using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BIC_FHTW.DiscordBot.Handler;

public class ReactionHandler : IHandler
{
    private Emoji PushpinEmoji = new("\ud83d\udccc");   // 📌
    private Emoji NoEntryEmoji = new("\u26d4");         // ⛔
    private readonly DiscordSocketClient _client;
    private readonly BotSettings _settings;
    private readonly ILogger<BotService> _logger;
    
    private IServiceScope? _scope;

    public ReactionHandler(DiscordSocketClient client, BotSettings settings, ILogger<BotService> logger)
    {
        _client = client;
        _settings = settings;
        _logger = logger;
    }

    internal void Initialize(IServiceScope scope)
    {
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));

        BindEvents();
    }
    
    public void BindEvents()
    {
        _client.ReactionAdded += HandleReactionAddedAsync;
        _client.ReactionRemoved += HandleReactionRemovedAsync;
    }

    public void UnbindEvents()
    {
        _client.ReactionAdded -= HandleReactionAddedAsync;
        _client.ReactionRemoved -= HandleReactionRemovedAsync;
    }
    
    private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> cacheable, SocketReaction reaction)
    {
        if (reaction.User.Value.IsBot)
            return;

        switch (reaction.Emote.Name)
        {
            case "\ud83d\udccc":    // 📌
                if(message.GetOrDownloadAsync().Result.IsPinned)
                    return;
                await message.GetOrDownloadAsync().Result.AddReactionAsync(PushpinEmoji);
                await message.GetOrDownloadAsync().Result.PinAsync();
                break;
            case "\u26d4":          // ⛔
                if (!_settings.OwnerId.Equals(reaction.User.Value.Id))
                {
                    return;
                }
                await message.GetOrDownloadAsync().Result.RemoveAllReactionsForEmoteAsync(PushpinEmoji);
                await message.GetOrDownloadAsync().Result.RemoveAllReactionsForEmoteAsync(NoEntryEmoji);
                await message.GetOrDownloadAsync().Result.UnpinAsync();
                break;
        }
    }

    private async Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> cacheable, SocketReaction reaction)
    {
        if (reaction.User.Value.IsBot)
            return;

        switch (reaction.Emote.Name)
        {
            case "\ud83d\udccc":    // 📌
                if (!message.GetOrDownloadAsync().Result.IsPinned)
                    return;
                // Unpin message if last user unpinned it
                if (message.GetOrDownloadAsync().Result.Reactions[PushpinEmoji].ReactionCount == 1)
                {
                    await message.GetOrDownloadAsync().Result.UnpinAsync();
                    await message.GetOrDownloadAsync().Result.RemoveReactionAsync(PushpinEmoji, _client.CurrentUser.Id);
                }
                break;
        }
    }
}