using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BIC_FHTW.DiscordBot.Handler;

public class BotHandler : IHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly BotSettings _settings;
    private readonly ILogger<BotService> _logger;

    private IServiceScope? _scope;

    public BotHandler(DiscordSocketClient client, CommandService commands, BotSettings settings, ILogger<BotService> logger)
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

    public void BindEvents()
    {
        _client.Ready += HandleReady;
        _client.Log += LogAsync;
    }

    public void UnbindEvents()
    {
        _client.Ready -= HandleReady;
        _client.Log -= LogAsync;
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
}