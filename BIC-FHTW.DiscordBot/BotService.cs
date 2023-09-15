using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BIC_FHTW.DiscordBot.Handler;
using BIC_FHTW.Scraper.Services;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BIC_FHTW.DiscordBot;

public class BotService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<BotService> _logger;
    private readonly IScraperService _scraperService;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly InteractionService _interactions;
    private readonly BotSettings _settings;
    private readonly InteractionHandler _interactionHandler;
    private readonly CommandHandler _commandHandler;
    private readonly ReactionHandler _reactionHandler;

    private IServiceScope? _scope;

    public BotService(IServiceProvider? provider,
                      ILogger<BotService> logger,   
                      IScraperService? scraperService,
                   DiscordSocketClient? client,
                   CommandService? commands,
                   InteractionService? interactions,
                   BotSettings? botConfig
                   )
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger;
        _scraperService = scraperService ?? throw new ArgumentNullException(nameof(scraperService));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _interactions = interactions ?? throw new ArgumentNullException(nameof(interactions));
        _settings = botConfig ?? throw new ArgumentNullException(nameof(botConfig));

        _commandHandler = new CommandHandler(_client, _commands, _settings, _logger);
        _interactionHandler = new InteractionHandler(_client, _interactions, _settings, _logger);
        _reactionHandler = new ReactionHandler(_client, _settings, _logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
        Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

        _logger.LogInformation("Setting up discord bot for execution");

        IServiceScope? scope = default;
        try
        {
            _scope = _provider.CreateScope();

            _commandHandler.Initialize(_scope);
            _interactionHandler.Initialize(_scope);
            _reactionHandler.Initialize(_scope);

            await _commands.AddModulesAsync(typeof(BotService).Assembly, _scope.ServiceProvider);

            await StartClientAsync(stoppingToken);
        }
        catch (Exception ex) when (!(ex is TaskCanceledException))
        {
            try
            {
                await _client.LogoutAsync();
            }
            finally
            {
                await _client.DisposeAsync();
                scope?.Dispose();
            }

            _logger.LogError(ex, "An exception was triggered during set-up.");

            throw;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _client.StopAsync();

        return base.StopAsync(cancellationToken);
    }

    private async Task StartClientAsync(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _client.LoginAsync(TokenType.Bot, _settings.BotToken);
            await _client.StartAsync();
            _logger.LogInformation("Discord bot is now running.");
        }
        catch (Exception e)
        {
            _logger.LogError($"Could not connect to Discord. Exception: {e.Message}");
            throw;
        }
    }
    public override void Dispose()
    {
        _client.Dispose();
        _scope?.Dispose();
        ((IDisposable)_commands).Dispose();
        ((IDisposable)_interactions).Dispose();

        base.Dispose();
    }
}