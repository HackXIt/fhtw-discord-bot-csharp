using System;
using System.Threading.Tasks;
using BIC_FHTW.DiscordBot.Attributes;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BIC_FHTW.DiscordBot;

internal class CommandHandler
{
    private Emoji PushpinEmoji = new("\ud83d\udccc");   // 📌
    private Emoji NoEntryEmoji = new("\u26d4");         // ⛔
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly BotSettings _settings;
    private readonly ILogger<BotService> _logger;

    private IServiceScope? _scope;

    public CommandHandler(DiscordSocketClient client, CommandService commands, BotSettings settings, ILogger<BotService> logger)
    {
        _client = client;
        _commands = commands;
        _settings = settings;
        _logger = logger;
    }

    internal void Initialize(IServiceScope scope)
    {
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));

        BindEvents();
    }

    private void BindEvents()
    {
        _client.Ready += HandleReady;
        _client.Log += LogAsync;
        _client.MessageReceived += HandleCommandAsync;
        _client.ReactionAdded += HandleReactionAddedAsync;
    }

    internal void UnbindEvents()
    {
        _client.Ready -= HandleReady;
        _client.Log -= LogAsync;
        _client.MessageReceived -= HandleCommandAsync;
        _client.ReactionAdded -= HandleReactionAddedAsync;
    }

    private Task LogAsync(LogMessage log)
    {
        _logger.LogInformation(log.Message);
        return Task.CompletedTask;
    }

    private async Task HandleReady()
    {
        await _client.SetActivityAsync(new Game($"BIC-FHTW | {_settings.Prefix}"));
    }

    private async Task HandleCommandAsync(SocketMessage message)
    {
        // only allow users to make use of commands
        if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot)
            return;

        int argumentPosition = 0;
        
        _logger.LogDebug($"Received user message: {userMessage.Content} by {userMessage.Author.Id}");

        // determines if the message has the determined prefix
        if (!userMessage.HasStringPrefix(_settings.Prefix, ref argumentPosition) 
            && !(_settings.UseMentionPrefix && userMessage.HasMentionPrefix(_client.CurrentUser, ref argumentPosition)))
            return;

        // set up the context used for commands
        var context = new SocketCommandContext(_client, userMessage);

        // execute the command
        var result = await _commands.ExecuteAsync(context, argumentPosition, _scope!.ServiceProvider);

        if (!result.IsSuccess && result.Error != CommandError.UnknownCommand && !string.IsNullOrWhiteSpace(result.ErrorReason))
            await context.Channel.SendMessageAsync(result.ErrorReason);
    }
    
    private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> cacheable, SocketReaction reaction)
    {
        if (reaction.User.Value.IsBot)
            return;

        switch (reaction.Emote.Name)
        {
            case "\ud83d\udccc":    // 📌
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
}